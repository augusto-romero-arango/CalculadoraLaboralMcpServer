using CalculadoraLaboral.McpServer.Domain.Constants;
using CalculadoraLaboral.McpServer.Domain.Models;

namespace CalculadoraLaboral.McpServer.Domain.Services;

public abstract class SeguridadSocial
{
    protected decimal _totalSalarial;
    protected decimal _totalDevengado;
    protected decimal _totalPrestacional;
    protected decimal _salarioMinimo;
    protected decimal _tarifaNOExonerada;
    protected decimal _tarifaExonerada;

    public abstract string Nombre { get; }
    public abstract string Descripcion { get; }
    public abstract decimal Valor { get; }

    protected SeguridadSocial(decimal totalSalarial, decimal totalDevengado, decimal totalPrestacional, decimal salarioMinimo, decimal tarifaNOExonerada, decimal tarifaExonerada)
    {
        _totalSalarial = totalSalarial;
        _totalDevengado = totalDevengado;
        _totalPrestacional = totalPrestacional;
        _salarioMinimo = salarioMinimo;
        _tarifaNOExonerada = tarifaNOExonerada;
        _tarifaExonerada = tarifaExonerada;
    }

    protected decimal TopeMaximoSMLV => _salarioMinimo * 25;
    
    protected decimal TopeExoneracion => _salarioMinimo * 10;

    protected decimal Porcentaje 
    {
        get
        {
            if (_totalDevengado < TopeExoneracion) return _tarifaExonerada;
            return _tarifaNOExonerada;
        }
    }

    protected decimal BaseCalculo 
    {
        get
        {
            // Implementación Ley 1393
            var base1393 = _totalDevengado * 0.6m;
            var ajuste1393 = base1393 > _totalSalarial ? base1393 - _totalSalarial : 0;
            var baseParaCalculo = ajuste1393 + _totalPrestacional;
            
            // Aplicar tope de 25 SMLV
            if (baseParaCalculo > TopeMaximoSMLV)
                baseParaCalculo = TopeMaximoSMLV;
            
            return baseParaCalculo;
        }
    }
}

public class SeguridadSocialSalud : SeguridadSocial
{
    public override string Nombre => "Salud";
    public override string Descripcion => "Aporte a salud por el empleador";
    public override decimal Valor => Math.Round(BaseCalculo * Porcentaje, 0, MidpointRounding.AwayFromZero);

    public SeguridadSocialSalud(decimal totalSalarial, decimal totalDevengado, decimal totalPrestacional, decimal salarioMinimo)
        : base(totalSalarial, totalDevengado, totalPrestacional, salarioMinimo, TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.Salud], 0) { }
}

public class SeguridadSocialPension : SeguridadSocial
{
    public override string Nombre => "Pensión";
    public override string Descripcion => "Aporte a pensión por el empleador";
    public override decimal Valor => Math.Round(BaseCalculo * Porcentaje, 0, MidpointRounding.AwayFromZero);

    public SeguridadSocialPension(decimal totalSalarial, decimal totalDevengado, decimal totalPrestacional, decimal salarioMinimo)
        : base(totalSalarial, totalDevengado, totalPrestacional, salarioMinimo, TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.Pension], TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.Pension]) { }
}

public class SeguridadSocialArl : SeguridadSocial
{
    private readonly decimal _factorRiesgoLaboral;

    public override string Nombre => "ARL";
    public override string Descripcion => "Administradora de Riesgos Laborales";
    public override decimal Valor => Math.Round(BaseCalculo * _factorRiesgoLaboral, 0, MidpointRounding.AwayFromZero);

    public SeguridadSocialArl(decimal totalSalarial, decimal totalDevengado, decimal totalPrestacional, decimal salarioMinimo, decimal factorRiesgoLaboral)
        : base(totalSalarial, totalDevengado, totalPrestacional, salarioMinimo, factorRiesgoLaboral, factorRiesgoLaboral)
    {
        _factorRiesgoLaboral = factorRiesgoLaboral;
    }
}

public abstract class Parafiscales
{
    protected decimal _totalPrestacional;
    protected decimal _totalDevengado;
    protected decimal _salarioMinimo;

    public abstract string Nombre { get; }
    public abstract string Descripcion { get; }
    public abstract decimal Valor { get; }

    protected decimal _tarifaNOExonerada;
    protected decimal _tarifaExonerada;

    protected Parafiscales(decimal totalPrestacional, decimal totalDevengado, decimal salarioMinimo, decimal tarifaNOExonerada, decimal tarifaExonerada)
    {
        _totalPrestacional = totalPrestacional;
        _totalDevengado = totalDevengado;
        _salarioMinimo = salarioMinimo;
        _tarifaNOExonerada = tarifaNOExonerada;
        _tarifaExonerada = tarifaExonerada;
    }

