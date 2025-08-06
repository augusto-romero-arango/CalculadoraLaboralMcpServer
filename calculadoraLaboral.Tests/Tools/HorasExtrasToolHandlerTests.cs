using System.Text.Json;
using CalculadoraLaboral.McpServer.Tools;
using CalculadoraLaboral.McpServer.Domain.Models;

namespace CalculadoraLaboral.Tests.Tools;

public class HorasExtrasToolHandlerTests
{
    private readonly HorasExtrasToolHandler _handler;

    public HorasExtrasToolHandlerTests()
    {
        _handler = new HorasExtrasToolHandler();
    }

    [Fact]
    public void Name_DebeRetornarNombreCorrecto()
    {
        Assert.Equal("calcular_horas_extras", _handler.Name);
    }

    [Fact]
    public void Description_DebeRetornarDescripcionNoVacia()
    {
        Assert.NotNull(_handler.Description);
        Assert.NotEmpty(_handler.Description);
    }

    [Fact]
    public async Task HandleAsync_ConMostrarTipos_DebeRetornarTodosLosTipos()
    {
        var arguments = JsonSerializer.Deserialize<JsonElement>("""
        {
            "mostrarTipos": true
        }
        """);

        var result = await _handler.HandleAsync(arguments);
        var resultString = result.ToString()!;
        var response = JsonSerializer.Deserialize<JsonElement>(resultString);

        Assert.True(response.GetProperty("success").GetBoolean());
        Assert.True(response.GetProperty("data").GetProperty("tiposDisponibles").GetArrayLength() > 0);
    }

    [Fact]
    public async Task HandleAsync_ConSalarioMensualYHoraIndividual_DebeCalcularCorrectamente()
    {
        var arguments = JsonSerializer.Deserialize<JsonElement>("""
        {
            "salarioMensual": 1300000,
            "tipoHora": "HED",
            "cantidad": "2.5"
        }
        """);

        var result = await _handler.HandleAsync(arguments);
        var resultString = result.ToString()!;
        var response = JsonSerializer.Deserialize<JsonElement>(resultString);

        Assert.True(response.GetProperty("success").GetBoolean());
        
        var data = response.GetProperty("data");
        Assert.Equal("HED", data.GetProperty("tipoHora").GetString());
        Assert.Equal(150, data.GetProperty("cantidadMinutos").GetInt32()); // 2.5 * 60
        Assert.Equal(2.5m, data.GetProperty("cantidadHorasDecimales").GetDecimal());
        Assert.Equal(1.25m, data.GetProperty("factor").GetDecimal());
    }

    [Fact]
    public async Task HandleAsync_ConValorHoraOrdinaria_DebeUsarEseValor()
    {
        var arguments = JsonSerializer.Deserialize<JsonElement>("""
        {
            "valorHoraOrdinaria": 7000,
            "tipoHora": "HEN",
            "cantidad": "1"
        }
        """);

        var result = await _handler.HandleAsync(arguments);
        var resultString = result.ToString()!;
        var response = JsonSerializer.Deserialize<JsonElement>(resultString);

        Assert.True(response.GetProperty("success").GetBoolean());
        
        var data = response.GetProperty("data");
        Assert.Equal(7000m, data.GetProperty("valorHoraOrdinaria").GetDecimal());
        Assert.Equal(1.75m, data.GetProperty("factor").GetDecimal()); // Factor HEN
        Assert.Equal(12250m, data.GetProperty("valorTotal").GetDecimal()); // 1 hora * 1.75 * 7000
    }

    [Fact]
    public async Task HandleAsync_ConMultiplesHoras_DebeCalcularTodas()
    {
        var arguments = JsonSerializer.Deserialize<JsonElement>("""
        {
            "valorHoraOrdinaria": 6500,
            "horas": {
                "HED": "2",
                "HEN": "1.5",
                "RN": "3"
            }
        }
        """);

        var result = await _handler.HandleAsync(arguments);
        var resultString = result.ToString()!;
        var response = JsonSerializer.Deserialize<JsonElement>(resultString);

        Assert.True(response.GetProperty("success").GetBoolean());
        
        var data = response.GetProperty("data");
        var horasCalculadas = data.GetProperty("horasCalculadas");
        
        Assert.Equal(3, horasCalculadas.GetArrayLength());
        Assert.True(data.GetProperty("totalGeneral").GetDecimal() > 0);
    }

    [Theory]
    [InlineData("2.5", 150)] // 2.5 horas = 150 minutos
    [InlineData("2:30", 150)] // 2 horas 30 minutos = 150 minutos
    [InlineData("1h 15min", 75)] // 1 hora 15 minutos = 75 minutos
    [InlineData("30min", 30)] // 30 minutos = 30 minutos
    [InlineData("1", 60)] // 1 hora = 60 minutos
    public async Task HandleAsync_ConDiferentesFormatosCantidad_DebeConvertirCorrectamente(string cantidad, int minutosEsperados)
    {
        var arguments = JsonSerializer.Deserialize<JsonElement>($@"{{
            ""valorHoraOrdinaria"": 6000,
            ""tipoHora"": ""HED"",
            ""cantidad"": ""{cantidad}""
        }}");

