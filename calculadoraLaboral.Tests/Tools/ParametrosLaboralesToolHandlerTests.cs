using System.Text.Json;
using CalculadoraLaboral.McpServer.Tools;

namespace CalculadoraLaboral.Tests.Tools;

public class ParametrosLaboralesToolHandlerTests
{
    private readonly ParametrosLaboralesToolHandler _handler = new();

    [Fact]
    public void Name_DebeRetornarNombreCorrect()
    {
        Assert.Equal("obtener_parametros_laborales", _handler.Name);
    }

    [Fact]
    public void Description_DebeRetornarDescripcionCompleta()
    {
        var expectedDescription = "Obtiene el salario mínimo legal vigente y el auxilio de transporte para un año específico. Por defecto usa el año actual (2025).";
        Assert.Equal(expectedDescription, _handler.Description);
    }

    [Fact]
    public void Schema_DebeContenerPropiedadesCorrectas()
    {
        var schema = _handler.Schema;
        var schemaJson = JsonSerializer.Serialize(schema);
        var schemaElement = JsonSerializer.Deserialize<JsonElement>(schemaJson);

        Assert.True(schemaElement.TryGetProperty("type", out var typeProperty));
        Assert.Equal("object", typeProperty.GetString());

        Assert.True(schemaElement.TryGetProperty("properties", out var propertiesProperty));
        Assert.True(propertiesProperty.TryGetProperty("anio", out var anioProperty));
        
        Assert.True(anioProperty.TryGetProperty("type", out var anioType));
        Assert.Equal("number", anioType.GetString());
        
        Assert.True(anioProperty.TryGetProperty("minimum", out var minimumProperty));
        Assert.Equal(2022, minimumProperty.GetInt32());
        
        Assert.True(anioProperty.TryGetProperty("maximum", out var maximumProperty));
        Assert.Equal(2026, maximumProperty.GetInt32());
    }

    [Theory]
    [InlineData(2022, 1_000_000, 117_172, 240)]  // Dic 31, 2022: antes de jul 15, 2023
    [InlineData(2023, 1_160_000, 140_606, 235)]  // Dic 31, 2023: después de jul 15, 2023, antes de jul 15, 2024
    [InlineData(2024, 1_300_000, 162_000, 230)]  // Dic 31, 2024: después de jul 15, 2024, antes de jul 15, 2025
    [InlineData(2025, 1_423_500, 200_000, 220)]  // Dic 31, 2025: después de jul 15, 2025, antes de jul 15, 2026
    [InlineData(2026, 1_423_500, 200_000, 210)]  // Dic 31, 2026: después de jul 15, 2026
    public async Task HandleAsync_ConAnioEspecífico_DebeRetornarParametrosCorrectos(
        int anio, 
        decimal salarioMinimoEsperado, 
        decimal auxilioTransporteEsperado, 
        int horasJornadaEsperadas)
    {
        // Arrange
        var arguments = JsonSerializer.Deserialize<JsonElement>($"{{\"año\": {anio}}}");

        // Act
        var result = await _handler.HandleAsync(arguments);
        var resultString = result.ToString();
        var resultJson = JsonSerializer.Deserialize<JsonElement>(resultString!);

        // Assert
        Assert.True(resultJson.TryGetProperty("success", out var successProperty));
        Assert.True(successProperty.GetBoolean());

        Assert.True(resultJson.TryGetProperty("anio", out var anioProperty));
        Assert.Equal(anio, anioProperty.GetInt32());

        Assert.True(resultJson.TryGetProperty("data", out var dataProperty));
        
        Assert.True(dataProperty.TryGetProperty("salarioMinimoLegalVigente", out var salarioMinimoProperty));
        Assert.Equal(salarioMinimoEsperado, salarioMinimoProperty.GetDecimal());

        Assert.True(dataProperty.TryGetProperty("auxilioTransporte", out var auxilioTransporteProperty));
        Assert.Equal(auxilioTransporteEsperado, auxilioTransporteProperty.GetDecimal());

        Assert.True(dataProperty.TryGetProperty("horasJornadaMensual", out var horasJornadaProperty));
        Assert.Equal(horasJornadaEsperadas, horasJornadaProperty.GetInt32());

        Assert.True(dataProperty.TryGetProperty("salarioIntegralMinimo", out var salarioIntegralProperty));
        Assert.Equal(salarioMinimoEsperado * 13, salarioIntegralProperty.GetDecimal());

        Assert.True(dataProperty.TryGetProperty("valorHoraOrdinaria", out var valorHoraProperty));
        Assert.Equal(salarioMinimoEsperado / horasJornadaEsperadas, valorHoraProperty.GetDecimal());
    }

