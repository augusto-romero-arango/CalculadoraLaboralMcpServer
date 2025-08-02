using CalculadoraLaboral.McpServer.Domain.Services;
using CalculadoraLaboral.McpServer.Domain.Constants;

namespace CalculadoraLaboral.Tests;

public class SeguridadSocialTests
{
    private readonly decimal _salarioMinimo = 1423500m; // SMLV 2025

    [Fact]
    public void SeguridadSocialSalud_DebeCalcularCorrectamente()
    {
        // Arrange
        decimal totalSalarial = 3000000;
        decimal totalDevengado = 3500000;
        decimal totalPrestacional = 3000000;
        decimal factorRiesgo = 0.00522m;

        // Act
        var salud = new SeguridadSocialSalud(totalSalarial, totalDevengado, totalPrestacional, _salarioMinimo);

        // Assert
        Assert.Equal("Salud", salud.Nombre);
        Assert.Equal("Aporte a salud por el empleador", salud.Descripcion);
        Assert.True(salud.Valor > 0);
        
        // Verificar que se aplica la tarifa correcta (8.5%)
        Assert.True(salud.Valor > totalPrestacional * 0.08m); // Debe ser más del 8%
        Assert.True(salud.Valor < totalPrestacional * 0.10m); // Debe ser menos del 10%
    }

    [Fact]
    public void SeguridadSocialPension_DebeCalcularCorrectamente()
    {
        // Arrange
        decimal totalSalarial = 2500000;
        decimal totalDevengado = 3000000;
        decimal totalPrestacional = 2500000;

        // Act
        var pension = new SeguridadSocialPension(totalSalarial, totalDevengado, totalPrestacional, _salarioMinimo);

        // Assert
        Assert.Equal("Pensión", pension.Nombre);
        Assert.Equal("Aporte a pensión por el empleador", pension.Descripcion);
        Assert.True(pension.Valor > 0);
        
        // Verificar que se aplica la tarifa correcta (12%)
        Assert.True(pension.Valor > totalPrestacional * 0.11m); // Debe ser más del 11%
        Assert.True(pension.Valor < totalPrestacional * 0.13m); // Debe ser menos del 13%
    }

    [Fact]
    public void SeguridadSocialArl_DebeCalcularConFactorRiesgo()
    {
        // Arrange
        decimal totalSalarial = 4000000;
        decimal totalDevengado = 4500000;
        decimal totalPrestacional = 4000000;
        decimal factorRiesgo = 0.00522m; // Clase I

        // Act
        var arl = new SeguridadSocialArl(totalSalarial, totalDevengado, totalPrestacional, _salarioMinimo, factorRiesgo);

        // Assert
        Assert.Equal("ARL", arl.Nombre);
        Assert.Equal("Administradora de Riesgos Laborales", arl.Descripcion);
        Assert.True(arl.Valor > 0);
    }

    [Fact]
    public void BaseCalculo_Ley1393_DebeAplicarCorrectamente()
    {
        // Arrange - Caso donde totalDevengado * 0.6 > totalSalarial
        decimal totalSalarial = 2000000;
        decimal totalDevengado = 4000000; // 4M, entonces 60% = 2.4M > 2M
        decimal totalPrestacional = 2000000;
        
        // 60% de devengado = 2.4M
        // Ajuste 1393 = 2.4M - 2M = 400K
        // Base = 400K + 2M = 2.4M
        decimal baseEsperada = 2400000;

        // Act
        var salud = new SeguridadSocialSalud(totalSalarial, totalDevengado, totalPrestacional, _salarioMinimo);
        var valorSalud = salud.Valor;
        var valorEsperado = Math.Round(baseEsperada * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.Salud], 0, MidpointRounding.AwayFromZero);

