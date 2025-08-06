using System.Text.Json;
using System.Text.RegularExpressions;
using CalculadoraLaboral.McpServer.Domain.Models;
using CalculadoraLaboral.McpServer.Domain.Services;

namespace CalculadoraLaboral.McpServer.Tools;

public class HorasExtrasCalculoRequest
{
    public decimal? SalarioMensual { get; set; }
    public decimal? ValorHoraOrdinaria { get; set; }
    public string? TipoHora { get; set; }
    public string? Cantidad { get; set; }
    public bool? MostrarTipos { get; set; }
    public Dictionary<string, string>? Horas { get; set; }
}

public class HorasExtrasToolHandler : IToolHandler
{
    public string Name => "calcular_horas_extras";
    public string Description => "Calcula el valor de las horas extras basado en el salario del empleado y las horas trabajadas. Puede mostrar los tipos disponibles y manejar múltiples tipos de horas.";

    public object Schema => new
    {
        type = "object",
        properties = new
        {
            salarioMensual = new
            {
                type = "number",
                description = "Salario mensual del empleado (opcional si se proporciona valorHoraOrdinaria)",
                minimum = 0
            },
            valorHoraOrdinaria = new
            {
                type = "number", 
                description = "Valor de la hora ordinaria de trabajo (opcional si se proporciona salarioMensual)",
                minimum = 0
            },
            tipoHora = new
            {
                type = "string",
                description = "Tipo de hora extra (ej: 'HED', 'HEN', 'HEFD', 'DiurnaOrdinaria', etc.)"
            },
            cantidad = new
            {
                type = "string",
                description = "Cantidad de horas. Acepta formatos: '2.5', '2h 30min', '2:30'"
            },
            horas = new
            {
                type = "object",
                description = "Diccionario con múltiples tipos de horas y sus cantidades",
                additionalProperties = new
                {
                    type = "string",
                    description = "Cantidad de horas en formato '2.5', '2h 30min', o '2:30'"
                }
            },
            mostrarTipos = new
            {
                type = "boolean",
                description = "Si es true, muestra todos los tipos de horas extras disponibles con sus factores"
            }
        },
        required = new string[] { }
    };

