using CalculadoraLaboral.McpServer.Domain.Services;
using CalculadoraLaboral.McpServer.Domain.Constants;
using CalculadoraLaboral.McpServer.Domain.Models;

namespace CalculadoraLaboral.Tests;

public class IntegracionTests
{
    private readonly decimal _salarioMinimo = ParametrosAnuales.ObtenerSMLV(DateTime.Now);

    [Fact]
    public void LiquidacionCompleta_SalarioOrdinario_DebeCalcularTodosLosComponentes()
    {
        // Arrange - Empleado con salario ordinario
        decimal salarioBasico = 3000000m;
        decimal pagosSalariales = 500000m;
        decimal pagosNoSalariales = 200000m;
        decimal auxilioTransporte = ParametrosAnuales.ObtenerAuxilioTransporte(DateTime.Now);
        bool esSalarioIntegral = false;
        var riesgoLaboral = ClasesDeRiesgo.II;
        decimal factorRiesgo = 0.01044m;

        // Act - Calcular remuneración
        var remuneracion = new RemuneracionService(
            salarioBasico, 
            TipoSalario.Ordinario, 
            DateTime.Now, 
            pagosSalariales, 
            0, // horas extras
            pagosNoSalariales);

        var totalSalarial = remuneracion.TotalSalarial;
        var totalDevengado = remuneracion.TotalDevengado;
        var totalPrestacional = remuneracion.TotalPrestacional;

        // Act - Calcular prestaciones
        var prestaciones = PrestacionesSocialesService.CalcularPrestacionesSociales(
            totalSalarial, auxilioTransporte, esSalarioIntegral);

        // Act - Calcular seguridad social
        var seguridadSocial = SeguridadSocialService.CalcularTotalSeguridadSocial(
            totalSalarial, totalDevengado, totalPrestacional, _salarioMinimo, factorRiesgo);

        // Assert - Verificar componentes principales
        Assert.True(totalSalarial > 0);
        Assert.True(totalDevengado >= totalSalarial);
        Assert.True(totalPrestacional > 0);

        // Assert - Verificar prestaciones (todas deben ser > 0 para salario ordinario)
        Assert.Equal(4, prestaciones.Count);
        foreach (var prestacion in prestaciones)
        {
            Assert.True(prestacion.Valor > 0, $"{prestacion.Nombre} debe ser > 0 para salario ordinario");
        }

        // Assert - Verificar seguridad social (6 componentes)
        Assert.Equal(6, seguridadSocial.Count);
        foreach (var componente in seguridadSocial)
        {
            Assert.True(componente.Valor >= 0, $"{componente.Nombre} debe ser >= 0");
        }
    }

    [Fact]
    public void LiquidacionCompleta_SalarioIntegral_DebeAplicarReglasEspeciales()
    {
        // Arrange - Empleado con salario integral
        decimal salarioBasico = 20000000m; // 20M (> 13 SMLV)
        decimal pagosSalariales = 0m;
        decimal pagosNoSalariales = 1000000m;
        decimal auxilioTransporte = 0m; // No aplica para salario integral
        bool esSalarioIntegral = true;
        var riesgoLaboral = ClasesDeRiesgo.III;
        decimal factorRiesgo = 0.02436m;

        // Act - Calcular remuneración
        var remuneracion = new RemuneracionService(
            salarioBasico, 
            TipoSalario.Integral, 
            DateTime.Now, 
            pagosSalariales, 
            0, // horas extras
            pagosNoSalariales);

        var totalSalarial = remuneracion.TotalSalarial;
        var totalDevengado = remuneracion.TotalDevengado;
        var totalPrestacional = remuneracion.TotalPrestacional;

        // Act - Calcular prestaciones
        var prestaciones = PrestacionesSocialesService.CalcularPrestacionesSociales(
            totalSalarial, auxilioTransporte, esSalarioIntegral);

        // Act - Calcular seguridad social
        var seguridadSocial = SeguridadSocialService.CalcularTotalSeguridadSocial(
            totalSalarial, totalDevengado, totalPrestacional, _salarioMinimo, factorRiesgo);

        // Assert - Verificar factor prestacional 0.7
        decimal factorEsperado = totalSalarial * 0.7m;
        Assert.Equal(factorEsperado, totalPrestacional);

        // Assert - Verificar prestaciones (Prima, Cesantías e Interés = 0)
        var prima = prestaciones.First(p => p.Nombre == "Prima");
        var cesantias = prestaciones.First(p => p.Nombre == "Cesantías");
        var interes = prestaciones.First(p => p.Nombre == "Intereses de Cesantías");
        var vacaciones = prestaciones.First(p => p.Nombre == "Vacaciones");

        Assert.Equal(0, prima.Valor);
        Assert.Equal(0, cesantias.Valor);
        Assert.Equal(0, interes.Valor);
        Assert.True(vacaciones.Valor > 0);

        // Assert - Verificar que seguridad social se calcula normalmente
        var totalSeguridadSocial = seguridadSocial.Sum(s => s.Valor);
        Assert.True(totalSeguridadSocial > 0);
    }

