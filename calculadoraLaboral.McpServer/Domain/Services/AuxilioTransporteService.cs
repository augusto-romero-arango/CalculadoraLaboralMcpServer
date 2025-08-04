using CalculadoraLaboral.McpServer.Domain.Constants;

namespace CalculadoraLaboral.McpServer.Domain.Services;

public class AuxilioTransporteService
{
    private const string ErrorBaseNegativa = "El valor de la base no puede ser negativo";

    private decimal _baseParaAuxilioTransporte;
    private bool _viveCercaAlLugarDeTrabajo;
    private readonly DateTime _fecha;

    private decimal TopeAuxilioTransporte => ParametrosAnuales.ObtenerSMLV(_fecha) * 2;

    public decimal Valor => ObtenerAuxilioTransporte();
    public bool ViveCercaAlLugarDeTrabajo => _viveCercaAlLugarDeTrabajo;
    public bool AplicaAuxilioTransporte => _baseParaAuxilioTransporte < TopeAuxilioTransporte;

    public AuxilioTransporteService(decimal baseParaAuxilioTransporte, DateTime fecha, bool viveCercaAlLugarDeTrabajo = false)
    {
        if (baseParaAuxilioTransporte < 0)
            throw new ArgumentException(ErrorBaseNegativa);

        _baseParaAuxilioTransporte = baseParaAuxilioTransporte;
        _fecha = fecha;
        _viveCercaAlLugarDeTrabajo = viveCercaAlLugarDeTrabajo;
    }

    public decimal ObtenerAuxilioTransporte()
    {
        if (!AplicaAuxilioTransporte || _viveCercaAlLugarDeTrabajo)
            return 0;
            
        return ParametrosAnuales.ObtenerAuxilioTransporte(_fecha);
    }

    public void ModificarBaseParaAuxilioTransporte(decimal nuevoValor)
    {
        _baseParaAuxilioTransporte = nuevoValor;
    }

    public void ModificarViveCercaAlLugarDeTrabajo(bool viveCerca)
    {
        _viveCercaAlLugarDeTrabajo = viveCerca;
    }
}