    public async Task<object> HandleAsync(JsonElement arguments)
    {
        try
        {
            var request = ParseRequest(arguments);
            var resultado = await CalcularHorasExtrasAsync(request);
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

    private HorasExtrasCalculoRequest ParseRequest(JsonElement arguments)
    {
        var request = new HorasExtrasCalculoRequest();
        
        if (arguments.TryGetProperty("salarioMensual", out var salarioElement))
            request.SalarioMensual = salarioElement.GetDecimal();

        if (arguments.TryGetProperty("valorHoraOrdinaria", out var valorHoraElement))
            request.ValorHoraOrdinaria = valorHoraElement.GetDecimal();

        if (arguments.TryGetProperty("tipoHora", out var tipoElement))
            request.TipoHora = tipoElement.GetString();

        if (arguments.TryGetProperty("cantidad", out var cantidadElement))
            request.Cantidad = cantidadElement.GetString();

        if (arguments.TryGetProperty("mostrarTipos", out var mostrarTiposElement))
            request.MostrarTipos = mostrarTiposElement.GetBoolean();

        if (arguments.TryGetProperty("horas", out var horasElement))
        {
            request.Horas = new Dictionary<string, string>();
            foreach (var propiedad in horasElement.EnumerateObject())
            {
                request.Horas[propiedad.Name] = propiedad.Value.GetString() ?? "";
            }
        }

        return request;
    }

    private async Task<object> CalcularHorasExtrasAsync(HorasExtrasCalculoRequest request)
    {
        await Task.CompletedTask;

        if (request.MostrarTipos == true)
        {
            return MostrarTiposDisponibles();
        }

        var valorHoraOrdinaria = ObtenerValorHoraOrdinaria(request);
        
        if (request.Horas != null && request.Horas.Any())
        {
            return CalcularMultiplesHoras(valorHoraOrdinaria, request.Horas);
        }

        if (string.IsNullOrEmpty(request.TipoHora) || string.IsNullOrEmpty(request.Cantidad))
        {
            throw new ArgumentException("Debe proporcionar tipoHora y cantidad, o usar el parámetro horas para múltiples tipos, o mostrarTipos=true para ver los tipos disponibles");
        }

        return CalcularHoraIndividual(valorHoraOrdinaria, request.TipoHora, request.Cantidad);
    }

    private decimal ObtenerValorHoraOrdinaria(HorasExtrasCalculoRequest request)
    {
        if (request.ValorHoraOrdinaria.HasValue && request.ValorHoraOrdinaria > 0)
        {
            return request.ValorHoraOrdinaria.Value;
        }

        if (request.SalarioMensual.HasValue && request.SalarioMensual > 0)
        {
            const decimal horasJornadaMensual = 192m; // 48 horas semanales * 4 semanas
            return request.SalarioMensual.Value / horasJornadaMensual;
        }

        throw new ArgumentException("Debe proporcionar salarioMensual o valorHoraOrdinaria");
    }

    private object MostrarTiposDisponibles()
    {
        var tipos = new List<object>();
        
        foreach (TiposHorasExtra tipo in Enum.GetValues<TiposHorasExtra>())
        {
            var factor = FactorHorasExtra.Factores[tipo];
            var descripcion = ObtenerDescripcionTipo(tipo);
            
            tipos.Add(new
            {
                codigo = tipo.ToString(),
                descripcion = descripcion,
                factor = factor,
                ejemplo = $"Para usar: tipoHora: '{tipo}'"
            });
        }

        return new
        {
            success = true,
            message = "Tipos de horas extras disponibles",
            data = new
            {
                tiposDisponibles = tipos,
                formatosCantidad = new[]
                {
                    "Decimales: '2.5' (2 horas y 30 minutos)",
                    "Horas y minutos: '2h 30min', '2 horas 30 minutos'",
                    "Formato tiempo: '2:30'"
                },
                ejemploUso = new
                {
                    simple = "tipoHora: 'HED', cantidad: '2.5'",
                    multiple = "horas: { 'HED': '2.5', 'HEN': '1h 30min' }"
                }
            }
        };
    }

    private object CalcularMultiplesHoras(decimal valorHoraOrdinaria, Dictionary<string, string> horas)
    {
        var horasCalculadas = new List<object>();
        decimal totalGeneral = 0;
        var horasService = new HorasExtrasService(valorHoraOrdinaria);

        foreach (var kvp in horas)
        {
            try
            {
                var tipoHora = ResolverTipoHora(kvp.Key);
                var minutos = ConvertirCantidadAMinutos(kvp.Value);
                var cantidadHoras = (int)Math.Round(minutos / 60.0m, 0);
                
                horasService.RegistrarHoraExtra(tipoHora, cantidadHoras);
                var valorHora = horasService.ObtenerValorHoraPorItem(tipoHora);
                
                horasCalculadas.Add(new
                {
                    tipo = tipoHora.ToString(),
                    descripcion = ObtenerDescripcionTipo(tipoHora),
                    cantidadOriginal = kvp.Value,
                    cantidadMinutos = minutos,
                    cantidadHorasDecimales = Math.Round(minutos / 60.0m, 2),
                    cantidadHorasEnteras = cantidadHoras,
                    factor = FactorHorasExtra.Factores[tipoHora],
                    valorTotal = valorHora
                });
                
                totalGeneral += valorHora;
            }
            catch (Exception ex)
            {
                horasCalculadas.Add(new
                {
                    tipo = kvp.Key,
                    error = ex.Message,
                    cantidadOriginal = kvp.Value
                });
            }
        }

        return new
        {
            success = true,
            data = new
            {
                valorHoraOrdinaria = valorHoraOrdinaria,
                horasCalculadas = horasCalculadas,
                totalGeneral = Math.Round(totalGeneral, 0),
                resumen = $"Total de horas extras: ${totalGeneral:N0} COP"
            }
        };
    }

    private object CalcularHoraIndividual(decimal valorHoraOrdinaria, string tipoHoraStr, string cantidadStr)
    {
        var tipoHora = ResolverTipoHora(tipoHoraStr);
        var minutos = ConvertirCantidadAMinutos(cantidadStr);
        var cantidadHoras = (int)Math.Round(minutos / 60.0m, 0);
        
        var horasService = new HorasExtrasService(valorHoraOrdinaria);
        horasService.RegistrarHoraExtra(tipoHora, cantidadHoras);
        
        var valorTotal = horasService.ObtenerValorHoraPorItem(tipoHora);
        var factor = FactorHorasExtra.Factores[tipoHora];

        return new
        {
            success = true,
            data = new
            {
                tipoHora = tipoHora.ToString(),
                descripcion = ObtenerDescripcionTipo(tipoHora),
                cantidadOriginal = cantidadStr,
                cantidadMinutos = minutos,
                cantidadHorasDecimales = Math.Round(minutos / 60.0m, 2),
                cantidadHorasEnteras = cantidadHoras,
                valorHoraOrdinaria = valorHoraOrdinaria,
                factor = factor,
                valorTotal = valorTotal,
                calculo = new
                {
                    formula = $"{cantidadHoras} horas * {factor} * ${valorHoraOrdinaria:N0}",
                    explicacion = $"Se calculó como {cantidadHoras} horas × factor {factor} × valor hora ordinaria ${valorHoraOrdinaria:N0}"
                }
            }
        };
    }

    private TiposHorasExtra ResolverTipoHora(string tipoHoraStr)
    {
        var tipoLimpio = tipoHoraStr.Trim().ToUpperInvariant();
        
        // Intentar conversión directa
        if (Enum.TryParse<TiposHorasExtra>(tipoLimpio, true, out var tipoDirecto))
        {
            return tipoDirecto;
        }

        // Mapeo de aliases comunes
        var aliases = new Dictionary<string, TiposHorasExtra>
        {
            { "DIURNA", TiposHorasExtra.DiurnaOrdinaria },
            { "NOCTURNA", TiposHorasExtra.NocturnaOrdinaria },
            { "FESTIVA", TiposHorasExtra.DiurnaFestiva },
            { "FESTIVA_DIURNA", TiposHorasExtra.DiurnaFestiva },
            { "FESTIVA_NOCTURNA", TiposHorasExtra.NocturnaFestiva },
            { "EXTRA_DIURNA", TiposHorasExtra.HED },
            { "EXTRA_NOCTURNA", TiposHorasExtra.HEN },
            { "RECARGO_NOCTURNO", TiposHorasExtra.RecargoNocturno },
            { "RECARGO_FESTIVO", TiposHorasExtra.RecargoFestivo }
        };

        if (aliases.TryGetValue(tipoLimpio, out var tipoAlias))
        {
            return tipoAlias;
        }

        // Búsqueda parcial
        var tiposCoincidentes = Enum.GetValues<TiposHorasExtra>()
            .Where(t => t.ToString().ToUpperInvariant().Contains(tipoLimpio))
            .ToList();

        if (tiposCoincidentes.Count == 1)
        {
            return tiposCoincidentes.First();
        }

        if (tiposCoincidentes.Count > 1)
        {
            var opciones = string.Join(", ", tiposCoincidentes.Select(t => t.ToString()));
            throw new ArgumentException($"Tipo de hora ambiguo '{tipoHoraStr}'. Opciones posibles: {opciones}");
        }

        throw new ArgumentException($"Tipo de hora no reconocido: '{tipoHoraStr}'. Use mostrarTipos=true para ver todos los tipos disponibles");
    }

    private int ConvertirCantidadAMinutos(string cantidadStr)
    {
        if (string.IsNullOrWhiteSpace(cantidadStr))
            throw new ArgumentException("La cantidad no puede estar vacía");

        cantidadStr = cantidadStr.Trim().ToLowerInvariant();

        // Formato decimal directo (ej: "2.5")
        if (decimal.TryParse(cantidadStr, out var horasDecimales))
        {
            return (int)Math.Round(horasDecimales * 60);
        }

        // Formato HH:MM (ej: "2:30")
        var regexTiempo = new Regex(@"^(\d+):(\d{1,2})$");
        var matchTiempo = regexTiempo.Match(cantidadStr);
        if (matchTiempo.Success)
        {
            var horas = int.Parse(matchTiempo.Groups[1].Value);
            var minutos = int.Parse(matchTiempo.Groups[2].Value);
            return horas * 60 + minutos;
        }

        // Formato con texto (ej: "2h 30min", "2 horas 30 minutos")
        var regexTexto = new Regex(@"(?:(\d+(?:\.\d+)?)\s*(?:h|hora|horas))?\s*(?:(\d+(?:\.\d+)?)\s*(?:m|min|minuto|minutos))?");
        var matchTexto = regexTexto.Match(cantidadStr);
        
        if (matchTexto.Success)
        {
            var horasGrupo = matchTexto.Groups[1].Value;
            var minutosGrupo = matchTexto.Groups[2].Value;
            
            var totalMinutos = 0;
            
            if (!string.IsNullOrEmpty(horasGrupo) && decimal.TryParse(horasGrupo, out var h))
            {
                totalMinutos += (int)Math.Round(h * 60);
            }
            
            if (!string.IsNullOrEmpty(minutosGrupo) && decimal.TryParse(minutosGrupo, out var m))
            {
                totalMinutos += (int)Math.Round(m);
            }
            
            if (totalMinutos > 0)
                return totalMinutos;
        }

        throw new ArgumentException($"Formato de cantidad no reconocido: '{cantidadStr}'. Use formatos como: '2.5', '2:30', '2h 30min'");
    }

    private string ObtenerDescripcionTipo(TiposHorasExtra tipo)
    {
        return tipo switch
        {
            TiposHorasExtra.DiurnaOrdinaria => "Hora extra diurna ordinaria (6:00 AM - 10:00 PM, días laborables)",
            TiposHorasExtra.DiurnaFestiva => "Hora extra diurna festiva (6:00 AM - 10:00 PM, domingos/festivos)",
            TiposHorasExtra.NocturnaOrdinaria => "Hora extra nocturna ordinaria (10:00 PM - 6:00 AM, días laborables)",
            TiposHorasExtra.NocturnaFestiva => "Hora extra nocturna festiva (10:00 PM - 6:00 AM, domingos/festivos)",
            TiposHorasExtra.RecargoNocturno => "Recargo nocturno (10:00 PM - 6:00 AM, jornada ordinaria)",
            TiposHorasExtra.RecargoFestivo => "Recargo festivo (domingos/festivos, jornada ordinaria)",
            TiposHorasExtra.HED => "Hora Extra Diurna - equivale a DiurnaOrdinaria",
            TiposHorasExtra.HEN => "Hora Extra Nocturna - equivale a NocturnaOrdinaria",
            TiposHorasExtra.HEFD => "Hora Extra Festiva Diurna",
            TiposHorasExtra.HEFN => "Hora Extra Festiva Nocturna",
            TiposHorasExtra.RN => "Recargo Nocturno - equivale a RecargoNocturno",
            TiposHorasExtra.RDD => "Recargo Dominical Diurno ocasional compensado",
            TiposHorasExtra.RDN => "Recargo Dominical Nocturno ocasional compensado",
            TiposHorasExtra.RDDHC => "Recargo Dominical Diurno habitual compensado",
            TiposHorasExtra.RDNHC => "Recargo Dominical Nocturno habitual compensado",
            TiposHorasExtra.RDDONC => "Recargo Dominical Diurno ocasional no compensado",
            TiposHorasExtra.RDNONC => "Recargo Dominical Nocturno ocasional no compensado",
            _ => tipo.ToString()
        };
    }
}