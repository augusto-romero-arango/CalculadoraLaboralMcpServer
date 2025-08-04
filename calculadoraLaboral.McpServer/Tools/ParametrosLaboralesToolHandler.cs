using System.Text.Json;
using CalculadoraLaboral.McpServer.Domain.Constants;

namespace CalculadoraLaboral.McpServer.Tools;

public class ParametrosLaboralesRequest
{
    public int? Anio { get; set; }
}

public class ParametrosLaboralesToolHandler : IToolHandler
{
    public string Name => "obtener_parametros_laborales";
    public string Description => "Obtiene el salario mínimo legal vigente y el auxilio de transporte para un año específico. Por defecto usa el año actual (2025).";

    public object Schema => new
    {
        type = "object",
        properties = new
        {
            anio = new
            {
                type = "number",
                description = "Año para consultar los parámetros laborales (opcional, por defecto año actual)",
                minimum = 2022,
                maximum = 2026
            }
        },
        required = new string[] { }
    };

    public async Task<object> HandleAsync(JsonElement arguments)
    {
        try
        {
            var request = ParseRequest(arguments);
            var resultado = await ObtenerParametrosLaboralesAsync(request);
            return JsonSerializer.Serialize(resultado, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                error = true,
                message = ex.Message,
                details = ex.ToString()
            }, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
    }

    private ParametrosLaboralesRequest ParseRequest(JsonElement arguments)
    {
        var request = new ParametrosLaboralesRequest();
        // Normaliza: acepta "anio", "año" y "ano" como equivalentes
        if (arguments.TryGetProperty("anio", out var anioElement))
        {
            request.Anio = anioElement.GetInt32();
        }
        else if (arguments.TryGetProperty("año", out var anioElement2))
        {
            request.Anio = anioElement2.GetInt32();
        }
        else if (arguments.TryGetProperty("ano", out var anioElement3))
        {
            request.Anio = anioElement3.GetInt32();
        }
        return request;
    }

    private async Task<object> ObtenerParametrosLaboralesAsync(ParametrosLaboralesRequest request)
    {
        await Task.CompletedTask;
        var anioAUsar = request.Anio ?? DateTime.Now.Year;
        var fecha = new DateTime(anioAUsar, 12, 31);
        try
        {
            var salarioMinimo = ParametrosAnuales.ObtenerSMLV(fecha);
            var auxilioTransporte = ParametrosAnuales.ObtenerAuxilioTransporte(fecha);
            var horasJornada = ParametrosAnuales.ObtenerCantidadHorasJornada(fecha);
            return new
            {
                success = true,
                anio = anioAUsar,
                data = new
                {
                    salarioMinimoLegalVigente = salarioMinimo,
                    auxilioTransporte = auxilioTransporte,
                    horasJornadaMensual = horasJornada,
                    salarioIntegralMinimo = salarioMinimo * 13, // 13 SMLV
                    valorHoraOrdinaria = salarioMinimo / horasJornada,
                    descripcion = new
                    {
                        salarioMinimo = "Salario Mínimo Legal Vigente (SMLV) en pesos colombianos",
                        auxilioTransporte = "Auxilio de transporte mensual en pesos colombianos",
                        horasJornada = "Cantidad de horas de la jornada laboral mensual",
                        salarioIntegral = "Salario integral mínimo (13 SMLV)",
                        valorHora = "Valor de la hora ordinaria de trabajo"
                    }
                }
            };
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException($"No se encontraron parámetros laborales para el año {anioAUsar}. Años disponibles: 2022-2026", ex);
        }
    }
}