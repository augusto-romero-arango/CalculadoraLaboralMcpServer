using CalculadoraLaboral.McpServer.Domain.Services;
using CalculadoraLaboral.McpServer.Domain.Constants;

namespace CalculadoraLaboral.Tests;

public class ExoneracionParafiscalesTests
{
    private readonly decimal _salarioMinimo = 1_423_500m; // SMLV 2025

    [Theory]
    [InlineData(5_000_000)]  // 5M < 10 SMLV (14.235M)
    [InlineData(8_000_000)]  // 8M < 10 SMLV
    [InlineData(10_000_000)] // 10M < 10 SMLV
    [InlineData(14_234_999)] // Justo debajo del límite
    public void Parafiscales_DevengadoMenorA10SMLV_DebenEstarExoneradosICBFySENA(decimal totalDevengado)
    {
        // Arrange
        decimal baseSalarial = 5_000_000; // Base fija para las pruebas
        decimal tope10SMLV = _salarioMinimo * 10;

        // Act
        var caja = new ParafiscalesCajaCompensacion(baseSalarial, totalDevengado, _salarioMinimo);
        var icbf = new ParafiscalesIcbf(baseSalarial, totalDevengado, _salarioMinimo);
        var sena = new ParafiscalesSena(baseSalarial, totalDevengado, _salarioMinimo);

        // Assert
        Assert.True(totalDevengado < tope10SMLV, $"Total devengado {totalDevengado} debe ser menor a 10 SMLV {tope10SMLV}");
        
        // Caja NO se exonera
        decimal valorEsperadoCaja = Math.Round(baseSalarial * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.CCF], 0, MidpointRounding.AwayFromZero);
        Assert.Equal(valorEsperadoCaja, caja.Valor);
        
