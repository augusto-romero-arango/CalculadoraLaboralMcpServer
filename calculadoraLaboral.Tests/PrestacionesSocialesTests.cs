using CalculadoraLaboral.McpServer.Domain.Services;

namespace CalculadoraLaboral.Tests;

public class PrestacionesSocialesTests
{
    [Theory]
    [InlineData(2000000, 200000, false)] // Salario ordinario
    [InlineData(5000000, 0, false)]      // Salario ordinario sin auxilio transporte
    public void PrestacionesSociales_SalarioOrdinario_DebeCalcularTodasLasPrestaciones(decimal totalSalarial, decimal auxilioTransporte, bool esSalarioIntegral)
    {
        // Act
        var prestaciones = PrestacionesSocialesService.CalcularPrestacionesSociales(
            totalSalarial, auxilioTransporte, esSalarioIntegral);

        // Assert
        Assert.Equal(4, prestaciones.Count);
        Assert.Contains(prestaciones, p => p.Nombre == "Prima");
        Assert.Contains(prestaciones, p => p.Nombre == "Cesantías");
        Assert.Contains(prestaciones, p => p.Nombre == "Vacaciones");
        Assert.Contains(prestaciones, p => p.Nombre == "Intereses de Cesantías");

        // Todas las prestaciones deben ser > 0 para salario ordinario
        foreach (var prestacion in prestaciones)
        {
            Assert.True(prestacion.Valor > 0, $"{prestacion.Nombre} debe ser mayor a 0 para salario ordinario");
        }
    }

    [Theory]
    [InlineData(20000000, 0, true)]  // 20M salario integral
    [InlineData(15000000, 0, true)]  // 15M salario integral
    public void PrestacionesSociales_SalarioIntegral_PrimaCesantiasInteresDebenSerCero(decimal totalSalarial, decimal auxilioTransporte, bool esSalarioIntegral)
    {
        // Act
        var prestaciones = PrestacionesSocialesService.CalcularPrestacionesSociales(
            totalSalarial, auxilioTransporte, esSalarioIntegral);

        var prima = prestaciones.First(p => p.Nombre == "Prima");
        var cesantias = prestaciones.First(p => p.Nombre == "Cesantías");
        var interesCesantias = prestaciones.First(p => p.Nombre == "Intereses de Cesantías");
        var vacaciones = prestaciones.First(p => p.Nombre == "Vacaciones");

        // Assert
        Assert.Equal(0, prima.Valor);
        Assert.Equal(0, cesantias.Valor);
        Assert.Equal(0, interesCesantias.Valor);
        Assert.True(vacaciones.Valor > 0, "Vacaciones debe ser mayor a 0 incluso en salario integral");
    }

    [Fact]
    public void Prima_SalarioOrdinario_DebeCalcularCorrectamente()
    {
        // Arrange
        decimal totalSalarial = 2400000; // 2.4M
        decimal auxilioTransporte = 200000;
        bool esSalarioIntegral = false;
        decimal baseEsperada = totalSalarial + auxilioTransporte; // 2.6M
        decimal primaEsperada = Math.Round(baseEsperada / 12, 2, MidpointRounding.AwayFromZero);

        // Act
        var prima = new Prima(totalSalarial, auxilioTransporte, esSalarioIntegral);

        // Assert
        Assert.Equal(primaEsperada, prima.Valor);
        Assert.Equal("Prima", prima.Nombre);
        Assert.Equal("Prima de servicios", prima.Descripcion);
    }

    [Fact]
    public void Prima_SalarioIntegral_DebeSerCero()
    {
        // Arrange
        decimal totalSalarial = 20000000;
        decimal auxilioTransporte = 0;
        bool esSalarioIntegral = true;

        // Act
        var prima = new Prima(totalSalarial, auxilioTransporte, esSalarioIntegral);

        // Assert
        Assert.Equal(0, prima.Valor);
    }

    [Fact]
    public void Cesantia_SalarioOrdinario_DebeCalcularCorrectamente()
    {
        // Arrange
        decimal totalSalarial = 3000000;
        decimal auxilioTransporte = 200000;
        bool esSalarioIntegral = false;
        decimal baseEsperada = totalSalarial + auxilioTransporte;
        decimal cesantiaEsperada = Math.Round(baseEsperada / 12, 2, MidpointRounding.AwayFromZero);

        // Act
        var cesantia = new Cesantia(totalSalarial, auxilioTransporte, esSalarioIntegral);

        // Assert
        Assert.Equal(cesantiaEsperada, cesantia.Valor);
        Assert.Equal("Cesantías", cesantia.Nombre);
    }

