using CalculadoraLaboral.McpServer.Domain.Services;

namespace CalculadoraLaboral.Tests;

public class ExoneracionParafiscalesTests
{
    private readonly decimal _salarioMinimo = 1423500m; // SMLV 2025

    [Theory]
    [InlineData(5000000)]  // 5M < 10 SMLV (14.235M)
    [InlineData(8000000)]  // 8M < 10 SMLV
    [InlineData(10000000)] // 10M < 10 SMLV
    [InlineData(14235499)] // Justo debajo del límite
    public void Parafiscales_DevengadoMenorA10SMLV_DebenEstarExonerados(decimal totalDevengado)
    {
        // Arrange
        decimal totalPrestacional = totalDevengado * 0.8m; // Simular prestacional menor
        decimal tope10SMLV = _salarioMinimo * 10;

        // Act
        var caja = new ParafiscalesCajaCompensacion(totalPrestacional, totalDevengado, _salarioMinimo);
        var icbf = new ParafiscalesIcbf(totalPrestacional, totalDevengado, _salarioMinimo);
        var sena = new ParafiscalesSena(totalPrestacional, totalDevengado, _salarioMinimo);

        // Assert
        Assert.True(totalDevengado < tope10SMLV, $"Total devengado {totalDevengado} debe ser menor a 10 SMLV {tope10SMLV}");
        Assert.Equal(0, caja.Valor);
        Assert.Equal(0, icbf.Valor);
        Assert.Equal(0, sena.Valor);
    }

    [Theory]
    [InlineData(15000000)]  // 15M > 10 SMLV (14.235M)
    [InlineData(20000000)]  // 20M > 10 SMLV
    [InlineData(14235500)]  // Justo arriba del límite
    [InlineData(50000000)]  // Muy alto
    public void Parafiscales_DevengadoMayorA10SMLV_DebenCalcularseNormalmente(decimal totalDevengado)
    {
        // Arrange
        decimal totalPrestacional = totalDevengado * 0.8m;
        decimal tope10SMLV = _salarioMinimo * 10;

        // Act
        var caja = new ParafiscalesCajaCompensacion(totalPrestacional, totalDevengado, _salarioMinimo);
        var icbf = new ParafiscalesIcbf(totalPrestacional, totalDevengado, _salarioMinimo);
        var sena = new ParafiscalesSena(totalPrestacional, totalDevengado, _salarioMinimo);

        // Assert
        Assert.True(totalDevengado >= tope10SMLV, $"Total devengado {totalDevengado} debe ser mayor o igual a 10 SMLV {tope10SMLV}");
        Assert.True(caja.Valor > 0, "Caja de compensación debe tener valor > 0");
        Assert.True(icbf.Valor > 0, "ICBF debe tener valor > 0");
        Assert.True(sena.Valor > 0, "SENA debe tener valor > 0");
    }

    [Fact]
    public void Parafiscales_ExactamenteEnLimite10SMLV_DebenCalcularseNormalmente()
    {
        // Arrange
        decimal totalDevengado = _salarioMinimo * 10; // Exactamente 10 SMLV
        decimal totalPrestacional = totalDevengado;

        // Act
        var caja = new ParafiscalesCajaCompensacion(totalPrestacional, totalDevengado, _salarioMinimo);
        var icbf = new ParafiscalesIcbf(totalPrestacional, totalDevengado, _salarioMinimo);
        var sena = new ParafiscalesSena(totalPrestacional, totalDevengado, _salarioMinimo);

        // Assert
        Assert.True(caja.Valor > 0, "En el límite exacto, caja debe calcularse normalmente");
        Assert.True(icbf.Valor > 0, "En el límite exacto, ICBF debe calcularse normalmente");
        Assert.True(sena.Valor > 0, "En el límite exacto, SENA debe calcularse normalmente");
    }

