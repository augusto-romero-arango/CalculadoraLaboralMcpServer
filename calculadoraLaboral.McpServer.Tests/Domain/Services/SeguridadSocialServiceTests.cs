using CalculadoraLaboral.McpServer.Domain.Services;
using Xunit;

namespace CalculadoraLaboral.McpServer.Tests.Domain.Services;

public class SeguridadSocialServiceTests
{
    [Fact]
    public void SeguridadSocialSalud_ConSalarioMinimo_DebeRetornarCero()
    {
        // Arrange
        decimal totalSalarial = 1_300_000;
        decimal totalDevengado = 1_300_000;
        decimal totalPrestacional = 1_300_000;
        decimal salarioMinimo = 1_300_000;

        // Act
        var seguridadSocialSalud = new SeguridadSocialSalud(
            totalSalarial,
            totalDevengado,
            totalPrestacional,
            salarioMinimo
        );

        // Assert
        Assert.Equal(0, seguridadSocialSalud.Valor);
    }

    [Fact]
    public void SeguridadSocialSalud_ConSalarioAlto_DebeCalcularCorrectamente()
    {
        // Arrange
        decimal totalSalarial = 13_000_000;
        decimal totalDevengado = 13_000_000;
        decimal totalPrestacional = 13_000_000;
        decimal salarioMinimo = 1_300_000;

        // Act
        var seguridadSocialSalud = new SeguridadSocialSalud(
            totalSalarial,
            totalDevengado,
            totalPrestacional,
            salarioMinimo
        );

        // Assert
        Assert.Equal(1_105_000, seguridadSocialSalud.Valor);
    }

    [Fact]
    public void SeguridadSocialSalud_ConLey1393_DebeAplicarBaseCorrectamente()
    {
        // Arrange
        decimal totalSalarial = 5_000_000;
        decimal totalDevengado = 25_000_000;
        decimal totalPrestacional = 5_000_000;
        decimal salarioMinimo = 1_300_000;

        // Act
        var seguridadSocialSalud = new SeguridadSocialSalud(
            totalSalarial,
            totalDevengado,
            totalPrestacional,
            salarioMinimo
        );

        // Assert
        Assert.Equal(1_275_000, seguridadSocialSalud.Valor);
    }

    [Fact]
    public void SeguridadSocialSalud_ConTope25SMLV_DebeAplicarTopeMaximo()
    {
        // Arrange
        decimal totalSalarial = 36_000_000;
        decimal totalDevengado = 56_000_000;
        decimal totalPrestacional = 36_000_000;
        decimal salarioMinimo = 1_300_000;

        // Act
        var seguridadSocialSalud = new SeguridadSocialSalud(
            totalSalarial,
            totalDevengado,
            totalPrestacional,
            salarioMinimo
        );

        // Assert
        Assert.Equal(2_762_500, seguridadSocialSalud.Valor);
    }

    [Fact]
    public void SeguridadSocialPension_ConSalarioMinimo_DebeCalcularCorrectamente()
    {
        // Arrange
        decimal totalSalarial = 1_300_000;
        decimal totalDevengado = 1_300_000;
        decimal totalPrestacional = 1_300_000;
        decimal salarioMinimo = 1_300_000;

        // Act
        var seguridadSocialPension = new SeguridadSocialPension(
            totalSalarial,
            totalDevengado,
            totalPrestacional,
            salarioMinimo
        );

        // Assert
        Assert.Equal(156_000, seguridadSocialPension.Valor);
    }

    [Fact]
    public void SeguridadSocialPension_ConLey1393_DebeCalcularCorrectamente()
    {
        // Arrange
        decimal totalSalarial = 5_000_000;
        decimal totalDevengado = 25_000_000;
        decimal totalPrestacional = 5_000_000;
        decimal salarioMinimo = 1_300_000;

        // Act
        var seguridadSocialPension = new SeguridadSocialPension(
            totalSalarial,
            totalDevengado,
            totalPrestacional,
            salarioMinimo
        );

        // Assert
        Assert.Equal(1_800_000, seguridadSocialPension.Valor);
    }

    [Fact]
    public void SeguridadSocialPension_ConSalarioAltoyTope_DebeCalcularCorrectamente()
    {
        // Arrange
        decimal totalSalarial = 30_500_000;
        decimal totalDevengado = 50_500_000;
        decimal totalPrestacional = 30_500_000;
        decimal salarioMinimo = 1_300_000;

        // Act
        var seguridadSocialPension = new SeguridadSocialPension(
            totalSalarial,
            totalDevengado,
            totalPrestacional,
            salarioMinimo
        );

        // Assert
        Assert.Equal(3_660_000, seguridadSocialPension.Valor);
    }

    [Theory]
    [InlineData(0.00522, 6_786)]
    [InlineData(0.01044, 13_572)]
    [InlineData(0.02436, 31_668)]
    [InlineData(0.04350, 56_550)]
    [InlineData(0.06960, 90_480)]
    public void SeguridadSocialArl_ConDiferentesRiesgos_DebeCalcularCorrectamente(decimal factorRiesgo, decimal esperado)
    {
        // Arrange
        decimal totalSalarial = 1_300_000;
        decimal totalDevengado = 1_300_000;
        decimal totalPrestacional = 1_300_000;
        decimal salarioMinimo = 1_300_000;

        // Act
        var seguridadSocialArl = new SeguridadSocialArl(
            totalSalarial,
            totalDevengado,
            totalPrestacional,
            salarioMinimo,
            factorRiesgo
        );

        // Assert
        Assert.Equal(esperado, seguridadSocialArl.Valor);
    }