        // Assert
        Assert.Equal(valorEsperado, valorSalud);
    }

    [Fact]
    public void BaseCalculo_Ley1393_NoDebeAplicarSiDevengadoBajo()
    {
        // Arrange - Caso donde totalDevengado * 0.6 <= totalSalarial
        decimal totalSalarial = 3000000;
        decimal totalDevengado = 3200000; // 60% = 1.92M < 3M, no aplica ajuste
        decimal totalPrestacional = 3000000;

        // Act
        var salud = new SeguridadSocialSalud(totalSalarial, totalDevengado, totalPrestacional, _salarioMinimo);
        var valorSalud = salud.Valor;
        
        // Base debería ser max(totalPrestacional, salarioMinimo)
        var baseEsperada = Math.Max(totalPrestacional, _salarioMinimo);
        var valorEsperado = Math.Round(baseEsperada * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.Salud], 0, MidpointRounding.AwayFromZero);

        // Assert
        Assert.Equal(valorEsperado, valorSalud);
    }

    [Fact]
    public void BaseCalculo_DebeLimitarA25SMLV()
    {
        // Arrange - Caso con base muy alta que debe limitarse
        decimal totalSalarial = 30000000; // 30M
        decimal totalDevengado = 60000000; // 60M, muy alto
        decimal totalPrestacional = 30000000;
        decimal tope25SMLV = _salarioMinimo * 25; // Aprox 35.6M

        // Act
        var salud = new SeguridadSocialSalud(totalSalarial, totalDevengado, totalPrestacional, _salarioMinimo);
        var valorSalud = salud.Valor;
        
        // La base no debe exceder 25 SMLV
        var valorMaximo = Math.Round(tope25SMLV * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.Salud], 0, MidpointRounding.AwayFromZero);

        // Assert
        Assert.True(valorSalud <= valorMaximo, $"El valor de salud ({valorSalud}) no debe exceder el tope de 25 SMLV ({valorMaximo})");
    }

    [Fact]
    public void BaseCalculo_DebeGarantizarMinimoSMLV()
    {
        // Arrange - Caso con valores muy bajos
        decimal totalSalarial = 500000;
        decimal totalDevengado = 600000;
        decimal totalPrestacional = 500000;

        // Act
        var salud = new SeguridadSocialSalud(totalSalarial, totalDevengado, totalPrestacional, _salarioMinimo);
        var valorSalud = salud.Valor;
        
        var valorMinimo = Math.Round(_salarioMinimo * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.Salud], 0, MidpointRounding.AwayFromZero);

        // Assert
        Assert.True(valorSalud >= valorMinimo, $"El valor de salud ({valorSalud}) no debe ser menor al mínimo basado en SMLV ({valorMinimo})");
    }

    [Fact]
    public void CalcularSeguridadSocial_DebeRetornarTresElementos()
    {
        // Arrange
        decimal totalSalarial = 3000000;
        decimal totalDevengado = 3500000;
        decimal totalPrestacional = 3000000;
        decimal factorRiesgo = 0.01044m;

        // Act
        var seguridadSocial = SeguridadSocialService.CalcularSeguridadSocial(
            totalSalarial, totalDevengado, totalPrestacional, _salarioMinimo, factorRiesgo);

        // Assert
        Assert.Equal(3, seguridadSocial.Count);
        Assert.Contains(seguridadSocial, s => s.Nombre == "Salud");
        Assert.Contains(seguridadSocial, s => s.Nombre == "Pensión");
        Assert.Contains(seguridadSocial, s => s.Nombre == "ARL");
        
        foreach (var item in seguridadSocial)
        {
            Assert.True(item.Valor > 0, $"{item.Nombre} debe tener valor mayor a 0");
        }
    }

    [Fact]
    public void ParafiscalesCajaCompensacion_DebeCalcularCorrectamente()
    {
        // Arrange
        decimal totalPrestacional = 3000000;
        decimal totalDevengado = 3500000;

        // Act
        var caja = new ParafiscalesCajaCompensacion(totalPrestacional, totalDevengado, _salarioMinimo);

        // Assert
        Assert.Equal("Caja de Compensación", caja.Nombre);
        Assert.Equal("Aporte a caja de compensación familiar", caja.Descripcion);
        Assert.True(caja.Valor > 0);
    }

    [Fact]
    public void ParafiscalesIcbf_DebeCalcularCorrectamente()
    {
        // Arrange
        decimal totalPrestacional = 4000000;
        decimal totalDevengado = 4500000;

        // Act
        var icbf = new ParafiscalesIcbf(totalPrestacional, totalDevengado, _salarioMinimo);

        // Assert
        Assert.Equal("ICBF", icbf.Nombre);
        Assert.Equal("Instituto Colombiano de Bienestar Familiar", icbf.Descripcion);
        Assert.True(icbf.Valor > 0);
    }

    [Fact]
    public void ParafiscalesSena_DebeCalcularCorrectamente()
    {
        // Arrange
        decimal totalPrestacional = 2500000;
        decimal totalDevengado = 3000000;

        // Act
        var sena = new ParafiscalesSena(totalPrestacional, totalDevengado, _salarioMinimo);

        // Assert
        Assert.Equal("SENA", sena.Nombre);
        Assert.Equal("Servicio Nacional de Aprendizaje", sena.Descripcion);
        Assert.True(sena.Valor > 0);
    }

    [Fact]
    public void CalcularParafiscales_DebeRetornarTresElementos()
    {
        // Arrange
        decimal totalPrestacional = 3500000;
        decimal totalDevengado = 4000000;

        // Act
        var parafiscales = SeguridadSocialService.CalcularParafiscales(
            totalPrestacional, totalDevengado, _salarioMinimo);

        // Assert
        Assert.Equal(3, parafiscales.Count);
        Assert.Contains(parafiscales, p => p.Nombre == "Caja de Compensación");
        Assert.Contains(parafiscales, p => p.Nombre == "ICBF");
        Assert.Contains(parafiscales, p => p.Nombre == "SENA");
        
        foreach (var item in parafiscales)
        {
            Assert.True(item.Valor > 0, $"{item.Nombre} debe tener valor mayor a 0");
        }
    }

    [Fact]
    public void CalcularTotalSeguridadSocial_DebeRetornarSeisElementos()
    {
        // Arrange
        decimal totalSalarial = 3000000;
        decimal totalDevengado = 3500000;
        decimal totalPrestacional = 3000000;
        decimal factorRiesgo = 0.02436m;

        // Act
        var total = SeguridadSocialService.CalcularTotalSeguridadSocial(
            totalSalarial, totalDevengado, totalPrestacional, _salarioMinimo, factorRiesgo);

        // Assert
        Assert.Equal(6, total.Count); // 3 seguridad social + 3 parafiscales
        
        var totalValor = total.Sum(t => t.Valor);
        Assert.True(totalValor > 0);
        
        // Verificar que todos los elementos tienen nombres únicos
        var nombres = total.Select(t => t.Nombre).ToList();
        Assert.Equal(nombres.Count, nombres.Distinct().Count());
    }
}