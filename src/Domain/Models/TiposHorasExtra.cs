namespace CalculadoraLaboral.McpServer.Domain.Models;

public enum TiposHorasExtra
{
    DiurnaOrdinaria,
    DiurnaFestiva,
    NocturnaOrdinaria,
    NocturnaFestiva,
    RecargoNocturno,
    RecargoFestivo
}

public static class FactorHorasExtra
{
    public static readonly Dictionary<TiposHorasExtra, decimal> Factores = new()
    {
        { TiposHorasExtra.DiurnaOrdinaria, 1.25m },
        { TiposHorasExtra.DiurnaFestiva, 1.75m },
        { TiposHorasExtra.NocturnaOrdinaria, 1.75m },
        { TiposHorasExtra.NocturnaFestiva, 2.00m },
        { TiposHorasExtra.RecargoNocturno, 1.35m },
        { TiposHorasExtra.RecargoFestivo, 1.75m }
    };
}