    [Fact]
    public void LiquidacionCompleta_EmpleadoBajosIngresos_DebeAplicarExoneraciones()
    {
        // Arrange - Empleado con ingresos < 10 SMLV
        decimal salarioBasico = 2000000m;
        decimal pagosSalariales = 1000000m;
        decimal pagosNoSalariales = 500000m;
        decimal auxilioTransporte = ParametrosAnuales.ObtenerAuxilioTransporte(DateTime.Now);
        bool esSalarioIntegral = false;
        decimal factorRiesgo = 0.00522m;

        // Total devengado = 3.7M (< 10 SMLV ≈ 14.2M)

        // Act
        var remuneracion = new RemuneracionService(
            salarioBasico, 
            TipoSalario.Ordinario, 
            DateTime.Now, 
            pagosSalariales, 
            0, // horas extras
            pagosNoSalariales);

        var totalDevengado = remuneracion.TotalDevengado;
        var totalSalarial = remuneracion.TotalSalarial;
        var totalPrestacional = remuneracion.TotalPrestacional;

        var seguridadSocial = SeguridadSocialService.CalcularTotalSeguridadSocial(
            totalSalarial, totalDevengado, totalPrestacional, _salarioMinimo, factorRiesgo);

        // Assert - Verificar que está por debajo del tope
        Assert.True(totalDevengado < _salarioMinimo * 10);

        // Assert - Parafiscales deben estar exonerados (valor = 0)
        var parafiscales = seguridadSocial.Where(s => 
            s.Nombre.Contains("Compensación") || 
            s.Nombre == "ICBF" || 
            s.Nombre == "SENA").ToList();

        foreach (var para in parafiscales)
        {
            Assert.Equal(0, para.Valor);
        }

        // Assert - Seguridad social debe calcularse normalmente
        var seguridadSocialItems = seguridadSocial.Where(s => 
            s.Nombre == "Salud" || 
            s.Nombre == "Pensión" || 
            s.Nombre == "ARL").ToList();

        foreach (var ss in seguridadSocialItems)
        {
            Assert.True(ss.Valor > 0, $"{ss.Nombre} debe calcularse normalmente");
        }
    }

    [Fact]
    public void LiquidacionCompleta_EmpleadoAltosIngresos_DebeAplicarLey1393()
    {
        // Arrange - Empleado con altos ingresos no salariales
        decimal salarioBasico = 5000000m;
        decimal pagosSalariales = 2000000m;
        decimal pagosNoSalariales = 8000000m; // Alto componente no salarial
        decimal auxilioTransporte = 0m; // No aplica
        bool esSalarioIntegral = false;
        decimal factorRiesgo = 0.0435m; // Clase IV

        // Total salarial = 7M, Total devengado = 15M
        // 60% devengado = 9M > 7M salarial, entonces aplica Ley 1393

        // Act
        var remuneracion = new RemuneracionService(
            salarioBasico, 
            TipoSalario.Ordinario, 
            DateTime.Now, 
            pagosSalariales, 
            0, // horas extras
            pagosNoSalariales);

        var totalSalarial = remuneracion.TotalSalarial;
        var totalDevengado = remuneracion.TotalDevengado;
        var totalPrestacional = remuneracion.TotalPrestacional;

        var seguridadSocial = SeguridadSocialService.CalcularTotalSeguridadSocial(
            totalSalarial, totalDevengado, totalPrestacional, _salarioMinimo, factorRiesgo);

        // Assert - Verificar condiciones para Ley 1393
        var base1393 = totalDevengado * 0.6m;
        Assert.True(base1393 > totalSalarial, "Debe aplicar Ley 1393");

        // Assert - Los valores de seguridad social deben ser mayores debido al ajuste
        var totalSeguridadSocialValor = seguridadSocial.Where(s => 
            s.Nombre == "Salud" || 
            s.Nombre == "Pensión" || 
            s.Nombre == "ARL").Sum(s => s.Valor);

        Assert.True(totalSeguridadSocialValor > 0);

        // Verificar que no excede el tope de 25 SMLV
        var salud = seguridadSocial.First(s => s.Nombre == "Salud");
        var valorMaximoSalud = Math.Round(_salarioMinimo * 25 * 0.085m, 0, MidpointRounding.AwayFromZero);
        Assert.True(salud.Valor <= valorMaximoSalud);
    }