    [Fact]
    public void SeguridadSocial_NoSeAplicaExoneracion_SiempreSeCalculan()
    {
        // Arrange - Devengado muy bajo que exoneraría parafiscales
        decimal totalSalarial = 1000000;
        decimal totalDevengado = 2000000; // < 10 SMLV
        decimal totalPrestacional = 1000000;
        decimal factorRiesgo = 0.00522m;

        // Act
        var salud = new SeguridadSocialSalud(totalSalarial, totalDevengado, totalPrestacional, _salarioMinimo);
        var pension = new SeguridadSocialPension(totalSalarial, totalDevengado, totalPrestacional, _salarioMinimo);
        var arl = new SeguridadSocialArl(totalSalarial, totalDevengado, totalPrestacional, _salarioMinimo, factorRiesgo);

        // Assert - Seguridad social NO se exonera
        Assert.True(salud.Valor > 0, "Salud debe calcularse independientemente del nivel de ingresos");
        Assert.True(pension.Valor > 0, "Pensión debe calcularse independientemente del nivel de ingresos");
        Assert.True(arl.Valor > 0, "ARL debe calcularse independientemente del nivel de ingresos");
    }

    [Fact]
    public void CalcularParafiscales_ConExoneracion_DebeRetornarValoresCero()
    {
        // Arrange
        decimal totalPrestacional = 5000000;
        decimal totalDevengado = 7000000; // < 10 SMLV

        // Act
        var parafiscales = SeguridadSocialService.CalcularParafiscales(
            totalPrestacional, totalDevengado, _salarioMinimo);

        // Assert
        Assert.Equal(3, parafiscales.Count);
        foreach (var parafiscal in parafiscales)
        {
            Assert.Equal(0, parafiscal.Valor);
        }
    }

    [Fact]
    public void CalcularParafiscales_SinExoneracion_DebeRetornarValoresPositivos()
    {
        // Arrange
        decimal totalPrestacional = 15000000;
        decimal totalDevengado = 18000000; // > 10 SMLV

        // Act
        var parafiscales = SeguridadSocialService.CalcularParafiscales(
            totalPrestacional, totalDevengado, _salarioMinimo);

        // Assert
        Assert.Equal(3, parafiscales.Count);
        foreach (var parafiscal in parafiscales)
        {
            Assert.True(parafiscal.Valor > 0, $"{parafiscal.Nombre} debe tener valor > 0");
        }
    }

    [Fact]
    public void CalcularTotalSeguridadSocial_ConExoneracion_SoloSeguridadSocialTieneValor()
    {
        // Arrange
        decimal totalSalarial = 6000000;
        decimal totalDevengado = 8000000; // < 10 SMLV
        decimal totalPrestacional = 6000000;
        decimal factorRiesgo = 0.01044m;

        // Act
        var total = SeguridadSocialService.CalcularTotalSeguridadSocial(
            totalSalarial, totalDevengado, totalPrestacional, _salarioMinimo, factorRiesgo);

        // Assert
        Assert.Equal(6, total.Count);
        
        // Seguridad social debe tener valores > 0
        var seguridadSocial = total.Where(t => 
            t.Nombre == "Salud" || 
            t.Nombre == "Pensión" || 
            t.Nombre == "ARL").ToList();
        
        foreach (var ss in seguridadSocial)
        {
            Assert.True(ss.Valor > 0, $"{ss.Nombre} debe tener valor > 0");
        }
        
        // Parafiscales deben tener valores = 0
        var parafiscales = total.Where(t => 
            t.Nombre.Contains("Compensación") || 
            t.Nombre == "ICBF" || 
            t.Nombre == "SENA").ToList();
        
        foreach (var para in parafiscales)
        {
            Assert.Equal(0, para.Valor);
        }
    }

    [Theory]
    [InlineData(1423500)]   // 1 SMLV
    [InlineData(7117500)]   // 5 SMLV
    [InlineData(14235000)]  // 10 SMLV - 500
    public void BaseCalculo_DevengadoBajo_DebeRetornarCero(decimal totalDevengado)
    {
        // Arrange
        decimal totalPrestacional = totalDevengado;

        // Act
        var caja = new ParafiscalesCajaCompensacion(totalPrestacional, totalDevengado, _salarioMinimo);

        // Este test verifica que la propiedad BaseCalculo en Parafiscales retorna 0
        // cuando totalDevengado < 10 SMLV, lo que resulta en Valor = 0

        // Assert
        Assert.Equal(0, caja.Valor);
    }
}