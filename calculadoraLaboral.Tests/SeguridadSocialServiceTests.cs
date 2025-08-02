using CalculadoraLaboral.McpServer.Domain.Services;
using CalculadoraLaboral.McpServer.Domain.Constants;

namespace CalculadoraLaboral.Tests;

public class SeguridadSocialServiceTests
{
    private readonly decimal _salarioMinimo = 1_423_500m; // SMLV 2025
    private readonly decimal _factorRiesgoLaboral = 0.00522m; // Factor típico de ARL

    [Fact]
    public void CalcularSeguridadSocial_SalarioNormal_DebeCalcularCorrectamente()
    {
        // Arrange
        decimal totalSalarial = 5_000_000m;
        decimal totalDevengado = 5_000_000m; // Igual para salario normal
        decimal totalPrestacional = 5_000_000m; // Igual para salario normal

        // Act
        var seguridadSocial = SeguridadSocialService.CalcularSeguridadSocial(
            totalSalarial, totalDevengado, totalPrestacional, _salarioMinimo, _factorRiesgoLaboral);

        // Assert
        Assert.Equal(3, seguridadSocial.Count);
        
        var salud = seguridadSocial.First(s => s.Nombre == "Salud");
        var pension = seguridadSocial.First(s => s.Nombre == "Pensión");
        var arl = seguridadSocial.First(s => s.Nombre == "ARL");

        // Para salario normal, todos los valores son iguales, por lo que la base de cálculo es totalPrestacional
        decimal baseEsperada = totalPrestacional;

        decimal valorEsperadoSalud = 0; // La salud es exonerada con devengado < 10 SMLV
        decimal valorEsperadoPension = Math.Round(baseEsperada * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.Pension], 0, MidpointRounding.AwayFromZero);
        decimal valorEsperadoArl = Math.Round(baseEsperada * _factorRiesgoLaboral, 0, MidpointRounding.AwayFromZero);

