using CalculadoraLaboral.McpServer.Domain.Models;

namespace CalculadoraLaboral.McpServer.Domain.Services;

public abstract class PrestacionSocial
{
    protected decimal _totalSalarial;
    protected decimal _auxilioTransporte;
    protected bool _esSalarioIntegral;

    public abstract string Nombre { get; }
    public abstract string Descripcion { get; }
    public abstract decimal Valor { get; }

    protected PrestacionSocial(decimal totalSalarial, decimal auxilioTransporte, bool esSalarioIntegral)
    {
        _totalSalarial = totalSalarial;
        _auxilioTransporte = auxilioTransporte;
        _esSalarioIntegral = esSalarioIntegral;
    }

    protected decimal BaseCalculo => _esSalarioIntegral ? _totalSalarial * 0.7m : _totalSalarial + _auxilioTransporte;
}

public class Prima : PrestacionSocial
{
    public override string Nombre => "Prima";
    public override string Descripcion => "Prima de servicios";
    public override decimal Valor => Math.Round(BaseCalculo / 12, 2, MidpointRounding.AwayFromZero);

    public Prima(decimal totalSalarial, decimal auxilioTransporte, bool esSalarioIntegral)
        : base(totalSalarial, auxilioTransporte, esSalarioIntegral) { }
}

public class Cesantia : PrestacionSocial
{
    public override string Nombre => "Cesantías";
    public override string Descripcion => "Cesantías";
    public override decimal Valor => Math.Round(BaseCalculo / 12, 2, MidpointRounding.AwayFromZero);

    public Cesantia(decimal totalSalarial, decimal auxilioTransporte, bool esSalarioIntegral)
        : base(totalSalarial, auxilioTransporte, esSalarioIntegral) { }
}

public class Vacaciones : PrestacionSocial
{
    public override string Nombre => "Vacaciones";
    public override string Descripcion => "Vacaciones";
    public override decimal Valor => Math.Round(_totalSalarial / 24, 2, MidpointRounding.AwayFromZero);

    public Vacaciones(decimal totalSalarial, decimal auxilioTransporte, bool esSalarioIntegral)
        : base(totalSalarial, auxilioTransporte, esSalarioIntegral) { }
}

public class InteresCesantia : PrestacionSocial
{
    public override string Nombre => "Intereses de Cesantías";
    public override string Descripcion => "Intereses sobre cesantías";
    public override decimal Valor => Math.Round(BaseCalculo * 0.12m / 12, 2, MidpointRounding.AwayFromZero);

    public InteresCesantia(decimal totalSalarial, decimal auxilioTransporte, bool esSalarioIntegral)
        : base(totalSalarial, auxilioTransporte, esSalarioIntegral) { }
}

public class PrestacionesSocialesService
{
    public static List<PrestacionSocial> CalcularPrestacionesSociales(
        decimal totalSalarial, 
        decimal auxilioTransporte, 
        bool esSalarioIntegral)
    {
        return new List<PrestacionSocial>
        {
            new Prima(totalSalarial, auxilioTransporte, esSalarioIntegral),
            new Cesantia(totalSalarial, auxilioTransporte, esSalarioIntegral),
            new Vacaciones(totalSalarial, auxilioTransporte, esSalarioIntegral),
            new InteresCesantia(totalSalarial, auxilioTransporte, esSalarioIntegral)
        };
    }

    public static decimal CalcularTotalPrestacionesSociales(
        decimal totalSalarial, 
        decimal auxilioTransporte, 
        bool esSalarioIntegral)
    {
        var prestaciones = CalcularPrestacionesSociales(totalSalarial, auxilioTransporte, esSalarioIntegral);
        return prestaciones.Sum(p => p.Valor);
    }
}