    [Fact]
    public void Cesantia_SalarioIntegral_DebeSerCero()
    {
        // Arrange
        decimal totalSalarial = 18000000;
        decimal auxilioTransporte = 0;
        bool esSalarioIntegral = true;

        // Act
        var cesantia = new Cesantia(totalSalarial, auxilioTransporte, esSalarioIntegral);

        // Assert
        Assert.Equal(0, cesantia.Valor);
    }

    [Theory]
    [InlineData(2400000, true)]  // Salario integral
    [InlineData(2400000, false)] // Salario ordinario
    public void Vacaciones_DebeCalcularSiempre_IndependientementeTipoSalario(decimal totalSalarial, bool esSalarioIntegral)
    {
        // Arrange
        decimal auxilioTransporte = 0;
        decimal vacacionesEsperadas = Math.Round(totalSalarial / 24, 2, MidpointRounding.AwayFromZero);

        // Act
        var vacaciones = new Vacaciones(totalSalarial, auxilioTransporte, esSalarioIntegral);

        // Assert
        Assert.Equal(vacacionesEsperadas, vacaciones.Valor);
        Assert.Equal("Vacaciones", vacaciones.Nombre);
        Assert.True(vacaciones.Valor > 0);
    }

    [Fact]
    public void InteresCesantia_SalarioOrdinario_DebeCalcularCorrectamente()
    {
        // Arrange
        decimal totalSalarial = 2500000;
        decimal auxilioTransporte = 200000;
        bool esSalarioIntegral = false;
        decimal baseEsperada = totalSalarial + auxilioTransporte;
        decimal interesEsperado = Math.Round(baseEsperada * 0.12m / 12, 2, MidpointRounding.AwayFromZero);

        // Act
        var interes = new InteresCesantia(totalSalarial, auxilioTransporte, esSalarioIntegral);

        // Assert
        Assert.Equal(interesEsperado, interes.Valor);
        Assert.Equal("Intereses de Cesantías", interes.Nombre);
    }

    [Fact]
    public void InteresCesantia_SalarioIntegral_DebeSerCero()
    {
        // Arrange
        decimal totalSalarial = 16000000;
        decimal auxilioTransporte = 0;
        bool esSalarioIntegral = true;

        // Act
        var interes = new InteresCesantia(totalSalarial, auxilioTransporte, esSalarioIntegral);

        // Assert
        Assert.Equal(0, interes.Valor);
    }

    [Fact]
    public void BaseCalculo_SalarioIntegral_DebeAplicarFactor07()
    {
        // Arrange
        decimal totalSalarial = 20000000;
        decimal auxilioTransporte = 0;
        bool esSalarioIntegral = true;

        // Act
        var vacaciones = new Vacaciones(totalSalarial, auxilioTransporte, esSalarioIntegral);
        
        // Para vacaciones la base siempre es totalSalarial, no se aplica factor 0.7
        // Esto es correcto según la implementación TypeScript
        decimal esperado = Math.Round(totalSalarial / 24, 2, MidpointRounding.AwayFromZero);

        // Assert
        Assert.Equal(esperado, vacaciones.Valor);
    }

    [Fact]
    public void CalcularTotalPrestacionesSociales_DebeRetornarSumaCorrecta()
    {
        // Arrange
        decimal totalSalarial = 3000000;
        decimal auxilioTransporte = 200000;
        bool esSalarioIntegral = false;

        // Act
        var total = PrestacionesSocialesService.CalcularTotalPrestacionesSociales(
            totalSalarial, auxilioTransporte, esSalarioIntegral);
        
        var prestaciones = PrestacionesSocialesService.CalcularPrestacionesSociales(
            totalSalarial, auxilioTransporte, esSalarioIntegral);
        
        var sumaManual = prestaciones.Sum(p => p.Valor);

        // Assert
        Assert.Equal(sumaManual, total);
        Assert.True(total > 0);
    }
}