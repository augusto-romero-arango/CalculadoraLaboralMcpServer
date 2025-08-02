using CalculadoraLaboral.McpServer.Domain.Constants;
using CalculadoraLaboral.McpServer.Domain.Models;

namespace CalculadoraLaboral.McpServer.Domain.Services;

public class RemuneracionService
{
    private const decimal FactorPrestacionalSalarioOrdinario = 1m;
    private const decimal FactorPrestacionalSalarioIntegral = 0.7m;

    private readonly DateTime _fecha;
    private decimal _salarioBasico;
    private TipoSalario _tipoSalario;
    private decimal _valoresAdicionalesSalariales;
    private decimal _valorHorasExtras;
    private decimal _valoresAdicionalesNoSalariales;

    public bool EsSalarioIntegral => _tipoSalario == TipoSalario.Integral;
    public decimal FactorPrestacional => EsSalarioIntegral ? FactorPrestacionalSalarioIntegral : FactorPrestacionalSalarioOrdinario;
    public decimal ValorSalarioBasico => _salarioBasico;

    public decimal TotalSalarial => _salarioBasico + _valoresAdicionalesSalariales + _valorHorasExtras;
    public decimal TotalDevengado => TotalSalarial + _valoresAdicionalesNoSalariales;
    public decimal TotalPrestacional => TotalSalarial * FactorPrestacional;
    public decimal TotalBaseAuxilioTransporte => _salarioBasico + _valoresAdicionalesSalariales;
    public decimal ValorHoraOrdinaria => _salarioBasico / ParametrosAnuales.ObtenerCantidadHorasJornada(_fecha);
    public decimal PagosSalariales => _valoresAdicionalesSalariales;
    public decimal PagosNoSalariales => _valoresAdicionalesNoSalariales;

    public RemuneracionService(
        decimal salarioBasico,
        TipoSalario tipoSalario,
        DateTime fecha,
        decimal valoresAdicionalesSalariales = 0,
        decimal valorHorasExtras = 0,
        decimal valoresAdicionalesNoSalariales = 0)
    {
        _fecha = fecha;
        _salarioBasico = salarioBasico;
        _tipoSalario = tipoSalario;
        _valoresAdicionalesSalariales = valoresAdicionalesSalariales;
        _valorHorasExtras = valorHorasExtras;
        _valoresAdicionalesNoSalariales = valoresAdicionalesNoSalariales;
    }

    public void ModificarValorSalarial(decimal valorSalarial)
    {
        _valoresAdicionalesSalariales = valorSalarial;
    }

    public void ModificarSalario(decimal salarioBasico, TipoSalario tipoSalario)
    {
        _salarioBasico = salarioBasico;
        _tipoSalario = tipoSalario;
    }

    public void ModificarValorNoSalarial(decimal valorNoSalarial)
    {
        _valoresAdicionalesNoSalariales = valorNoSalarial;
    }

    public void ModificarValorHorasExtras(decimal valorHoraExtra)
    {
        _valorHorasExtras = valorHoraExtra;
    }
}