        Assert.Equal(valorEsperadoSalud, salud.Valor);
        Assert.Equal(valorEsperadoPension, pension.Valor);
        Assert.Equal(valorEsperadoArl, arl.Valor);
    }

    [Fact]
    public void CalcularSeguridadSocial_SalarioIntegral_DebeAplicarFactor70Porciento()
    {
        // Arrange - Salario integral (> 13 SMLV)
        decimal totalSalarial = 20_000_000m; // Mayor a 13 SMLV (18.5M)
        decimal totalPrestacional = totalSalarial * 0.70m; // 70% para salario integral
        decimal totalDevengado = totalSalarial; // Para salario integral sin Ley 1393

        // Act
        var seguridadSocial = SeguridadSocialService.CalcularSeguridadSocial(
            totalSalarial, totalDevengado, totalPrestacional, _salarioMinimo, _factorRiesgoLaboral);

        // Assert
        Assert.Equal(3, seguridadSocial.Count);
        
        var salud = seguridadSocial.First(s => s.Nombre == "Salud");
        var pension = seguridadSocial.First(s => s.Nombre == "Pensión");
        var arl = seguridadSocial.First(s => s.Nombre == "ARL");

        // Para salario integral sin Ley 1393, la base es totalPrestacional (70%)
        decimal baseEsperada = totalPrestacional;
        
        decimal valorEsperadoSalud = Math.Round(baseEsperada * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.Salud], 0, MidpointRounding.AwayFromZero);
        decimal valorEsperadoPension = Math.Round(baseEsperada * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.Pension], 0, MidpointRounding.AwayFromZero);
        decimal valorEsperadoArl = Math.Round(baseEsperada * _factorRiesgoLaboral, 0, MidpointRounding.AwayFromZero);

        Assert.Equal(valorEsperadoSalud, salud.Valor);
        Assert.Equal(valorEsperadoPension, pension.Valor);
        Assert.Equal(valorEsperadoArl, arl.Valor);
    }

    [Fact]
    public void CalcularSeguridadSocial_Ley1393_SalarioNormal_DebeAplicarFormula1393()
    {
        // Arrange - Ley 1393: devengado diferente del salarial
        decimal totalSalarial = 3_000_000m;
        decimal totalDevengado = 5_000_000m; // Incluye horas extras, bonificaciones, etc.
        decimal totalPrestacional = 3_000_000m; // Igual al salarial para salario normal

        // Act
        var seguridadSocial = SeguridadSocialService.CalcularSeguridadSocial(
            totalSalarial, totalDevengado, totalPrestacional, _salarioMinimo, _factorRiesgoLaboral);

        // Assert
        Assert.Equal(3, seguridadSocial.Count);
        
        var salud = seguridadSocial.First(s => s.Nombre == "Salud");
        var pension = seguridadSocial.First(s => s.Nombre == "Pensión");
        var arl = seguridadSocial.First(s => s.Nombre == "ARL");

        // Cálculo Ley 1393
        decimal base1393 = totalDevengado * 0.6m; // 60% del devengado
        decimal ajuste1393 = base1393 > totalSalarial ? base1393 - totalSalarial : 0;
        decimal baseEsperada = ajuste1393 + totalPrestacional;

        decimal valorEsperadoSalud = 0;
        decimal valorEsperadoPension = Math.Round(baseEsperada * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.Pension], 0, MidpointRounding.AwayFromZero);
        decimal valorEsperadoArl = Math.Round(baseEsperada * _factorRiesgoLaboral, 0, MidpointRounding.AwayFromZero);

        Assert.Equal(valorEsperadoSalud, salud.Valor);
        Assert.Equal(valorEsperadoPension, pension.Valor);
        Assert.Equal(valorEsperadoArl, arl.Valor);
    }

    [Fact]
    public void CalcularSeguridadSocial_Ley1393_SinAjuste_DebeUsarSoloPrestacional()
    {
        // Arrange - Caso donde 60% del devengado es menor al salarial
        decimal totalSalarial = 4_000_000m;
        decimal totalDevengado = 5_000_000m; // 60% = 3M < 4M salarial
        decimal totalPrestacional = 4_000_000m;

        // Act
        var seguridadSocial = SeguridadSocialService.CalcularSeguridadSocial(
            totalSalarial, totalDevengado, totalPrestacional, _salarioMinimo, _factorRiesgoLaboral);

        // Assert
        Assert.Equal(3, seguridadSocial.Count);
        
        var salud = seguridadSocial.First(s => s.Nombre == "Salud");
        var pension = seguridadSocial.First(s => s.Nombre == "Pensión");
        var arl = seguridadSocial.First(s => s.Nombre == "ARL");
        
        // 60% de 5M = 3M, que es menor a 4M salarial, entonces ajuste = 0
        decimal ajuste1393 = 0; // base1393 (3M) < totalSalarial (4M)
        decimal baseEsperada = ajuste1393 + totalPrestacional; // 0 + 4M = 4M

        decimal valorEsperadoSalud = 0; //Salud está exonerada en este caso
        decimal valorEsperadoPension = Math.Round(baseEsperada * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.Pension], 0, MidpointRounding.AwayFromZero);
        decimal valorEsperadoArl = Math.Round(baseEsperada * _factorRiesgoLaboral, 0, MidpointRounding.AwayFromZero);
        
        Assert.Equal(valorEsperadoSalud, salud.Valor);
        Assert.Equal(valorEsperadoPension, pension.Valor);
        Assert.Equal(valorEsperadoArl, arl.Valor);
    }

    [Fact]
    public void CalcularSeguridadSocial_Ley1393_SalarioIntegral_DebeAplicarFormula1393ConFactor70()
    {
        // Arrange - Ley 1393 con salario integral (> 13 SMLV)
        decimal totalSalarial = 20_000_000m; // Mayor a 13 SMLV (18.5M)
        decimal totalPrestacional = totalSalarial * 0.70m; // 70% para salario integral
        decimal totalDevengado = 25_000_000m; // Incluye horas extras, bonificaciones, etc.

        // Act
        var seguridadSocial = SeguridadSocialService.CalcularSeguridadSocial(
            totalSalarial, totalDevengado, totalPrestacional, _salarioMinimo, _factorRiesgoLaboral);

        // Assert
        var salud = seguridadSocial.First(s => s.Nombre == "Salud");
        var pension = seguridadSocial.First(s => s.Nombre == "Pensión");
        var arl = seguridadSocial.First(s => s.Nombre == "ARL");

        // Cálculo Ley 1393 con salario integral
        decimal base1393 = totalDevengado * 0.6m; // 60% del devengado = 15M
        decimal ajuste1393 = base1393 > totalSalarial ? base1393 - totalSalarial : 0; // 15M - 20M = 0 (no hay ajuste)
        decimal baseEsperada = ajuste1393 + totalPrestacional; // 0 + 14M = 14M

        decimal valorEsperadoSalud = Math.Round(baseEsperada * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.Salud], 0, MidpointRounding.AwayFromZero);
        decimal valorEsperadoPension = Math.Round(baseEsperada * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.Pension], 0, MidpointRounding.AwayFromZero);
        decimal valorEsperadoArl = Math.Round(baseEsperada * _factorRiesgoLaboral, 0, MidpointRounding.AwayFromZero);

        Assert.Equal(valorEsperadoSalud, salud.Valor);
        Assert.Equal(valorEsperadoPension, pension.Valor);
        Assert.Equal(valorEsperadoArl, arl.Valor);
    }

    [Fact]
    public void CalcularSeguridadSocial_Ley1393_SalarioIntegral_ConAjuste_DebeCalcularCorrectamente()
    {
        // Arrange - Ley 1393 con salario integral (> 13 SMLV) donde sí hay ajuste
        decimal totalSalarial = 20_000_000m; // Mayor a 13 SMLV (18.5M)
        decimal totalPrestacional = totalSalarial * 0.70m; // 70% = 14M
        decimal totalDevengado = 40_000_000m; // Muy alto para forzar ajuste

        // Act
        var seguridadSocial = SeguridadSocialService.CalcularSeguridadSocial(
            totalSalarial, totalDevengado, totalPrestacional, _salarioMinimo, _factorRiesgoLaboral);

        // Assert
        var salud = seguridadSocial.First(s => s.Nombre == "Salud");

        // Cálculo Ley 1393 con ajuste
        decimal base1393 = totalDevengado * 0.6m; // 60% de 40M = 24M
        decimal ajuste1393 = base1393 > totalSalarial ? base1393 - totalSalarial : 0; // 24M - 20M = 4M
        decimal baseEsperada = ajuste1393 + totalPrestacional; // 4M + 14M = 18M

        decimal valorEsperadoSalud = Math.Round(baseEsperada * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.Salud], 0, MidpointRounding.AwayFromZero);
        
        Assert.Equal(valorEsperadoSalud, salud.Valor);
    }

    [Fact]
    public void CalcularSeguridadSocial_ConTopeMaximo_DebeAplicarLimite25SMLV()
    {
        // Arrange - Valores muy altos para probar tope
        decimal totalSalarial = 50_000_000m;
        decimal totalPrestacional = 50_000_000m;
        decimal totalDevengado = 80_000_000m;

        // Act
        var seguridadSocial = SeguridadSocialService.CalcularSeguridadSocial(
            totalSalarial, totalDevengado, totalPrestacional, _salarioMinimo, _factorRiesgoLaboral);

        // Assert
        var salud = seguridadSocial.First(s => s.Nombre == "Salud");

        // Tope máximo de 25 SMLV
        decimal topeMaximo = _salarioMinimo * 25;
        
        // Cálculo con Ley 1393
        decimal base1393 = totalDevengado * 0.6m; // 48M
        decimal ajuste1393 = base1393 > totalSalarial ? base1393 - totalSalarial : 0; // 48M - 50M = 0
        decimal baseCalculada = ajuste1393 + totalPrestacional; // 0 + 50M = 50M
        
        // Aplicar tope
        decimal baseEsperada = baseCalculada > topeMaximo ? topeMaximo : baseCalculada;

        decimal valorEsperadoSalud = Math.Round(baseEsperada * TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.Salud], 0, MidpointRounding.AwayFromZero);
        
        Assert.Equal(valorEsperadoSalud, salud.Valor);
        Assert.True(baseEsperada <= topeMaximo, "La base de cálculo debe respetar el tope de 25 SMLV");
    }

    [Fact]
    public void CalcularSeguridadSocial_ConExoneracion_DebeAplicarTarifaExonerada()
    {
        // Arrange - Devengado menor a 10 SMLV para exoneración
        decimal totalSalarial = 5_000_000m;
        decimal totalDevengado = 7_000_000m; // < 10 SMLV
        decimal totalPrestacional = 5_000_000m;

        // Act
        var seguridadSocial = SeguridadSocialService.CalcularSeguridadSocial(
            totalSalarial, totalDevengado, totalPrestacional, _salarioMinimo, _factorRiesgoLaboral);

        // Assert
        var pension = seguridadSocial.First(s => s.Nombre == "Pensión");
        
        // Verificar que está por debajo del tope de exoneración
        decimal topeExoneracion = _salarioMinimo * 10;
        Assert.True(totalDevengado < topeExoneracion, "El devengado debe estar por debajo del tope de exoneración");
        
        // Para pensión, la tarifa exonerada es igual a la normal
        // Para salud, no hay exoneración (tarifa exonerada = 0)
        Assert.True(pension.Valor > 0, "Pensión debe tener valor incluso con exoneración");
    }

    [Fact]
    public void CalcularTotalSeguridadSocial_DebeRetornarListaCompleta()
    {
        // Arrange
        decimal totalSalarial = 5_000_000m;
        decimal totalDevengado = 6_000_000m;
        decimal totalPrestacional = 5_000_000m;

        // Act
        var resultado = SeguridadSocialService.CalcularTotalSeguridadSocial(
            totalSalarial, totalDevengado, totalPrestacional, _salarioMinimo, _factorRiesgoLaboral);

        // Assert
        Assert.Equal(6, resultado.Count); // 3 seguridad social + 3 parafiscales
        
        // Verificar que están todos los conceptos
        Assert.Contains(resultado, r => r.Nombre == "Salud");
        Assert.Contains(resultado, r => r.Nombre == "Pensión");
        Assert.Contains(resultado, r => r.Nombre == "ARL");
        Assert.Contains(resultado, r => r.Nombre == "Caja de Compensación");
        Assert.Contains(resultado, r => r.Nombre == "ICBF");
        Assert.Contains(resultado, r => r.Nombre == "SENA");
    }

    [Fact]
    public void CalcularTotalValorSeguridadSocial_DebeRetornarSumaCorrecta()
    {
        // Arrange
        decimal totalSalarial = 5_000_000m;
        decimal totalDevengado = 6_000_000m;
        decimal totalPrestacional = 5_000_000m;

        // Act
        var valorTotal = SeguridadSocialService.CalcularTotalValorSeguridadSocial(
            totalSalarial, totalDevengado, totalPrestacional, _salarioMinimo, _factorRiesgoLaboral);

        var detalles = SeguridadSocialService.CalcularTotalSeguridadSocial(
            totalSalarial, totalDevengado, totalPrestacional, _salarioMinimo, _factorRiesgoLaboral);

        // Assert
        decimal sumaEsperada = detalles.Sum(d => d.Valor);
        Assert.Equal(sumaEsperada, valorTotal);
        Assert.True(valorTotal > 0, "El valor total debe ser positivo");
    }
}
