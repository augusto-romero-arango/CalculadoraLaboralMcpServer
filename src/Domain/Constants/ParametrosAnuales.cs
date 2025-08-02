namespace CalculadoraLaboral.McpServer.Domain.Constants;

public static class ParametrosAnuales
{
    private static readonly Dictionary<int, decimal> SalarioMinimoLegalVigente = new()
    {
        { 2022, 1_000_000m },
        { 2023, 1_160_000m },
        { 2024, 1_300_000m },
        { 2025, 1_423_500m },
        { 2026, 1_423_500m }
    };

    private static readonly Dictionary<int, decimal> AuxilioTransporte = new()
    {
        { 2022, 117_172m },
        { 2023, 140_606m },
        { 2024, 162_000m },
        { 2025, 200_000m },
        { 2026, 200_000m }
    };

    public static decimal ObtenerSMLV(DateTime fecha)
    {
        var año = fecha.Year;
        return SalarioMinimoLegalVigente.TryGetValue(año, out var smlv) 
            ? smlv 
            : throw new ArgumentException($"No se encontró SMLV para el año {año}");
    }

    public static decimal ObtenerAuxilioTransporte(DateTime fecha)
    {
        var año = fecha.Year;
        return AuxilioTransporte.TryGetValue(año, out var auxilio)
            ? auxilio
            : throw new ArgumentException($"No se encontró auxilio de transporte para el año {año}");
    }

    public static int ObtenerCantidadHorasJornada(DateTime fecha)
    {
        if (fecha < new DateTime(2023, 7, 15)) return 240;
        if (fecha < new DateTime(2024, 7, 15)) return 235;
        if (fecha < new DateTime(2025, 7, 15)) return 230;
        if (fecha < new DateTime(2026, 7, 15)) return 220;
        
        return 210;
    }
}