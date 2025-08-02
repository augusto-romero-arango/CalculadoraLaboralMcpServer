namespace CalculadoraLaboral.McpServer.Domain.Models;

public enum ClasesDeRiesgo
{
    I = 1,
    II = 2,
    III = 3,
    IV = 4,
    V = 5
}

public static class FactorRiesgoLaboral
{
    public static readonly Dictionary<ClasesDeRiesgo, decimal> Factores = new()
    {
        { ClasesDeRiesgo.I, 0.00522m },
        { ClasesDeRiesgo.II, 0.01044m },
        { ClasesDeRiesgo.III, 0.02436m },
        { ClasesDeRiesgo.IV, 0.04350m },
        { ClasesDeRiesgo.V, 0.06960m }
    };
}