    [Fact]
    public async Task HandleAsync_SinArgumentos_DebeUsarAnioActual()
    {
        // Arrange
        var arguments = JsonSerializer.Deserialize<JsonElement>("{}");
        var anioActual = DateTime.Now.Year;

        // Act
        var result = await _handler.HandleAsync(arguments);
        var resultString = result.ToString();
        var resultJson = JsonSerializer.Deserialize<JsonElement>(resultString!);

        // Assert
        Assert.True(resultJson.TryGetProperty("success", out var successProperty));
        Assert.True(successProperty.GetBoolean());

        Assert.True(resultJson.TryGetProperty("anio", out var anioProperty));
        Assert.Equal(anioActual, anioProperty.GetInt32());
    }

    [Fact]
    public async Task HandleAsync_ConAnioInválido_DebeRetornarError()
    {
        // Arrange
        var arguments = JsonSerializer.Deserialize<JsonElement>("{\"año\": 2021}");

        // Act
        var result = await _handler.HandleAsync(arguments);
        var resultString = result.ToString();
        var resultJson = JsonSerializer.Deserialize<JsonElement>(resultString!);

        // Assert
        Assert.True(resultJson.TryGetProperty("error", out var errorProperty));
        Assert.True(errorProperty.GetBoolean());

        Assert.True(resultJson.TryGetProperty("message", out var messageProperty));
        Assert.Contains("No se encontraron parámetros laborales para el año 2021", messageProperty.GetString());
        Assert.Contains("Años disponibles: 2022-2026", messageProperty.GetString());
    }

    [Fact]
    public async Task HandleAsync_DebeIncluirDescripcionesEnRespuesta()
    {
        // Arrange
        var arguments = JsonSerializer.Deserialize<JsonElement>("{\"año\": 2025}");

        // Act
        var result = await _handler.HandleAsync(arguments);
        var resultString = result.ToString();
        var resultJson = JsonSerializer.Deserialize<JsonElement>(resultString!);

        // Assert
        Assert.True(resultJson.TryGetProperty("data", out var dataProperty));
        Assert.True(dataProperty.TryGetProperty("descripcion", out var descripcionProperty));

        Assert.True(descripcionProperty.TryGetProperty("salarioMinimo", out var salarioMinimoDescProperty));
        Assert.Equal("Salario Mínimo Legal Vigente (SMLV) en pesos colombianos", salarioMinimoDescProperty.GetString());

        Assert.True(descripcionProperty.TryGetProperty("auxilioTransporte", out var auxilioTransporteDescProperty));
        Assert.Equal("Auxilio de transporte mensual en pesos colombianos", auxilioTransporteDescProperty.GetString());

        Assert.True(descripcionProperty.TryGetProperty("horasJornada", out var horasJornadaDescProperty));
        Assert.Equal("Cantidad de horas de la jornada laboral mensual", horasJornadaDescProperty.GetString());

        Assert.True(descripcionProperty.TryGetProperty("salarioIntegral", out var salarioIntegralDescProperty));
        Assert.Equal("Salario integral mínimo (13 SMLV)", salarioIntegralDescProperty.GetString());

        Assert.True(descripcionProperty.TryGetProperty("valorHora", out var valorHoraDescProperty));
        Assert.Equal("Valor de la hora ordinaria de trabajo", valorHoraDescProperty.GetString());
    }

    [Fact]
    public async Task HandleAsync_RespuestaDebeSerJsonBienFormateado()
    {
        // Arrange
        var arguments = JsonSerializer.Deserialize<JsonElement>("{\"año\": 2025}");

        // Act
        var result = await _handler.HandleAsync(arguments);
        var resultString = result.ToString();

        // Assert
        Assert.NotNull(resultString);
        Assert.True(resultString.Contains("{\r\n") || resultString.Contains("{\n")); // Verificar que está indentado
        
        // Verificar que es JSON válido
        var resultJson = JsonSerializer.Deserialize<JsonElement>(resultString);
        Assert.True(resultJson.ValueKind == JsonValueKind.Object);
    }