    [Fact]
    public void CalculoHorasExtras_NuevosTipos_DebeCalcularCorrectamente()
    {
        // Arrange
        var valorHoraOrdinaria = 15000m;
        var cantidadHoras = 10m;

        // Act & Assert para los nuevos tipos
        var horasExtrasItems = new Dictionary<TiposHorasExtra, int>
        {
            { TiposHorasExtra.HEFD, (int)cantidadHoras },
            { TiposHorasExtra.HEFN, (int)cantidadHoras },
            { TiposHorasExtra.RDD, (int)cantidadHoras }
        };
        
        var horasExtrasService = new HorasExtrasService(valorHoraOrdinaria, horasExtrasItems);
        
        // HEFD - Hora extra festiva diurna (factor 2.05)
        var valorHEFD = horasExtrasService.ObtenerValorHoraPorItem(TiposHorasExtra.HEFD);
        var esperadoHEFD = Math.Round(cantidadHoras * 2.05m * valorHoraOrdinaria, 0, MidpointRounding.AwayFromZero);
        Assert.Equal(esperadoHEFD, valorHEFD);

        // HEFN - Hora extra festiva nocturna (factor 2.55)
        var valorHEFN = horasExtrasService.ObtenerValorHoraPorItem(TiposHorasExtra.HEFN);
        var esperadoHEFN = Math.Round(cantidadHoras * 2.55m * valorHoraOrdinaria, 0, MidpointRounding.AwayFromZero);
        Assert.Equal(esperadoHEFN, valorHEFN);

        // RDD - Recargo dominical diurno ocasional compensado (factor 0.80)
        var valorRDD = horasExtrasService.ObtenerValorHoraPorItem(TiposHorasExtra.RDD);
        var esperadoRDD = Math.Round(cantidadHoras * 0.80m * valorHoraOrdinaria, 0, MidpointRounding.AwayFromZero);
        Assert.Equal(esperadoRDD, valorRDD);
    }

    [Fact]
    public void ParametrosAnuales_DebenEstarCompletos()
    {
        // Act & Assert - Verificar años disponibles
        var fechas = new[]
        {
            new DateTime(2022, 6, 1),
            new DateTime(2023, 6, 1),
            new DateTime(2024, 6, 1),
            new DateTime(2025, 6, 1),
            new DateTime(2026, 6, 1)
        };

        foreach (var fecha in fechas)
        {
            // No debe lanzar excepciones
            var smlv = ParametrosAnuales.ObtenerSMLV(fecha);
            var auxilio = ParametrosAnuales.ObtenerAuxilioTransporte(fecha);
            var horas = ParametrosAnuales.ObtenerCantidadHorasJornada(fecha);

            Assert.True(smlv > 0, $"SMLV para {fecha.Year} debe ser > 0");
            Assert.True(auxilio > 0, $"Auxilio transporte para {fecha.Year} debe ser > 0");
            Assert.True(horas > 0, $"Horas jornada para {fecha.Year} debe ser > 0");
        }
    }

    [Fact]
    public void MigracionCompleta_ComparacionValores_DebeSerConsistente()
    {
        // Este test verifica que la migración mantiene consistencia
        // en los cálculos comparando valores esperados

        // Arrange
        decimal salarioBasico = 2500000m;
        decimal pagosSalariales = 500000m;
        decimal pagosNoSalariales = 300000m;
        decimal auxilioTransporte = 200000m;
        bool esSalarioIntegral = false;
        decimal factorRiesgo = 0.01044m;

        // Act - Calcular todo
        var remuneracion = new RemuneracionService(
            salarioBasico, 
            TipoSalario.Ordinario, 
            DateTime.Now, 
            pagosSalariales, 
            0, // horas extras
            pagosNoSalariales);

        var totalSalarial = remuneracion.TotalSalarial;
        var totalDevengado = remuneracion.TotalDevengado;
        var totalPrestacional = remuneracion.TotalPrestacional;

        var prestaciones = PrestacionesSocialesService.CalcularPrestacionesSociales(
            totalSalarial, auxilioTransporte, esSalarioIntegral);

        var seguridadSocial = SeguridadSocialService.CalcularTotalSeguridadSocial(
            totalSalarial, totalDevengado, totalPrestacional, _salarioMinimo, factorRiesgo);

        // Assert - Valores esperados según lógica TypeScript
        Assert.Equal(3000000m, totalSalarial); // 2.5M + 0.5M
        Assert.Equal(3300000m, totalDevengado); // 3M + 0.3M
        Assert.Equal(3000000m, totalPrestacional); // Factor 1.0 para ordinario

        // Verificar que prestaciones son > 0
        Assert.True(prestaciones.All(p => p.Valor > 0));

        // Verificar aplicación correcta de exoneración (< 10 SMLV)
        var parafiscales = seguridadSocial.Where(s => 
            s.Nombre.Contains("Compensación") || 
            s.Nombre == "ICBF" || 
            s.Nombre == "SENA").ToList();
        Assert.True(parafiscales.All(p => p.Valor == 0), "Parafiscales deben estar exonerados");

        // Verificar que seguridad social se calcula
        var seguridadSocialItems = seguridadSocial.Where(s => 
            s.Nombre == "Salud" || 
            s.Nombre == "Pensión" || 
            s.Nombre == "ARL").ToList();
        Assert.True(seguridadSocialItems.All(s => s.Valor > 0), "Seguridad social debe calcularse");
    }
}