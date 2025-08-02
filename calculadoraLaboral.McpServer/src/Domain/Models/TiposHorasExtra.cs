namespace CalculadoraLaboral.McpServer.Domain.Models;

public enum TiposHorasExtra
{
    DiurnaOrdinaria,
    DiurnaFestiva,
    NocturnaOrdinaria,
    NocturnaFestiva,
    RecargoNocturno,
    RecargoFestivo,
    HED,  // Hora extra diurna (equivalent to DiurnaOrdinaria)
    HEN,  // Hora extra nocturna (equivalent to NocturnaOrdinaria)
    HEFD, // Hora extra festiva diurna
    HEFN, // Hora extra festiva nocturna
    RN,   // Recargo nocturno (equivalent to RecargoNocturno)
    RDD,  // Recargo dominical diurno ocasional compensado
    RDN,  // Recargo dominical nocturno ocasional compensado
    RDDHC, // Recargo dominical diurno habitual compensado
    RDNHC, // Recargo dominical nocturno habitual compensado
    RDDONC, // Recargo dominical diurno ocasional no compensado
    RDNONC  // Recargo dominical nocturno ocasional no compensado
}

public static class FactorHorasExtra
{
    public static readonly Dictionary<TiposHorasExtra, decimal> Factores = new()
    {
        { TiposHorasExtra.DiurnaOrdinaria, 1.25m },
        { TiposHorasExtra.DiurnaFestiva, 1.75m },
        { TiposHorasExtra.NocturnaOrdinaria, 1.75m },
        { TiposHorasExtra.NocturnaFestiva, 2.00m },
        { TiposHorasExtra.RecargoNocturno, 0.35m },  // Corregido: era 1.35m
        { TiposHorasExtra.RecargoFestivo, 1.75m },
        
        // Nuevos tipos según implementación TypeScript
        { TiposHorasExtra.HED, 1.25m },   // Hora extra diurna
        { TiposHorasExtra.HEN, 1.75m },   // Hora extra nocturna
        { TiposHorasExtra.HEFD, 2.05m },  // Hora extra festiva diurna
        { TiposHorasExtra.HEFN, 2.55m },  // Hora extra festiva nocturna
        { TiposHorasExtra.RN, 0.35m },    // Recargo nocturno
        { TiposHorasExtra.RDD, 0.80m },   // Recargo dominical diurno ocasional compensado
        { TiposHorasExtra.RDN, 1.15m },   // Recargo dominical nocturno ocasional compensado
        { TiposHorasExtra.RDDHC, 1.8m },  // Recargo dominical diurno habitual compensado
        { TiposHorasExtra.RDNHC, 2.15m }, // Recargo dominical nocturno habitual compensado
        { TiposHorasExtra.RDDONC, 1.8m }, // Recargo dominical diurno ocasional no compensado
        { TiposHorasExtra.RDNONC, 2.15m } // Recargo dominical nocturno ocasional no compensado
    };
}