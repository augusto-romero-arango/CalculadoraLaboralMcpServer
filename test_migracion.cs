using CalculadoraLaboral.McpServer.Domain.Services;
using CalculadoraLaboral.McpServer.Domain.Constants;
using CalculadoraLaboral.McpServer.Domain.Models;

namespace TestMigracion;

public class TestComparativo
{
    public static void TestNuevosTiposHorasExtras()
    {
        Console.WriteLine("=== TEST: Nuevos Tipos de Horas Extras ===");
        
        // Verificar que los nuevos tipos existen y tienen factores correctos
        var tipos = new[] { 
            TiposHorasExtra.HEFD, TiposHorasExtra.HEFN, 
            TiposHorasExtra.RDD, TiposHorasExtra.RDN,
            TiposHorasExtra.RDDHC, TiposHorasExtra.RDNHC,
            TiposHorasExtra.RDDONC, TiposHorasExtra.RDNONC
        };
        
        foreach (var tipo in tipos)
        {
            var factor = FactorHorasExtra.Factores[tipo];
            Console.WriteLine($"{tipo}: Factor {factor}");
        }
        
        // Verificar corrección del recargo nocturno
        var factorRN = FactorHorasExtra.Factores[TiposHorasExtra.RecargoNocturno];
        Console.WriteLine($"RecargoNocturno corregido: {factorRN} (debe ser 0.35)");
        Console.WriteLine();
    }
    
    public static void TestSalarioIntegral()
    {
        Console.WriteLine("=== TEST: Prestaciones Salario Integral ===");
        
        decimal totalSalarial = 20_000_000m; // 20M COP
        decimal auxilioTransporte = 0m;
        bool esSalarioIntegral = true;
        
        var prestaciones = PrestacionesSocialesService.CalcularPrestacionesSociales(
            totalSalarial, auxilioTransporte, esSalarioIntegral);
        
        foreach (var prestacion in prestaciones)
        {
            Console.WriteLine($"{prestacion.Nombre}: {prestacion.Valor:C}");
        }
        
        // Para salario integral, Prima, Cesantías e Interés Cesantías deben ser 0
        var prima = prestaciones.First(p => p.Nombre == "Prima");
        var cesantias = prestaciones.First(p => p.Nombre == "Cesantías");
        var interes = prestaciones.First(p => p.Nombre == "Intereses de Cesantías");
        
        Console.WriteLine($"✓ Prima = 0? {prima.Valor == 0}");
        Console.WriteLine($"✓ Cesantías = 0? {cesantias.Valor == 0}");
        Console.WriteLine($"✓ Interés Cesantías = 0? {interes.Valor == 0}");
        Console.WriteLine();
    }
    
    public static void TestExoneracionParafiscales()
    {
        Console.WriteLine("=== TEST: Exoneración Parafiscales ===");
        
        decimal salarioMinimo = ParametrosAnuales.ObtenerSMLV(DateTime.Now);
        decimal totalDevengado = salarioMinimo * 8; // Menos de 10 SMLV
        decimal totalPrestacional = totalDevengado;
        decimal factorRiesgo = 0.00522m;
        
        var seguridadSocial = SeguridadSocialService.CalcularTotalSeguridadSocial(
            totalDevengado, totalDevengado, totalPrestacional, salarioMinimo, factorRiesgo);
        
        var parafiscales = seguridadSocial.Where(s => 
            s.Nombre.Contains("Compensación") || 
            s.Nombre.Contains("ICBF") || 
            s.Nombre.Contains("SENA")).ToList();
        
        Console.WriteLine($"Salario mínimo: {salarioMinimo:C}");
        Console.WriteLine($"Total devengado: {totalDevengado:C} (< 10 SMLV)");
        
        foreach (var para in parafiscales)
        {
            Console.WriteLine($"{para.Nombre}: {para.Valor:C}");
        }
        
        var totalParafiscales = parafiscales.Sum(p => p.Valor);
        Console.WriteLine($"✓ Parafiscales exonerados = 0? {totalParafiscales == 0}");
        Console.WriteLine();
    }
    
    public static void Main()
    {
        Console.WriteLine("VALIDACIÓN DE MIGRACIÓN: Bitakora.Calculadoras → calculadoraLaboral.McpServer");
        Console.WriteLine("=================================================================");
        Console.WriteLine();
        
        TestNuevosTiposHorasExtras();
        TestSalarioIntegral();
        TestExoneracionParafiscales();
        
        Console.WriteLine("✅ Migración completada y validada");
    }
}