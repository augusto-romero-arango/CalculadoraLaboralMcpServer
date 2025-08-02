namespace CalculadoraLaboral.McpServer.Domain.Constants;

public enum TiposTarifasSeguridadSocial
{
    Salud,
    Pension,
    CCF,
    ICBF,
    SENA
}

public static class TarifasSeguridadSocial
{
    public static readonly Dictionary<TiposTarifasSeguridadSocial, decimal> Tarifas = new()
    {
        { TiposTarifasSeguridadSocial.Salud, 0.085m },
        { TiposTarifasSeguridadSocial.Pension, 0.12m },
        { TiposTarifasSeguridadSocial.CCF, 0.04m },
        { TiposTarifasSeguridadSocial.ICBF, 0.03m },
        { TiposTarifasSeguridadSocial.SENA, 0.02m }
    };
}