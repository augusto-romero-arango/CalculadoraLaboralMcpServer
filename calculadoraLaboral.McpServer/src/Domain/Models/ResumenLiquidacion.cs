namespace CalculadoraLaboral.McpServer.Domain.Models;

public record GastoNomina
{
    public decimal SalarioBasico { get; init; }
    public decimal AuxilioTransporte { get; init; }
    public decimal PagosSalariales { get; init; }
    public decimal PagosNoSalariales { get; init; }
    public decimal HorasExtrasYRecargos { get; init; }
}

public record ProvisionDetalle
{
    public string Nombre { get; init; } = string.Empty;
    public decimal Valor { get; init; }
    public string Descripcion { get; init; } = string.Empty;
}

public record ProvisionesEmpleador
{
    public List<ProvisionDetalle> PrestacionesSociales { get; init; } = new();
    public List<ProvisionDetalle> SeguridadSocial { get; init; } = new();
}

public record ResumenLiquidacion
{
    public GastoNomina Gastos { get; init; } = new();
    public decimal TotalGastos { get; init; }
    public ProvisionesEmpleador ProvisionEmpleador { get; init; } = new();
    public decimal TotalProvisionEmpleador { get; init; }
    public decimal TotalLiquidacion { get; init; }
}