        // ICBF y SENA SÍ se exoneran
        Assert.Equal(0m, icbf.Valor);
        Assert.Equal(0m, sena.Valor);
    }

    [Theory]
    [InlineData(15_000_000)]  // 15M > 10 SMLV (14.235M)
    [InlineData(20_000_000)]  // 20M > 10 SMLV
    [InlineData(14_235_500)]  // Justo arriba del límite
    [InlineData(50_000_000)]  // Muy alto
    public void Parafiscales_DevengadoMayorA10SMLV_DebenCalcularseNormalmente(decimal totalDevengado)
    {
        // Arrange
        decimal baseSalarial = 5_000_000; // Base fija para las pruebas
        decimal tope10SMLV = _salarioMinimo * 10;

        // Act
        var caja = new ParafiscalesCajaCompensacion(baseSalarial, totalDevengado, _salarioMinimo);
        var icbf = new ParafiscalesIcbf(baseSalarial, totalDevengado, _salarioMinimo);
        var sena = new ParafiscalesSena(baseSalarial, totalDevengado, _salarioMinimo);

        // Assert
        Assert.True(totalDevengado >= tope10SMLV, $"Total devengado {totalDevengado} debe ser mayor o igual a 10 SMLV {tope10SMLV}");
        
        decimal valorEsperadoCaja = Math.Round(baseSalarial * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.CCF], 0, MidpointRounding.AwayFromZero);
        decimal valorEsperadoICBF = Math.Round(baseSalarial * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.ICBF], 0, MidpointRounding.AwayFromZero);
        decimal valorEsperadoSENA = Math.Round(baseSalarial * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.SENA], 0, MidpointRounding.AwayFromZero);
        
        Assert.Equal(valorEsperadoCaja, caja.Valor);
        Assert.Equal(valorEsperadoICBF, icbf.Valor);
        Assert.Equal(valorEsperadoSENA, sena.Valor);
    }

    [Fact]
    public void Parafiscales_ExactamenteEnLimite10SMLV_DebenCalcularseNormalmente()
    {
        // Arrange
        decimal baseSalarial = _salarioMinimo * 10; // Exactamente 10 SMLV
        decimal totalDevengado = baseSalarial; // Valores iguales

        // Act
        var caja = new ParafiscalesCajaCompensacion(baseSalarial, totalDevengado, _salarioMinimo);
        var icbf = new ParafiscalesIcbf(baseSalarial, totalDevengado, _salarioMinimo);
        var sena = new ParafiscalesSena(baseSalarial, totalDevengado, _salarioMinimo);

        // Assert
        decimal valorEsperadoCaja = Math.Round(baseSalarial * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.CCF], 0, MidpointRounding.AwayFromZero);
        decimal valorEsperadoICBF = Math.Round(baseSalarial * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.ICBF], 0, MidpointRounding.AwayFromZero);
        decimal valorEsperadoSENA = Math.Round(baseSalarial * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.SENA], 0, MidpointRounding.AwayFromZero);
        
        Assert.Equal(valorEsperadoCaja, caja.Valor);
        Assert.Equal(valorEsperadoICBF, icbf.Valor);
        Assert.Equal(valorEsperadoSENA, sena.Valor);
    }

   
    [Fact]
    public void CalcularParafiscales_ConExoneracion_DebeRetornarValoresCeroParaICBFySENA()
    {
        // Arrange
        decimal baseSalarial = 5_000_000;
        decimal totalDevengado = 7_000_000; // < 10 SMLV para prueba de exoneración

        // Act
        var parafiscales = SeguridadSocialService.CalcularParafiscales(
            baseSalarial, totalDevengado, _salarioMinimo);

        // Assert
        Assert.Equal(3, parafiscales.Count);
        
        var caja = parafiscales.First(p => p.Nombre == "Caja de Compensación");
        var icbf = parafiscales.First(p => p.Nombre == "ICBF");
        var sena = parafiscales.First(p => p.Nombre == "SENA");
        
        decimal valorEsperadoCaja = Math.Round(baseSalarial * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.CCF], 0, MidpointRounding.AwayFromZero);
        
        Assert.Equal(valorEsperadoCaja, caja.Valor);
        Assert.Equal(0m, icbf.Valor);
        Assert.Equal(0m, sena.Valor);
    }

    [Fact]
    public void CalcularParafiscales_SinExoneracion_DebeRetornarValoresPositivos()
    {
        // Arrange
        decimal baseSalarial = 15_000_000;
        decimal totalDevengado = 18_000_000; // > 10 SMLV para prueba sin exoneración

        // Act
        var parafiscales = SeguridadSocialService.CalcularParafiscales(
            baseSalarial, totalDevengado, _salarioMinimo);

        // Assert
        Assert.Equal(3, parafiscales.Count);
        
        var caja = parafiscales.First(p => p.Nombre == "Caja de Compensación");
        var icbf = parafiscales.First(p => p.Nombre == "ICBF");
        var sena = parafiscales.First(p => p.Nombre == "SENA");
        
        decimal valorEsperadoCaja = Math.Round(baseSalarial * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.CCF], 0, MidpointRounding.AwayFromZero);
        decimal valorEsperadoICBF = Math.Round(baseSalarial * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.ICBF], 0, MidpointRounding.AwayFromZero);
        decimal valorEsperadoSENA = Math.Round(baseSalarial * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.SENA], 0, MidpointRounding.AwayFromZero);
        
        Assert.Equal(valorEsperadoCaja, caja.Valor);
        Assert.Equal(valorEsperadoICBF, icbf.Valor);
        Assert.Equal(valorEsperadoSENA, sena.Valor);
    }

    [Fact]
    public void CalcularTotalSeguridadSocial_ConExoneracion_SoloICBFySENATienenValorCero()
    {
        // Arrange
        decimal baseSalarial = 6_000_000;
        decimal totalDevengado = 8_000_000; // < 10 SMLV para prueba de exoneración
        decimal factorRiesgo = 0.01044m;

        // Act
        var total = SeguridadSocialService.CalcularTotalSeguridadSocial(
            baseSalarial, totalDevengado, baseSalarial, _salarioMinimo, factorRiesgo);

        // Assert
        Assert.Equal(6, total.Count);
        
        var salud = total.First(t => t.Nombre == "Salud");
        var pension = total.First(t => t.Nombre == "Pensión");
        var arl = total.First(t => t.Nombre == "ARL");
        var caja = total.First(t => t.Nombre == "Caja de Compensación");
        var icbf = total.First(t => t.Nombre == "ICBF");
        var sena = total.First(t => t.Nombre == "SENA");
        
        decimal valorEsperadoCaja = Math.Round(baseSalarial * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.CCF], 0, MidpointRounding.AwayFromZero);
        decimal valorEsperadoPension = Math.Round(baseSalarial * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.Pension], 0, MidpointRounding.AwayFromZero);
        decimal valorEsperadoArl = Math.Round(baseSalarial * factorRiesgo, 0, MidpointRounding.AwayFromZero);
        
        Assert.Equal(0m, salud.Valor); // Salud también se exonera con Ley PAEF
        Assert.Equal(valorEsperadoPension, pension.Valor);
        Assert.Equal(valorEsperadoArl, arl.Valor);
        Assert.Equal(valorEsperadoCaja, caja.Valor);
        Assert.Equal(0m, icbf.Valor);
        Assert.Equal(0m, sena.Valor);
    }

    [Theory]
    [InlineData(1_423_500)]   // 1 SMLV
    [InlineData(7_117_500)]   // 5 SMLV
    [InlineData(14_234_999)]  // 10 SMLV
    public void BaseCalculo_DevengadoBajo_DebeRetornarCorrectamente(decimal baseSalarial)
    {
        // Arrange
        decimal totalDevengado = baseSalarial; // Valores iguales

        // Act
        var caja = new ParafiscalesCajaCompensacion(baseSalarial, totalDevengado, _salarioMinimo);
        var icbf = new ParafiscalesIcbf(baseSalarial, totalDevengado, _salarioMinimo);
        var sena = new ParafiscalesSena(baseSalarial, totalDevengado, _salarioMinimo);

        // Assert
        decimal valorEsperadoCaja = Math.Round(baseSalarial * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.CCF], 0, MidpointRounding.AwayFromZero);
        
        Assert.Equal(valorEsperadoCaja, caja.Valor);
        Assert.Equal(0m, icbf.Valor);
        Assert.Equal(0m, sena.Valor);
    }

    [Fact]
    public void CajaDeCompensacion_NoDebeExonerarseNuncaIndependienteDelDevengado()
    {
        // Arrange
        decimal baseSalarial = 2_000_000;
        decimal totalDevengadoBajo = 5_000_000; // < 10 SMLV
        decimal totalDevengadoAlto = 20_000_000; // > 10 SMLV

        // Act
        var cajaBajo = new ParafiscalesCajaCompensacion(baseSalarial, totalDevengadoBajo, _salarioMinimo);
        var cajaAlto = new ParafiscalesCajaCompensacion(baseSalarial, totalDevengadoAlto, _salarioMinimo);

        // Assert
        decimal valorEsperadoCaja = Math.Round(baseSalarial * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.CCF], 0, MidpointRounding.AwayFromZero);
        
        Assert.Equal(valorEsperadoCaja, cajaBajo.Valor);
        Assert.Equal(valorEsperadoCaja, cajaAlto.Valor);
    }

    [Fact]
    public void Exoneracion_DebeAplicarseCorrerctamenteConSalarioMinimoVariable()
    {
        // Arrange
        decimal salarioMinimoBajo = 1_000_000m;
        decimal salarioMinimoAlto = 2_000_000m;
        decimal baseSalarial = 5_000_000;
        decimal totalDevengado = 12_000_000; // Entre los dos límites de exoneración
        
        // Act
        var icbfBajo = new ParafiscalesIcbf(baseSalarial, totalDevengado, salarioMinimoBajo);
        var icbfAlto = new ParafiscalesIcbf(baseSalarial, totalDevengado, salarioMinimoAlto);
        
        // Assert
        decimal valorEsperadoICBF = Math.Round(baseSalarial * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.ICBF], 0, MidpointRounding.AwayFromZero);
        
        // Con salario mínimo bajo: 10 SMLV = 10M, totalDevengado = 12M > 10M, NO debe exonerarse
        Assert.Equal(valorEsperadoICBF, icbfBajo.Valor);
        
        // Con salario mínimo alto: 10 SMLV = 20M, totalDevengado = 12M < 20M, debe exonerarse
        Assert.Equal(0m, icbfAlto.Valor);
    }

    [Theory]
    [InlineData(9_000_000)]   // < 10 SMLV
    [InlineData(15_000_000)]  // > 10 SMLV
    public void Salud_DebeAplicarExoneracionAlIgualQueICBFySENA(decimal totalDevengado)
    {
        // Arrange
        decimal baseSalarial = totalDevengado;
        decimal totalPrestacional = totalDevengado;
    
        // Act
        var salud = new SeguridadSocialSalud(baseSalarial, totalDevengado, totalPrestacional, _salarioMinimo);
    
        // Assert
        decimal valorEsperadoSalud = Math.Round(baseSalarial * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.Salud], 0, MidpointRounding.AwayFromZero);
    
        if (totalDevengado < _salarioMinimo * 10)
            Assert.Equal(0m, salud.Valor);
        else
            Assert.Equal(valorEsperadoSalud, salud.Valor);
    }

    [Theory]
    [InlineData(9_000_000)]   // < 10 SMLV
    [InlineData(15_000_000)]  // > 10 SMLV
    public void Pension_NoDebeExonerarseIndependienteDelDevengado(decimal totalDevengado)
    {
        // Arrange
        decimal baseSalarial = totalDevengado;
        decimal totalPrestacional = totalDevengado;
    
        // Act
        var pension = new SeguridadSocialPension(baseSalarial, totalDevengado, totalPrestacional, _salarioMinimo);
    
        // Assert
        decimal valorEsperadoPension = Math.Round(baseSalarial * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.Pension], 0, MidpointRounding.AwayFromZero);
    
        Assert.Equal(valorEsperadoPension, pension.Valor);
    }

    [Theory]
    [InlineData(9_000_000, 46_980)]  // Devengado bajo (< 10 SMLV), valor esperado = 9M * 0.00522
    [InlineData(15_000_000, 78_300)] // Devengado alto (> 10 SMLV), valor esperado = 15M * 0.00522
    public void ARL_NoDebeExonerarseIndependienteDelDevengado(decimal totalDevengado, decimal valorEsperado)
    {
        // Arrange
        decimal factorRiesgo = 0.00522m; // Riesgo Clase I
        
        // Act
        var arl = new SeguridadSocialArl(totalDevengado, totalDevengado, totalDevengado, _salarioMinimo, factorRiesgo);
        
        // Assert
        Assert.Equal(valorEsperado, arl.Valor);
    }

    [Theory]
    [InlineData(5_000_000)]   // 5M < 10 SMLV
    [InlineData(8_000_000)]   // 8M < 10 SMLV
    [InlineData(9_000_000)]   // 9M < 10 SMLV
    [InlineData(12_000_000)]  // 12M < 10 SMLV
    public void CalcularTotalSeguridadSocial_DevengadoBajo_DebeAplicarExoneraciones(decimal totalDevengadoBajo)
    {
        // Arrange
        decimal factorRiesgo = 0.01044m;
        decimal baseSalarial = totalDevengadoBajo;
        decimal totalPrestacional = totalDevengadoBajo;
    
        // Act
        var detalleBajo = SeguridadSocialService.CalcularTotalSeguridadSocial(
            baseSalarial, totalDevengadoBajo, totalPrestacional, _salarioMinimo, factorRiesgo);
    
        // Assert
        var icbfBajo = detalleBajo.First(d => d.Nombre == "ICBF");
        var senaBajo = detalleBajo.First(d => d.Nombre == "SENA");
        var saludBajo = detalleBajo.First(d => d.Nombre == "Salud");
        var ccfBajo = detalleBajo.First(d => d.Nombre == "Caja de Compensación");
        var pensionBajo = detalleBajo.First(d => d.Nombre == "Pensión");
        var arlBajo = detalleBajo.First(d => d.Nombre == "ARL");
    
        // Calcular valores esperados para caso sin exoneración
        decimal valorEsperadoCaja = Math.Round(baseSalarial * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.CCF], 0, MidpointRounding.AwayFromZero);
        decimal valorEsperadoPension = Math.Round(baseSalarial * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.Pension], 0, MidpointRounding.AwayFromZero);
        decimal valorEsperadoArl = Math.Round(baseSalarial * factorRiesgo, 0, MidpointRounding.AwayFromZero);
    
        // Verificar exonerados en escenario de devengado bajo
        Assert.Equal(0m, icbfBajo.Valor);
        Assert.Equal(0m, senaBajo.Valor);
        Assert.Equal(0m, saludBajo.Valor);
    
        // CCF, Pensión y ARL no se exoneran
        Assert.Equal(valorEsperadoCaja, ccfBajo.Valor);
        Assert.Equal(valorEsperadoPension, pensionBajo.Valor);
        Assert.Equal(valorEsperadoArl, arlBajo.Valor);
    }
    
    [Theory]
    [InlineData(15_000_000)]  // 15M > 10 SMLV
    [InlineData(18_000_000)]  // 18M > 10 SMLV
    [InlineData(20_000_000)]  // 20M > 10 SMLV
    [InlineData(25_000_000)]  // 25M > 10 SMLV
    public void CalcularTotalSeguridadSocial_DevengadoAlto_NoDebeAplicarExoneraciones(decimal totalDevengadoAlto)
    {
        // Arrange
        decimal factorRiesgo = 0.01044m;
        decimal baseSalarial = totalDevengadoAlto;
        decimal totalPrestacional = totalDevengadoAlto;
    
        // Act
        var detalleAlto = SeguridadSocialService.CalcularTotalSeguridadSocial(
            baseSalarial, totalDevengadoAlto, totalPrestacional, _salarioMinimo, factorRiesgo);
    
        // Assert
        var icbfAlto = detalleAlto.First(d => d.Nombre == "ICBF");
        var senaAlto = detalleAlto.First(d => d.Nombre == "SENA");
        var saludAlto = detalleAlto.First(d => d.Nombre == "Salud");
    
        // Calcular valores esperados para caso sin exoneración
        decimal valorEsperadoSalud = Math.Round(baseSalarial * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.Salud], 0, MidpointRounding.AwayFromZero);
        decimal valorEsperadoICBF = Math.Round(baseSalarial * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.ICBF], 0, MidpointRounding.AwayFromZero);
        decimal valorEsperadoSENA = Math.Round(baseSalarial * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.SENA], 0, MidpointRounding.AwayFromZero);
    
        // Verificar NO exonerados en escenario de devengado alto
        Assert.Equal(valorEsperadoICBF, icbfAlto.Valor);
        Assert.Equal(valorEsperadoSENA, senaAlto.Valor);
        Assert.Equal(valorEsperadoSalud, saludAlto.Valor);
    }

    // PRUEBAS ESPECÍFICAS PARA SALARIOS INTEGRALES - AQUÍ SÍ SE APLICA EL 70%

    [Theory]
    [InlineData(15_000_000, 10_500_000)] // Salario integral 15M, base prestacional 70% = 10.5M
    [InlineData(20_000_000, 14_000_000)] // Salario integral 20M, base prestacional 70% = 14M
    [InlineData(30_000_000, 21_000_000)] // Salario integral 30M, base prestacional 70% = 21M
    public void Parafiscales_ConSalarioIntegral_DebeCalcularSobre70PorCiento(decimal salarioIntegral, decimal basePrestacionalEsperada)
    {
        // Arrange - CASO ESPECÍFICO: SALARIO INTEGRAL CON 70% PRESTACIONAL
        decimal totalDevengado = salarioIntegral;
        decimal totalSalarial = salarioIntegral; 
        decimal totalPrestacional = salarioIntegral * 0.7m; // 70% para salario integral
        
        // Act
        var caja = new ParafiscalesCajaCompensacion(totalPrestacional, totalDevengado, _salarioMinimo);
        var icbf = new ParafiscalesIcbf(totalPrestacional, totalDevengado, _salarioMinimo);
        var sena = new ParafiscalesSena(totalPrestacional, totalDevengado, _salarioMinimo);
        
        // Assert
        Assert.Equal(basePrestacionalEsperada, totalPrestacional);
        
        decimal valorEsperadoCaja = Math.Round(totalPrestacional * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.CCF], 0, MidpointRounding.AwayFromZero);
        decimal valorEsperadoICBF = Math.Round(totalPrestacional * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.ICBF], 0, MidpointRounding.AwayFromZero);
        decimal valorEsperadoSENA = Math.Round(totalPrestacional * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.SENA], 0, MidpointRounding.AwayFromZero);
        
        Assert.Equal(valorEsperadoCaja, caja.Valor);
        Assert.Equal(valorEsperadoICBF, icbf.Valor);
        Assert.Equal(valorEsperadoSENA, sena.Valor);
    }

    [Fact]
    public void SeguridadSocial_ConSalarioIntegral_DebeCalcularSobre70PorCiento()
    {
        // Arrange - CASO ESPECÍFICO: SALARIO INTEGRAL CON 70% PRESTACIONAL
        decimal salarioIntegral = 18_000_000;
        decimal totalSalarial = salarioIntegral;
        decimal totalDevengado = salarioIntegral;
        decimal totalPrestacional = salarioIntegral * 0.7m; // 70% para salario integral
        decimal factorRiesgo = 0.00522m; // Clase I
        
        // Act
        var salud = new SeguridadSocialSalud(totalSalarial, totalDevengado, totalPrestacional, _salarioMinimo);
        var pension = new SeguridadSocialPension(totalSalarial, totalDevengado, totalPrestacional, _salarioMinimo);
        var arl = new SeguridadSocialArl(totalSalarial, totalDevengado, totalPrestacional, _salarioMinimo, factorRiesgo);
        
        // Assert
        decimal valorEsperadoSalud = Math.Round(totalPrestacional * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.Salud], 0, MidpointRounding.AwayFromZero);
        decimal valorEsperadoPension = Math.Round(totalPrestacional * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.Pension], 0, MidpointRounding.AwayFromZero);
        decimal valorEsperadoArl = Math.Round(totalPrestacional * factorRiesgo, 0, MidpointRounding.AwayFromZero);
        
        Assert.Equal(valorEsperadoSalud, salud.Valor);
        Assert.Equal(valorEsperadoPension, pension.Valor);
        Assert.Equal(valorEsperadoArl, arl.Valor);
    }

    [Fact]
    public void ExoneracionParafiscales_ConSalarioIntegral_DebeAplicarseCorrectamente()
    {
        // Arrange - CASO ESPECÍFICO: SALARIO INTEGRAL CON 70% PRESTACIONAL
        decimal salarioIntegralBajo = 10_000_000;
        decimal salarioIntegralAlto = 25_000_000;
        
        decimal totalPrestacionalBajo = salarioIntegralBajo * 0.7m; // 70% = 7M
        decimal totalPrestacionalAlto = salarioIntegralAlto * 0.7m; // 70% = 17.5M
        
        decimal limite10SMLV = _salarioMinimo * 10; // 14.235M
        
        // Act
        var icbfBajo = new ParafiscalesIcbf(totalPrestacionalBajo, salarioIntegralBajo, _salarioMinimo);
        var senaBajo = new ParafiscalesSena(totalPrestacionalBajo, salarioIntegralBajo, _salarioMinimo);
        var saludBajo = new SeguridadSocialSalud(salarioIntegralBajo, salarioIntegralBajo, totalPrestacionalBajo, _salarioMinimo);
        
        var icbfAlto = new ParafiscalesIcbf(totalPrestacionalAlto, salarioIntegralAlto, _salarioMinimo);
        var senaAlto = new ParafiscalesSena(totalPrestacionalAlto, salarioIntegralAlto, _salarioMinimo);
        var saludAlto = new SeguridadSocialSalud(salarioIntegralAlto, salarioIntegralAlto, totalPrestacionalAlto, _salarioMinimo);
        
        // Assert
        // El salario integral bajo (10M) está por debajo de 10 SMLV, deberían aplicar exoneraciones
        Assert.Equal(0m, icbfBajo.Valor);
        Assert.Equal(0m, senaBajo.Valor);
        Assert.Equal(0m, saludBajo.Valor);
        
        // El salario integral alto (25M) está por encima de 10 SMLV, no deberían aplicar exoneraciones
        decimal valorEsperadoICBFAlto = Math.Round(totalPrestacionalAlto * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.ICBF], 0, MidpointRounding.AwayFromZero);
        decimal valorEsperadoSENAAlto = Math.Round(totalPrestacionalAlto * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.SENA], 0, MidpointRounding.AwayFromZero);
        decimal valorEsperadoSaludAlto = Math.Round(totalPrestacionalAlto * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.Salud], 0, MidpointRounding.AwayFromZero);
        
        Assert.Equal(valorEsperadoICBFAlto, icbfAlto.Valor);
        Assert.Equal(valorEsperadoSENAAlto, senaAlto.Valor);
        Assert.Equal(valorEsperadoSaludAlto, saludAlto.Valor);
    }

    [Fact]
    public void CalcularTotalSeguridadSocial_ConSalarioIntegral_DebeCalcularCorrectamente()
    {
        // Arrange - CASO ESPECÍFICO: SALARIO INTEGRAL CON 70% PRESTACIONAL
        decimal salarioIntegral = 20_000_000;
        decimal totalSalarial = salarioIntegral;
        decimal totalDevengado = salarioIntegral;
        decimal totalPrestacional = salarioIntegral * 0.7m; // 70% para salario integral
        decimal factorRiesgo = 0.01044m;
        
        // Act
        var detalles = SeguridadSocialService.CalcularTotalSeguridadSocial(
            totalSalarial, totalDevengado, totalPrestacional, _salarioMinimo, factorRiesgo);
        
        // Assert
        Assert.Equal(6, detalles.Count); // 3 seguridad social + 3 parafiscales
        
        var salud = detalles.First(t => t.Nombre == "Salud");
        var pension = detalles.First(t => t.Nombre == "Pensión");
        var arl = detalles.First(t => t.Nombre == "ARL");
        var caja = detalles.First(t => t.Nombre == "Caja de Compensación");
        var icbf = detalles.First(t => t.Nombre == "ICBF");
        var sena = detalles.First(t => t.Nombre == "SENA");
        
        // Calcular valores esperados usando el 70% del salario
        decimal valorEsperadoSalud = Math.Round(totalPrestacional * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.Salud], 0, MidpointRounding.AwayFromZero);
        decimal valorEsperadoPension = Math.Round(totalPrestacional * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.Pension], 0, MidpointRounding.AwayFromZero);
        decimal valorEsperadoArl = Math.Round(totalPrestacional * factorRiesgo, 0, MidpointRounding.AwayFromZero);
        decimal valorEsperadoCaja = Math.Round(totalPrestacional * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.CCF], 0, MidpointRounding.AwayFromZero);
        decimal valorEsperadoICBF = Math.Round(totalPrestacional * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.ICBF], 0, MidpointRounding.AwayFromZero);
        decimal valorEsperadoSENA = Math.Round(totalPrestacional * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.SENA], 0, MidpointRounding.AwayFromZero);
        
        Assert.Equal(valorEsperadoSalud, salud.Valor);
        Assert.Equal(valorEsperadoPension, pension.Valor);
        Assert.Equal(valorEsperadoArl, arl.Valor);
        Assert.Equal(valorEsperadoCaja, caja.Valor);
        Assert.Equal(valorEsperadoICBF, icbf.Valor);
        Assert.Equal(valorEsperadoSENA, sena.Valor);
        
        // Verificar que la suma total es correcta
        decimal totalEsperado = valorEsperadoSalud + valorEsperadoPension + valorEsperadoArl + 
                               valorEsperadoCaja + valorEsperadoICBF + valorEsperadoSENA;
        
        decimal totalActual = detalles.Sum(d => d.Valor);
        Assert.Equal(totalEsperado, totalActual);
    }
}