using CalculadoraLaboral.McpServer.Domain.Constants;
using CalculadoraLaboral.McpServer.Domain.Models;

namespace CalculadoraLaboral.McpServer.Domain.Services;

public class LiquidacionNominaService
{
    private readonly SalarioService _salarioService;
    private RemuneracionService _remuneracionService = null!;
    private AuxilioTransporteService _auxilioTransporteService = null!;
    private HorasExtrasService _horasExtrasService = null!;
    private decimal _factorRiesgoLaboral;
    private ClasesDeRiesgo _clasesDeRiesgo;

    public decimal SalarioBasico => _salarioService.SalarioBasico;
    public TipoSalario TipoSalario => _salarioService.TipoSalario;
    public decimal PagosNoSalariales => _remuneracionService.PagosNoSalariales;
    public decimal PagosSalariales => _remuneracionService.PagosSalariales;
    public ClasesDeRiesgo ClaseRiesgoLaboral => _clasesDeRiesgo;
    public DateTime Fecha => _salarioService.Fecha;
    public bool EsSalarioIntegral => _salarioService.TipoSalario == TipoSalario.Integral;
    public bool AplicaAuxilioTransporte => _auxilioTransporteService.AplicaAuxilioTransporte;
    public decimal ValorAuxilioTransporte => _auxilioTransporteService.Valor;
    public bool ViveCercaAlLugarDeTrabajo => _auxilioTransporteService.ViveCercaAlLugarDeTrabajo;
    public decimal ValorTotalHorasExtras => _horasExtrasService.ValorTotal;

    public decimal TotalizarGastos =>
        _salarioService.SalarioBasico +
        _auxilioTransporteService.Valor +
        _remuneracionService.PagosSalariales +
        _remuneracionService.PagosNoSalariales +
        _horasExtrasService.ValorTotal;

    public GastoNomina DetalleGastos => new()
    {
        SalarioBasico = _salarioService.SalarioBasico,
        AuxilioTransporte = _auxilioTransporteService.Valor,
        PagosSalariales = _remuneracionService.PagosSalariales,
        PagosNoSalariales = _remuneracionService.PagosNoSalariales,
        HorasExtrasYRecargos = _horasExtrasService.ValorTotal
    };

    public LiquidacionNominaService(decimal salarioBasico, TipoSalario tipoSalario, DateTime fecha)
    {
        _salarioService = new SalarioService(salarioBasico, tipoSalario, fecha);
        Inicializar();
    }

    private void Inicializar()
    {
        ModificarRiesgoLaboral(ClasesDeRiesgo.I);

        _remuneracionService = new RemuneracionService(
            _salarioService.SalarioBasico,
            _salarioService.TipoSalario,
            _salarioService.Fecha);

        _auxilioTransporteService = new AuxilioTransporteService(
            _remuneracionService.TotalBaseAuxilioTransporte,
            Fecha);

        _horasExtrasService = new HorasExtrasService(
            _remuneracionService.ValorHoraOrdinaria);
    }

    public ResumenLiquidacion Liquidar()
    {
        var totalPrestacionesSociales = LiquidarPrestacionesSociales();
        var totalSeguridadSocial = LiquidarSeguridadSocial();

        var prestacionesDetalles = PrestacionesSocialesService.CalcularPrestacionesSociales(
            _remuneracionService.TotalSalarial,
            _auxilioTransporteService.Valor,
            EsSalarioIntegral)
            .Select(p => new ProvisionDetalle
            {
                Nombre = p.Nombre,
                Valor = p.Valor,
                Descripcion = p.Descripcion
            }).ToList();

        var seguridadSocialDetalles = SeguridadSocialService.CalcularTotalSeguridadSocial(
            _remuneracionService.TotalSalarial,
            _remuneracionService.TotalDevengado,
            _remuneracionService.TotalPrestacional,
            ParametrosAnuales.ObtenerSMLV(Fecha),
            _factorRiesgoLaboral);

        return new ResumenLiquidacion
        {
            Gastos = DetalleGastos,
            TotalGastos = TotalizarGastos,
            ProvisionEmpleador = new ProvisionesEmpleador
            {
                PrestacionesSociales = prestacionesDetalles,
                SeguridadSocial = seguridadSocialDetalles
            },
            TotalProvisionEmpleador = totalSeguridadSocial + totalPrestacionesSociales,
            TotalLiquidacion = totalPrestacionesSociales + totalSeguridadSocial + _remuneracionService.TotalDevengado
        };
    }

    public void RegistrarHorasExtras(TiposHorasExtra tipo, int cantidad)
    {
        _horasExtrasService.RegistrarHoraExtra(tipo, cantidad);
        _remuneracionService.ModificarValorHorasExtras(_horasExtrasService.ValorTotal);
    }

    private decimal LiquidarPrestacionesSociales()
    {
        return PrestacionesSocialesService.CalcularTotalPrestacionesSociales(
            _remuneracionService.TotalSalarial,
            _auxilioTransporteService.Valor,
            EsSalarioIntegral);
    }

    private decimal LiquidarSeguridadSocial()
    {
        var salarioMinimo = ParametrosAnuales.ObtenerSMLV(Fecha);
        return SeguridadSocialService.CalcularTotalValorSeguridadSocial(
            _remuneracionService.TotalSalarial,
            _remuneracionService.TotalDevengado,
            _remuneracionService.TotalPrestacional,
            salarioMinimo,
            _factorRiesgoLaboral);
    }

    public void ModificarRiesgoLaboral(ClasesDeRiesgo riesgo)
    {
        _clasesDeRiesgo = riesgo;
        _factorRiesgoLaboral = FactorRiesgoLaboral.Factores[riesgo];
    }

    public void ModificarTipoSalario(TipoSalario tipoSalario)
    {
        _salarioService.ModificarTipoSalario(tipoSalario);
        _remuneracionService.ModificarSalario(_salarioService.SalarioBasico, tipoSalario);
    }

    public void ModificarValorSalario(decimal valor)
    {
        _auxilioTransporteService.ModificarBaseParaAuxilioTransporte(valor);
        _salarioService.ModificarValorSalario(valor);
        _remuneracionService.ModificarSalario(valor, _salarioService.TipoSalario);
        _horasExtrasService.ModificarValorHoraOrdinaria(_remuneracionService.ValorHoraOrdinaria);
    }

    public void ModificarValorNoSalarial(decimal valor)
    {
        _remuneracionService.ModificarValorNoSalarial(valor);
    }

    public void ModificarValorSalarial(decimal valor)
    {
        _remuneracionService.ModificarValorSalarial(valor);
    }

    public void ModificarViveCercaAlLugarDeTrabajo(bool viveCerca)
    {
        _auxilioTransporteService.ModificarViveCercaAlLugarDeTrabajo(viveCerca);
    }

    public decimal ObtenerValorHoraPorItem(TiposHorasExtra tipo)
    {
        return _horasExtrasService.ObtenerValorHoraPorItem(tipo);
    }

    public int ObtenerCantidadHorasPorItem(TiposHorasExtra tipo)
    {
        return _horasExtrasService.ObtenerCantidadHorasPorItem(tipo);
    }
}