    [Theory]
    [InlineData(2025, 1_423_500)]
    [InlineData(2024, 1_300_000)]
    [InlineData(2023, 1_160_000)]
    public async Task HandleAsync_CalculoSalarioIntegral_DebeSerCorrect(int anio, decimal salarioMinimo)
    {
        // Arrange
        var arguments = JsonSerializer.Deserialize<JsonElement>($"{{\"año\": {anio}}}");

        // Act
        var result = await _handler.HandleAsync(arguments);
        var resultString = result.ToString();
        var resultJson = JsonSerializer.Deserialize<JsonElement>(resultString!);

        // Assert
        Assert.True(resultJson.TryGetProperty("data", out var dataProperty));
        Assert.True(dataProperty.TryGetProperty("salarioIntegralMinimo", out var salarioIntegralProperty));
        
        var salarioIntegralEsperado = salarioMinimo * 13;
        Assert.Equal(salarioIntegralEsperado, salarioIntegralProperty.GetDecimal());
    }

    [Theory]
    [InlineData(2025, 1_423_500, 220)]
    [InlineData(2024, 1_300_000, 230)]
    [InlineData(2023, 1_160_000, 235)]
    public async Task HandleAsync_CalculoValorHora_DebeSerCorrect(int año, decimal salarioMinimo, int horasJornada)
    {
        // Arrange
        var arguments = JsonSerializer.Deserialize<JsonElement>($"{{\"año\": {año}}}");

        // Act
        var result = await _handler.HandleAsync(arguments);
        var resultString = result.ToString();
        var resultJson = JsonSerializer.Deserialize<JsonElement>(resultString!);

        // Assert
        Assert.True(resultJson.TryGetProperty("data", out var dataProperty));
        Assert.True(dataProperty.TryGetProperty("valorHoraOrdinaria", out var valorHoraProperty));
        
        var valorHoraEsperado = salarioMinimo / horasJornada;
        Assert.Equal(valorHoraEsperado, valorHoraProperty.GetDecimal());
    }

    [Fact]
    public async Task HandleAsync_ConArgumentosIncorrectos_DebeRetornarError()
    {
        // Arrange - tratar de pasar un string como año cuando se espera número
        var arguments = JsonSerializer.Deserialize<JsonElement>("{\"año\": \"texto_inválido\"}");

        // Act
        var result = await _handler.HandleAsync(arguments);
        var resultString = result.ToString();
        var resultJson = JsonSerializer.Deserialize<JsonElement>(resultString!);

        // Assert
        Assert.True(resultJson.TryGetProperty("error", out var errorProperty));
        Assert.True(errorProperty.GetBoolean());
        
        Assert.True(resultJson.TryGetProperty("message", out var messageProperty));
        Assert.NotNull(messageProperty.GetString());
    }

    [Fact]
    public async Task HandleAsync_ConAño2023_DebeRetornar2023NoAñoActual()
    {
        // Arrange - Este test replica exactamente tu caso de uso
        var arguments = JsonSerializer.Deserialize<JsonElement>("{\"año\": 2023}");

        // Act
        var result = await _handler.HandleAsync(arguments);
        var resultString = result.ToString();
        var resultJson = JsonSerializer.Deserialize<JsonElement>(resultString!);

        // Assert - Verificar que el año devuelto es 2023, no el año actual (2025)
        Assert.True(resultJson.TryGetProperty("anio", out var anioProperty));
        Assert.Equal(2023, anioProperty.GetInt32());
        
        // Verificar que los datos corresponden a 2023
        Assert.True(resultJson.TryGetProperty("data", out var dataProperty));
        Assert.True(dataProperty.TryGetProperty("salarioMinimoLegalVigente", out var salarioProperty));
        Assert.Equal(1_160_000, salarioProperty.GetDecimal());
        
        Assert.True(dataProperty.TryGetProperty("auxilioTransporte", out var auxilioProperty));
        Assert.Equal(140_606, auxilioProperty.GetDecimal());
    }
}