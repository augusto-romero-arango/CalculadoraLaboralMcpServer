using CalculadoraLaboral.McpServer.Domain.Constants;
using CalculadoraLaboral.McpServer.Domain.Models;

namespace CalculadoraLaboral.McpServer.Domain.Services;

public class SalarioService
{
    private const int CantidadSmlvIntegral = 13;
    private const string ErrorSalarioMenorMinimo = "El salario debe ser mayor al salario mínimo.";
    private const string ErrorSalarioIntegralInvalido = "El valor del salario debe ser mayor o igual a 13 SMLV.";

    public TipoSalario TipoSalario { get; private set; }
    public decimal SalarioBasico { get; private set; }
    public DateTime Fecha { get; private set; }

    public SalarioService(decimal salarioBasico, TipoSalario tipoSalario, DateTime fecha)
    {
        TipoSalario = tipoSalario;
        SalarioBasico = salarioBasico;
        Fecha = fecha;
        ValidarSalarioMinimo();
    }

    public void ModificarTipoSalario(TipoSalario tipo)
    {
        TipoSalario = tipo;
        ValidarSalarioMinimo();
    }

    public void ModificarValorSalario(decimal valor)
    {
        SalarioBasico = valor;
        ValidarSalarioMinimo();
    }

    private void ValidarSalarioMinimo()
    {
        if (TipoSalario == TipoSalario.Ordinario)
            ValidarSalarioMinimoOrdinario();
        else
            ValidarSalarioIntegral();
    }

    private void ValidarSalarioMinimoOrdinario()
    {
        var salarioMinimo = ParametrosAnuales.ObtenerSMLV(Fecha);
        if (SalarioBasico < salarioMinimo)
            throw new InvalidOperationException(ErrorSalarioMenorMinimo);
    }

    private void ValidarSalarioIntegral()
    {
        var salarioMinimo = ParametrosAnuales.ObtenerSMLV(Fecha);
        var salarioIntegralMinimo = salarioMinimo * CantidadSmlvIntegral;
        if (SalarioBasico < salarioIntegralMinimo)
            throw new InvalidOperationException(ErrorSalarioIntegralInvalido);
    }
}