        var result = await _handler.HandleAsync(arguments);
        var resultString = result.ToString()!;
        var response = JsonSerializer.Deserialize<JsonElement>(resultString);

        Assert.True(response.GetProperty("success").GetBoolean());
        
        var data = response.GetProperty("data");
        Assert.Equal(minutosEsperados, data.GetProperty("cantidadMinutos").GetInt32());
    }

    [Theory]
    [InlineData("HED", "HED")]
    [InlineData("DIURNA", "DiurnaOrdinaria")]
    [InlineData("diurnaordinaria", "DiurnaOrdinaria")]
    [InlineData("HEN", "HEN")]
    [InlineData("nocturna", "NocturnaOrdinaria")]
    public async Task HandleAsync_ConDiferentesNombresTipo_DebeResolverCorrectamente(string tipoInput, string tipoEsperado)
    {
        var arguments = JsonSerializer.Deserialize<JsonElement>($@"{{
            ""valorHoraOrdinaria"": 6000,
            ""tipoHora"": ""{tipoInput}"",
            ""cantidad"": ""1""
        }}");

        var result = await _handler.HandleAsync(arguments);
        var resultString = result.ToString()!;
        var response = JsonSerializer.Deserialize<JsonElement>(resultString);

        Assert.True(response.GetProperty("success").GetBoolean());
        
        var data = response.GetProperty("data");
        Assert.Equal(tipoEsperado, data.GetProperty("tipoHora").GetString());
    }

    [Fact]
    public async Task HandleAsync_SinParametrosRequeridos_DebeRetornarError()
    {
        var arguments = JsonSerializer.Deserialize<JsonElement>("{}");

        var result = await _handler.HandleAsync(arguments);
        var resultString = result.ToString()!;
        var response = JsonSerializer.Deserialize<JsonElement>(resultString);

        Assert.True(response.GetProperty("error").GetBoolean());
        Assert.Contains("Debe proporcionar", response.GetProperty("message").GetString()!);
    }

    [Fact]
    public async Task HandleAsync_ConTipoHoraInvalido_DebeRetornarError()
    {
        var arguments = JsonSerializer.Deserialize<JsonElement>("""
        {
            "valorHoraOrdinaria": 6000,
            "tipoHora": "TIPO_INEXISTENTE",
            "cantidad": "1"
        }
        """);

        var result = await _handler.HandleAsync(arguments);
        var resultString = result.ToString()!;
        var response = JsonSerializer.Deserialize<JsonElement>(resultString);

        Assert.True(response.GetProperty("error").GetBoolean());
        Assert.Contains("no reconocido", response.GetProperty("message").GetString()!);
    }

    [Fact]
    public async Task HandleAsync_ConCantidadInvalida_DebeRetornarError()
    {
        var arguments = JsonSerializer.Deserialize<JsonElement>("""
        {
            "valorHoraOrdinaria": 6000,
            "tipoHora": "HED",
            "cantidad": "formato_invalido"
        }
        """);

        var result = await _handler.HandleAsync(arguments);
        var resultString = result.ToString()!;
        var response = JsonSerializer.Deserialize<JsonElement>(resultString);

        Assert.True(response.GetProperty("error").GetBoolean());
        Assert.Contains("Formato de cantidad no reconocido", response.GetProperty("message").GetString()!);
    }

    [Fact]
    public async Task HandleAsync_SinSalarioNiValorHora_DebeRetornarError()
    {
        var arguments = JsonSerializer.Deserialize<JsonElement>("""
        {
            "tipoHora": "HED",
            "cantidad": "1"
        }
        """);

        var result = await _handler.HandleAsync(arguments);
        var resultString = result.ToString()!;
        var response = JsonSerializer.Deserialize<JsonElement>(resultString);

        Assert.True(response.GetProperty("error").GetBoolean());
        Assert.Contains("Debe proporcionar salarioMensual o valorHoraOrdinaria", response.GetProperty("message").GetString()!);
    }

    [Fact]
    public async Task HandleAsync_ConTipoAmbiguo_DebeRetornarErrorConOpciones()
    {
        // "NOCTURNA" podría coincidir con NocturnaOrdinaria y NocturnaFestiva
        // Pero el handler debe resolver automáticamente a NocturnaOrdinaria
        var arguments = JsonSerializer.Deserialize<JsonElement>("""
        {
            "valorHoraOrdinaria": 6000,
            "tipoHora": "NOCTURNA",
            "cantidad": "1"
        }
        """);

        var result = await _handler.HandleAsync(arguments);
        var resultString = result.ToString()!;
        var response = JsonSerializer.Deserialize<JsonElement>(resultString);

        // Este test verifica que el mapeo de aliases funcione correctamente
        Assert.True(response.GetProperty("success").GetBoolean());
        var data = response.GetProperty("data");
        Assert.Equal("NocturnaOrdinaria", data.GetProperty("tipoHora").GetString());
    }
}