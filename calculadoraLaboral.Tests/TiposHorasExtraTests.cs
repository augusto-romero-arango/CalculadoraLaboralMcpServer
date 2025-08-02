using CalculadoraLaboral.McpServer.Domain.Models;

namespace CalculadoraLaboral.Tests;

public class TiposHorasExtraTests
{
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
    public void FactorHorasExtra_NuevosTipos_DebeRetornarFactorCorrecto(TiposHorasExtra tipo, decimal factorEsperado)
    {
        // Act
        var factor = FactorHorasExtra.Factores[tipo];

        // Assert
        Assert.Equal(factorEsperado, factor);
    }

    [Fact]
    public void FactorHorasExtra_RecargoNocturno_DebeEstar035NoUno35()
    {
        // Arrange
        var tipoRecargo = TiposHorasExtra.RecargoNocturno;
        var factorEsperado = 0.35m;

        // Act
        var factor = FactorHorasExtra.Factores[tipoRecargo];

        // Assert
        Assert.Equal(factorEsperado, factor);
        Assert.NotEqual(1.35m, factor); // Verificar que no sea el valor incorrecto anterior
    }

    [Fact]
    public void FactorHorasExtra_TiposOriginales_DebenMantenerFactores()
    {
        // Arrange & Act & Assert
        Assert.Equal(1.25m, FactorHorasExtra.Factores[TiposHorasExtra.DiurnaOrdinaria]);
        Assert.Equal(1.75m, FactorHorasExtra.Factores[TiposHorasExtra.DiurnaFestiva]);
        Assert.Equal(1.75m, FactorHorasExtra.Factores[TiposHorasExtra.NocturnaOrdinaria]);
        Assert.Equal(2.00m, FactorHorasExtra.Factores[TiposHorasExtra.NocturnaFestiva]);
        Assert.Equal(1.75m, FactorHorasExtra.Factores[TiposHorasExtra.RecargoFestivo]);
    }

    [Fact]
    public void FactorHorasExtra_TodosLosTipos_DebenTenerFactor()
    {
        // Arrange
        var todosLosTipos = Enum.GetValues<TiposHorasExtra>();

        // Act & Assert
        foreach (var tipo in todosLosTipos)
        {
            Assert.True(FactorHorasExtra.Factores.ContainsKey(tipo), 
                $"El tipo {tipo} debe tener un factor definido");
            Assert.True(FactorHorasExtra.Factores[tipo] > 0, 
                $"El factor para {tipo} debe ser mayor que 0");
        }
    }

    [Theory]
    [InlineData(TiposHorasExtra.HEFD, TiposHorasExtra.DiurnaFestiva)]
    [InlineData(TiposHorasExtra.HEFN, TiposHorasExtra.NocturnaFestiva)]
    public void FactorHorasExtra_TiposFestivos_DebenTenerFactoresDiferentes(TiposHorasExtra tipoNuevo, TiposHorasExtra tipoOriginal)
    {
        // Act
        var factorNuevo = FactorHorasExtra.Factores[tipoNuevo];
        var factorOriginal = FactorHorasExtra.Factores[tipoOriginal];

        // Assert
        Assert.NotEqual(factorOriginal, factorNuevo);
        Assert.True(factorNuevo > factorOriginal, 
            $"El factor para {tipoNuevo} ({factorNuevo}) debe ser mayor que {tipoOriginal} ({factorOriginal})");
    }
}