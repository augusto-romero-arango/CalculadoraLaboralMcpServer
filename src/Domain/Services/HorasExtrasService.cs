using CalculadoraLaboral.McpServer.Domain.Models;

namespace CalculadoraLaboral.McpServer.Domain.Services;

public class HorasExtrasService
{
    private const string ErrorValorHoraMenorUno = "El valor de la hora no puede ser menor a uno";
    private const string ErrorCantidadNegativa = "La cantidad de la hora no puede ser negativo";
    private const string ErrorTipoHoraInvalido = "El tipo de hora no es válido";
    private const string ErrorCantidadHorasInvalida = "La cantidad de horas no es válida";

    private decimal _valorHoraOrdinaria;
    private readonly Dictionary<TiposHorasExtra, int> _items;

    public decimal ValorTotal => _items.Sum(item => CalcularHoraExtra(item.Key));

    public HorasExtrasService(decimal valorHoraOrdinaria, Dictionary<TiposHorasExtra, int>? items = null)
    {
        _items = items ?? new Dictionary<TiposHorasExtra, int>();
        ValidarDatos(valorHoraOrdinaria, _items);
        _valorHoraOrdinaria = valorHoraOrdinaria;
    }

    public void ModificarValorHoraOrdinaria(decimal nuevoValor)
    {
        if (nuevoValor <= 0)
            throw new ArgumentException(ErrorValorHoraMenorUno);
        _valorHoraOrdinaria = nuevoValor;
    }

    public int ObtenerCantidadHorasPorItem(TiposHorasExtra tipo)
    {
        return _items.TryGetValue(tipo, out var cantidad) ? cantidad : 0;
    }

    public decimal ObtenerValorHoraPorItem(TiposHorasExtra tipo)
    {
        return CalcularHoraExtra(tipo);
    }

    public void RegistrarHoraExtra(TiposHorasExtra tipo, int cantidad)
    {
        ValidarValorYTipo(cantidad, tipo);
        _items[tipo] = cantidad;
    }

    private void ValidarDatos(decimal valorHoraOrdinaria, Dictionary<TiposHorasExtra, int> items)
    {
        if (valorHoraOrdinaria <= 0)
            throw new ArgumentException(ErrorValorHoraMenorUno);

        foreach (var (tipo, cantidad) in items)
        {
            ValidarValorYTipo(cantidad, tipo);
        }
    }

    private void ValidarValorYTipo(int cantidad, TiposHorasExtra tipo)
    {
        if (cantidad < 0)
            throw new ArgumentException(ErrorCantidadNegativa);

        if (!Enum.IsDefined(typeof(TiposHorasExtra), tipo))
            throw new ArgumentException(ErrorTipoHoraInvalido);
    }

    private decimal CalcularHoraExtra(TiposHorasExtra tipo)
    {
        var cantidad = ObtenerCantidadHorasPorItem(tipo);
        var factor = FactorHorasExtra.Factores[tipo];
        var valorTotal = cantidad * factor * _valorHoraOrdinaria;
        return Math.Round(valorTotal, 0, MidpointRounding.AwayFromZero);
    }
}