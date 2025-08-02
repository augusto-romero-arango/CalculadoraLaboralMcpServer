using CalculadoraLaboral.McpServer.Domain.Models;
using CalculadoraLaboral.McpServer.Domain.Services;
using Xunit;

namespace CalculadoraLaboral.Tests;

public class HorasExtrasServiceTests
{
    [Fact]
    public void Constructor_ConValorHoraPositivo_DebeCrearInstancia()
    {
        // Arrange & Act
        var horasExtrasService = new HorasExtrasService(10_000);
        
        // Assert
        Assert.NotNull(horasExtrasService);
        Assert.Equal(0, horasExtrasService.ValorTotal);
    }
    
    [Fact]
    public void Constructor_ConValorHoraNegativo_DebeLanzarExcepcion()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new HorasExtrasService(-1_000));
        Assert.Equal("El valor de la hora no puede ser menor a uno", exception.Message);
    }
    
    [Fact]
    public void Constructor_ConValorHoraCero_DebeLanzarExcepcion()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new HorasExtrasService(0));
        Assert.Equal("El valor de la hora no puede ser menor a uno", exception.Message);
    }
    
    [Fact]
    public void Constructor_ConItemsValidos_DebeCrearInstancia()
    {
        // Arrange
        var items = new Dictionary<TiposHorasExtra, int>
        {
            { TiposHorasExtra.HED, 2 },
            { TiposHorasExtra.HEN, 3 }
        };
        // Act
        var horasExtrasService = new HorasExtrasService(10_000, items);
        // Assert
        Assert.NotNull(horasExtrasService);
        Assert.Equal(2, horasExtrasService.ObtenerCantidadHorasPorItem(TiposHorasExtra.HED));
        Assert.Equal(3, horasExtrasService.ObtenerCantidadHorasPorItem(TiposHorasExtra.HEN));
    }

    [Fact]
    public void Constructor_ConItemInvalido_DebeLanzarExcepcion()
    {
        // Arrange
        var items = new Dictionary<TiposHorasExtra, int>
        {
            { TiposHorasExtra.HED, -1 }  // Cantidad negativa
        };
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new HorasExtrasService(10_000, items));
        Assert.Equal("La cantidad de la hora no puede ser negativo", exception.Message);
    }

    [Fact]
    public void ModificarValorHoraOrdinaria_ConValorPositivo_DebeActualizarValor()
    {
        // Arrange
        var horasExtrasService = new HorasExtrasService(10_000);
        horasExtrasService.RegistrarHoraExtra(TiposHorasExtra.HED, 2);
        var valorInicial = horasExtrasService.ObtenerValorHoraPorItem(TiposHorasExtra.HED);
        
        // Act
        horasExtrasService.ModificarValorHoraOrdinaria(20_000);
        var valorFinal = horasExtrasService.ObtenerValorHoraPorItem(TiposHorasExtra.HED);
        
        // Assert
        Assert.Equal(2 * valorInicial, valorFinal);
    }
    
    [Fact]
    public void ModificarValorHoraOrdinaria_ConValorNegativo_DebeLanzarExcepcion()
    {
        // Arrange
        var horasExtrasService = new HorasExtrasService(10_000);
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => horasExtrasService.ModificarValorHoraOrdinaria(-1_000));
        Assert.Equal("El valor de la hora no puede ser menor a uno", exception.Message);
    }
    
    [Fact]
    public void ModificarValorHoraOrdinaria_ConValorCero_DebeLanzarExcepcion()
    {
        // Arrange
        var horasExtrasService = new HorasExtrasService(10_000);
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => horasExtrasService.ModificarValorHoraOrdinaria(0));
        Assert.Equal("El valor de la hora no puede ser menor a uno", exception.Message);
    }
    
    [Fact]
    public void ObtenerCantidadHorasPorItem_ConTipoRegistrado_DebeRetornarCantidad()
    {
        // Arrange
        var horasExtrasService = new HorasExtrasService(10_000);
        horasExtrasService.RegistrarHoraExtra(TiposHorasExtra.HED, 2);
        
        // Act
        var cantidad = horasExtrasService.ObtenerCantidadHorasPorItem(TiposHorasExtra.HED);
        
        // Assert
        Assert.Equal(2, cantidad);
    }

    [Fact]
    public void ObtenerCantidadHorasPorItem_ConTipoNoRegistrado_DebeRetornarCero()
    {
        // Arrange
        var horasExtrasService = new HorasExtrasService(10_000);
        
        // Act
        var cantidad = horasExtrasService.ObtenerCantidadHorasPorItem(TiposHorasExtra.HED);
        
        // Assert
        Assert.Equal(0, cantidad);
    }

    [Theory]
    [InlineData(TiposHorasExtra.HED, 1.25)]
    [InlineData(TiposHorasExtra.HEN, 1.75)]
    [InlineData(TiposHorasExtra.HEFD, 2.05)]
    [InlineData(TiposHorasExtra.HEFN, 2.55)]
    [InlineData(TiposHorasExtra.RN, 0.35)]
    [InlineData(TiposHorasExtra.RDD, 0.80)]
    [InlineData(TiposHorasExtra.RDN, 1.15)]
    [InlineData(TiposHorasExtra.RDDHC, 1.8)]
    [InlineData(TiposHorasExtra.RDNHC, 2.15)]
    [InlineData(TiposHorasExtra.RDDONC, 1.8)]
    [InlineData(TiposHorasExtra.RDNONC, 2.15)]
    [InlineData(TiposHorasExtra.DiurnaOrdinaria, 1.25)]
    [InlineData(TiposHorasExtra.DiurnaFestiva, 1.75)]
    [InlineData(TiposHorasExtra.NocturnaOrdinaria, 1.75)]
    [InlineData(TiposHorasExtra.NocturnaFestiva, 2.00)]
    [InlineData(TiposHorasExtra.RecargoNocturno, 0.35)]
    [InlineData(TiposHorasExtra.RecargoFestivo, 1.75)]
    public void ObtenerValorHoraPorItem_ParaCadaTipo_DebeCalcularValorConFactorCorrecto(TiposHorasExtra tipo, double factorEsperado)
    {
        // Arrange
        decimal valorHoraOrdinaria = 10_000;
        int cantidad = 3;
        var horasExtrasService = new HorasExtrasService(valorHoraOrdinaria);
        horasExtrasService.RegistrarHoraExtra(tipo, cantidad);
        
        // Act
        var valorCalculado = horasExtrasService.ObtenerValorHoraPorItem(tipo);
        
        // Assert
        decimal valorEsperado = Math.Round(cantidad * valorHoraOrdinaria * (decimal)factorEsperado, 0, MidpointRounding.AwayFromZero);
        Assert.Equal(valorEsperado, valorCalculado);
    }

    [Fact]
    public void RegistrarHoraExtra_ConCantidadPositiva_DebeRegistrarCorrectamente()
    {
        // Arrange
        var horasExtrasService = new HorasExtrasService(10_000);
        
        // Act
        horasExtrasService.RegistrarHoraExtra(TiposHorasExtra.HED, 2);
        horasExtrasService.RegistrarHoraExtra(TiposHorasExtra.HEN, 3);
        
        // Assert
        Assert.Equal(2, horasExtrasService.ObtenerCantidadHorasPorItem(TiposHorasExtra.HED));
        Assert.Equal(3, horasExtrasService.ObtenerCantidadHorasPorItem(TiposHorasExtra.HEN));
    }

    [Fact]
    public void RegistrarHoraExtra_ConCantidadNegativa_DebeLanzarExcepcion()
    {
        // Arrange
        var horasExtrasService = new HorasExtrasService(10_000);
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            horasExtrasService.RegistrarHoraExtra(TiposHorasExtra.HED, -1));
        Assert.Equal("La cantidad de la hora no puede ser negativo", exception.Message);
    }

    [Fact]
    public void RegistrarHoraExtra_SobrescribirHoraExistente_DebeActualizarCantidad()
    {
        // Arrange
        var horasExtrasService = new HorasExtrasService(10_000);
        horasExtrasService.RegistrarHoraExtra(TiposHorasExtra.HED, 2);
        
        // Act
        horasExtrasService.RegistrarHoraExtra(TiposHorasExtra.HED, 5);
        
        // Assert
        Assert.Equal(5, horasExtrasService.ObtenerCantidadHorasPorItem(TiposHorasExtra.HED));
    }

    [Fact]
    public void ValorTotal_SinHorasRegistradas_DebeRetornarCero()
    {
        // Arrange
        var horasExtrasService = new HorasExtrasService(10_000);
        
        // Act & Assert
        Assert.Equal(0, horasExtrasService.ValorTotal);
    }

    [Fact]
    public void ValorTotal_ConHorasRegistradas_DebeCalcularSumatoriaCorrecta()
    {
        // Arrange
        decimal valorHoraOrdinaria = 10_000;
        var horasExtrasService = new HorasExtrasService(valorHoraOrdinaria);
        horasExtrasService.RegistrarHoraExtra(TiposHorasExtra.HED, 2);
        horasExtrasService.RegistrarHoraExtra(TiposHorasExtra.HEN, 3);
        
        // Act
        var valorTotal = horasExtrasService.ValorTotal;
        
        // Assert
        decimal valorHoraExtraDiurna = Math.Round(2 * valorHoraOrdinaria * 1.25m, 0, MidpointRounding.AwayFromZero);
        decimal valorHoraExtraNocturna = Math.Round(3 * valorHoraOrdinaria * 1.75m, 0, MidpointRounding.AwayFromZero);
        Assert.Equal(valorHoraExtraDiurna + valorHoraExtraNocturna, valorTotal);
    }

    [Fact]
    public void ValorTotal_ActualizarHoras_DebeRecalcularValorTotal()
    {
        // Arrange
        decimal valorHoraOrdinaria = 10_000;
        var horasExtrasService = new HorasExtrasService(valorHoraOrdinaria);
        horasExtrasService.RegistrarHoraExtra(TiposHorasExtra.HED, 2);
        var valorInicial = horasExtrasService.ValorTotal;
        
        // Act
        horasExtrasService.RegistrarHoraExtra(TiposHorasExtra.HEN, 3);
        var valorFinal = horasExtrasService.ValorTotal;
        
        // Assert
        decimal valorHoraExtraNocturna = Math.Round(3 * valorHoraOrdinaria * 1.75m, 0, MidpointRounding.AwayFromZero);
        Assert.Equal(valorInicial + valorHoraExtraNocturna, valorFinal);
    }

    [Fact]
    public void RegistrarHoraExtra_ConTipoInvalido_DebeLanzarExcepcion()
    {
        // Arrange
        var horasExtrasService = new HorasExtrasService(10_000);
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            horasExtrasService.RegistrarHoraExtra((TiposHorasExtra)999, 1));
        Assert.Equal("El tipo de hora no es válido", exception.Message);
    }

    [Fact]
    public void Constructor_ConTipoInvalido_DebeLanzarExcepcion()
    {
        // Arrange
        var items = new Dictionary<TiposHorasExtra, int>
        {
            { (TiposHorasExtra)999, 1 }
        };
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new HorasExtrasService(10_000, items));
        Assert.Equal("El tipo de hora no es válido", exception.Message);
    }

    [Fact]
    public void ValorTotal_ConTodosLosTipos_DebeSumarCorrectamente()
    {
        // Arrange
        decimal valorHoraOrdinaria = 10_000;
        var items = new Dictionary<TiposHorasExtra, int>();
        decimal sumaEsperada = 0;
        foreach (var kvp in FactorHorasExtra.Factores)
        {
            items[kvp.Key] = 2;
            sumaEsperada += Math.Round(2 * valorHoraOrdinaria * kvp.Value, 0, MidpointRounding.AwayFromZero);
        }
        var horasExtrasService = new HorasExtrasService(valorHoraOrdinaria, items);
        
        // Act
        var valorTotal = horasExtrasService.ValorTotal;
        
        // Assert
        Assert.Equal(sumaEsperada, valorTotal);
    }
}