    protected decimal TopeExoneracion => _salarioMinimo * 10;

    protected decimal Porcentaje
    {
        get
        {
            if (_totalDevengado < TopeExoneracion) return _tarifaExonerada;
            return _tarifaNOExonerada;
        }
    }

    protected decimal BaseCalculo => _totalPrestacional;
}

public class ParafiscalesCajaCompensacion : Parafiscales
{
    public override string Nombre => "Caja de Compensación";
    public override string Descripcion => "Aporte a caja de compensación familiar";
    public override decimal Valor => Math.Round(BaseCalculo * Porcentaje, 0, MidpointRounding.AwayFromZero);

    public ParafiscalesCajaCompensacion(decimal totalPrestacional, decimal totalDevengado, decimal salarioMinimo)
        : base(totalPrestacional, totalDevengado, salarioMinimo, TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.CCF], TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.CCF]) { }
}

public class ParafiscalesIcbf : Parafiscales
{
    public override string Nombre => "ICBF";
    public override string Descripcion => "Instituto Colombiano de Bienestar Familiar";
    public override decimal Valor => Math.Round(BaseCalculo * Porcentaje, 0, MidpointRounding.AwayFromZero);

    public ParafiscalesIcbf(decimal totalPrestacional, decimal totalDevengado, decimal salarioMinimo)
        : base(totalPrestacional, totalDevengado, salarioMinimo, TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.ICBF], 0) { }
}

public class ParafiscalesSena : Parafiscales
{
    public override string Nombre => "SENA";
    public override string Descripcion => "Servicio Nacional de Aprendizaje";
    public override decimal Valor => Math.Round(BaseCalculo * Porcentaje, 0, MidpointRounding.AwayFromZero);

    public ParafiscalesSena(decimal totalPrestacional, decimal totalDevengado, decimal salarioMinimo)
        : base(totalPrestacional, totalDevengado, salarioMinimo, TarifasSeguridadSocial.Tarifas[TiposTarifasSeguridadSocial.SENA], 0) { }
}

public class SeguridadSocialService
{
    public static List<SeguridadSocial> CalcularSeguridadSocial(
        decimal totalSalarial,
        decimal totalDevengado,
        decimal totalPrestacional,
        decimal salarioMinimo,
        decimal factorRiesgoLaboral)
    {
        return new List<SeguridadSocial>
        {
            new SeguridadSocialSalud(totalSalarial, totalDevengado, totalPrestacional, salarioMinimo),
            new SeguridadSocialPension(totalSalarial, totalDevengado, totalPrestacional, salarioMinimo),
            new SeguridadSocialArl(totalSalarial, totalDevengado, totalPrestacional, salarioMinimo, factorRiesgoLaboral)
        };
    }

    public static List<Parafiscales> CalcularParafiscales(
        decimal totalPrestacional,
        decimal totalDevengado,
        decimal salarioMinimo)
    {
        return new List<Parafiscales>
        {
            new ParafiscalesCajaCompensacion(totalPrestacional, totalDevengado, salarioMinimo),
            new ParafiscalesIcbf(totalPrestacional, totalDevengado, salarioMinimo),
            new ParafiscalesSena(totalPrestacional, totalDevengado, salarioMinimo)
        };
    }

    public static List<ProvisionDetalle> CalcularTotalSeguridadSocial(
        decimal totalSalarial,
        decimal totalDevengado,
        decimal totalPrestacional,
        decimal salarioMinimo,
        decimal factorRiesgoLaboral)
    {
        var seguridadSocial = CalcularSeguridadSocial(totalSalarial, totalDevengado, totalPrestacional, salarioMinimo, factorRiesgoLaboral);
        var parafiscales = CalcularParafiscales(totalPrestacional, totalDevengado, salarioMinimo);

        var result = new List<ProvisionDetalle>();
        
        result.AddRange(seguridadSocial.Select(s => new ProvisionDetalle
        {
            Nombre = s.Nombre,
            Valor = s.Valor,
            Descripcion = s.Descripcion
        }));

        result.AddRange(parafiscales.Select(p => new ProvisionDetalle
        {
            Nombre = p.Nombre,
            Valor = p.Valor,
            Descripcion = p.Descripcion
        }));

        return result;
    }

    public static decimal CalcularTotalValorSeguridadSocial(
        decimal totalSalarial,
        decimal totalDevengado,
        decimal totalPrestacional,
        decimal salarioMinimo,
        decimal factorRiesgoLaboral)
    {
        var detalles = CalcularTotalSeguridadSocial(totalSalarial, totalDevengado, totalPrestacional, salarioMinimo, factorRiesgoLaboral);
        return detalles.Sum(d => d.Valor);
    }
}