    [Fact]
    public void ParafiscalesCajaCompensacion_ConSalarioAlto_DebeCalcularCorrectamente()
    {
        // Arrange
        decimal totalPrestacional = 13_000_000;
        decimal totalDevengado = 13_000_000;
        decimal salarioMinimo = 1_300_000;

        // Act
        var parafiscales = new ParafiscalesCajaCompensacion(
            totalPrestacional,
            totalDevengado,
            salarioMinimo
        );

        // Assert
        Assert.Equal(520_000, parafiscales.Valor);
    }

    [Fact]
    public void ParafiscalesIcbf_ConSalarioAlto_DebeCalcularCorrectamente()
    {
        // Arrange
        decimal totalPrestacional = 13_000_000;
        decimal totalDevengado = 13_000_000;
        decimal salarioMinimo = 1_300_000;

        // Act
        var parafiscales = new ParafiscalesIcbf(
            totalPrestacional,
            totalDevengado,
            salarioMinimo
        );

        // Assert
        Assert.Equal(390_000, parafiscales.Valor);
    }

    [Fact]
    public void ParafiscalesSena_ConSalarioAlto_DebeCalcularCorrectamente()
    {
        // Arrange
        decimal totalPrestacional = 13_000_000;
        decimal totalDevengado = 13_000_000;
        decimal salarioMinimo = 1_300_000;

        // Act
        var parafiscales = new ParafiscalesSena(
            totalPrestacional,
            totalDevengado,
            salarioMinimo
        );

        // Assert
        Assert.Equal(260_000, parafiscales.Valor);
    }

    [Fact]
    public void ParafiscalesCajaCompensacion_ConBaseVariable_DebeCalcularCorrectamente()
    {
        // Arrange
        decimal totalPrestacional = 11_830_000;
        decimal totalDevengado = 16_900_000;
        decimal salarioMinimo = 1_300_000;

        // Act
        var parafiscales = new ParafiscalesCajaCompensacion(
            totalPrestacional,
            totalDevengado,
            salarioMinimo
        );

        // Assert
        Assert.Equal(473_200, parafiscales.Valor);
    }

    [Fact]
    public void ParafiscalesIcbf_ConBaseVariable_DebeCalcularCorrectamente()
    {
        // Arrange
        decimal totalPrestacional = 11_830_000;
        decimal totalDevengado = 16_900_000;
        decimal salarioMinimo = 1_300_000;

        // Act
        var parafiscales = new ParafiscalesIcbf(
            totalPrestacional,
            totalDevengado,
            salarioMinimo
        );

        // Assert
        Assert.Equal(354_900, parafiscales.Valor);
    }

    [Fact]
    public void ParafiscalesSena_ConBaseVariable_DebeCalcularCorrectamente()
    {
        // Arrange
        decimal totalPrestacional = 11_830_000;
        decimal totalDevengado = 16_900_000;
        decimal salarioMinimo = 1_300_000;

        // Act
        var parafiscales = new ParafiscalesSena(
            totalPrestacional,
            totalDevengado,
            salarioMinimo
        );

        // Assert
        Assert.Equal(236_600, parafiscales.Valor);
    }

    [Fact]
    public void CalcularTotalSeguridadSocial_DebeRetornarTodosLosConceptos()
    {
        // Arrange
        decimal totalSalarial = 5_000_000;
        decimal totalDevengado = 25_000_000;
        decimal totalPrestacional = 5_000_000;
        decimal salarioMinimo = 1_300_000;
        decimal factorRiesgoLaboral = 0.00522m;

        // Act
        var result = SeguridadSocialService.CalcularTotalSeguridadSocial(
            totalSalarial,
            totalDevengado,
            totalPrestacional,
            salarioMinimo,
            factorRiesgoLaboral
        );

        // Assert
        Assert.Equal(6, result.Count); // 3 seguridad social + 3 parafiscales
        
        var salud = result.FirstOrDefault(r => r.Nombre == "Salud");
        var pension = result.FirstOrDefault(r => r.Nombre == "Pensión");
        var arl = result.FirstOrDefault(r => r.Nombre == "ARL");
        var ccf = result.FirstOrDefault(r => r.Nombre == "Caja de Compensación");
        var icbf = result.FirstOrDefault(r => r.Nombre == "ICBF");
        var sena = result.FirstOrDefault(r => r.Nombre == "SENA");

        Assert.NotNull(salud);
        Assert.NotNull(pension);
        Assert.NotNull(arl);
        Assert.NotNull(ccf);
        Assert.NotNull(icbf);
        Assert.NotNull(sena);

        Assert.Equal(1_275_000, salud.Valor);
        Assert.Equal(1_800_000, pension.Valor);
        Assert.Equal(78_390, arl.Valor);
        Assert.Equal(200_000, ccf.Valor);
        Assert.Equal(150_000, icbf.Valor);
        Assert.Equal(100_000, sena.Valor);
    }
}