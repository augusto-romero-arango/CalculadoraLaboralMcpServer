using System.Text.Json;
using CalculadoraLaboral.McpServer.Domain.Models;
using CalculadoraLaboral.McpServer.Domain.Services;

namespace CalculadoraLaboral.McpServer.Tools;

public class CalcularNominaRequest
{
    public decimal SalarioBasico { get; set; }
    public string TipoSalario { get; set; } = "Ordinario";
    public DateTime Fecha { get; set; }
    public bool ViveCercaAlTrabajo { get; set; }
    public string ClaseRiesgoLaboral { get; set; } = "I";
    public decimal PagosSalariales { get; set; } = 0;
    public decimal PagosNoSalariales { get; set; } = 0;
    public HorasExtrasRequest? HorasExtras { get; set; }
}

public class HorasExtrasRequest
{
    public int DiurnasOrdinarias { get; set; } = 0;
    public int DiurnasFestivas { get; set; } = 0;
    public int NocturnasOrdinarias { get; set; } = 0;
    public int NocturnasFestivas { get; set; } = 0;
    public int RecargosNocturnos { get; set; } = 0;
    public int RecargosFestivos { get; set; } = 0;
}

public class CalcularNominaToolHandler : IToolHandler
{
    public string Name => "calcular_nomina";
    public string Description => "Calcula el costo total de nómina de un empleado incluyendo gastos directos y provisiones del empleador";

    public object Schema => new
    {
        type = "object",
        properties = new
        {
            salarioBasico = new
            {
                type = "number",
                description = "Salario básico mensual del empleado"
            },
            tipoSalario = new
            {
                type = "string",
                @enum = new[] { "Ordinario", "Integral" },
                description = "Tipo de salario del empleado",
                @default = "Ordinario"
            },
            fecha = new
            {
                type = "string",
                format = "date",
                description = "Fecha para parámetros anuales (formato: YYYY-MM-DD)"
            },
            viveCercaAlTrabajo = new
            {
                type = "boolean",
                description = "Si el empleado vive cerca al lugar de trabajo (afecta auxilio de transporte)"
            },
            claseRiesgoLaboral = new
            {
                type = "string",
                @enum = new[] { "I", "II", "III", "IV", "V" },
                description = "Clasificación de riesgo laboral",
                @default = "I"
            },
            pagosSalariales = new
            {
                type = "number",
                description = "Pagos adicionales que constituyen salario",
                @default = 0
            },
            pagosNoSalariales = new
            {
                type = "number",
                description = "Pagos adicionales que no constituyen salario",
                @default = 0
            },
            horasExtras = new
            {
                type = "object",
                properties = new
                {
                    diurnasOrdinarias = new { type = "number", @default = 0 },
                    diurnasFestivas = new { type = "number", @default = 0 },
                    nocturnasOrdinarias = new { type = "number", @default = 0 },
                    nocturnasFestivas = new { type = "number", @default = 0 },
                    recargosNocturnos = new { type = "number", @default = 0 },
                    recargosFestivos = new { type = "number", @default = 0 }
                },
                description = "Cantidad de horas extras y recargos por tipo"
            }
        },
        required = new[] { "salarioBasico", "tipoSalario", "fecha", "viveCercaAlTrabajo", "claseRiesgoLaboral" }
    };

    public async Task<object> HandleAsync(JsonElement arguments)
    {
        try
        {
            var request = ParseRequest(arguments);
            var resultado = await CalcularNominaAsync(request);
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

    private CalcularNominaRequest ParseRequest(JsonElement arguments)
    {
        var request = new CalcularNominaRequest();

        if (arguments.TryGetProperty("salarioBasico", out var salarioBasico))
            request.SalarioBasico = salarioBasico.GetDecimal();

        if (arguments.TryGetProperty("tipoSalario", out var tipoSalario))
            request.TipoSalario = tipoSalario.GetString() ?? "Ordinario";

        if (arguments.TryGetProperty("fecha", out var fecha))
        {
            var fechaStr = fecha.GetString();
            if (!DateTime.TryParse(fechaStr, out var fechaParsed))
            {
                fechaParsed = DateTime.Now;
            }
            request.Fecha = fechaParsed;
        }

        if (arguments.TryGetProperty("viveCercaAlTrabajo", out var viveCerca))
            request.ViveCercaAlTrabajo = viveCerca.GetBoolean();

        if (arguments.TryGetProperty("claseRiesgoLaboral", out var riesgo))
            request.ClaseRiesgoLaboral = riesgo.GetString() ?? "I";

        if (arguments.TryGetProperty("pagosSalariales", out var pagosSal))
            request.PagosSalariales = pagosSal.GetDecimal();

        if (arguments.TryGetProperty("pagosNoSalariales", out var pagosNoSal))
            request.PagosNoSalariales = pagosNoSal.GetDecimal();

        if (arguments.TryGetProperty("horasExtras", out var horasExtras))
        {
            request.HorasExtras = new HorasExtrasRequest();
            
            if (horasExtras.TryGetProperty("diurnasOrdinarias", out var diurnasOrd))
                request.HorasExtras.DiurnasOrdinarias = diurnasOrd.GetInt32();
            
            if (horasExtras.TryGetProperty("diurnasFestivas", out var diurnasFest))
                request.HorasExtras.DiurnasFestivas = diurnasFest.GetInt32();
            
            if (horasExtras.TryGetProperty("nocturnasOrdinarias", out var nocturnasOrd))
                request.HorasExtras.NocturnasOrdinarias = nocturnasOrd.GetInt32();
            
            if (horasExtras.TryGetProperty("nocturnasFestivas", out var nocturnasFest))
                request.HorasExtras.NocturnasFestivas = nocturnasFest.GetInt32();
            
            if (horasExtras.TryGetProperty("recargosNocturnos", out var recargoNoct))
                request.HorasExtras.RecargosNocturnos = recargoNoct.GetInt32();
            
            if (horasExtras.TryGetProperty("recargosFestivos", out var recargoFest))
                request.HorasExtras.RecargosFestivos = recargoFest.GetInt32();
        }

        return request;
    }

    private async Task<object> CalcularNominaAsync(CalcularNominaRequest request)
    {
        // Convertir tipos de string a enum con manejo de errores
        if (!Enum.TryParse<TipoSalario>(request.TipoSalario, out var tipoSalario))
        {
            throw new ArgumentException($"TipoSalario inválido: {request.TipoSalario}");
        }
        
        if (!Enum.TryParse<ClasesDeRiesgo>(request.ClaseRiesgoLaboral, out var claseRiesgo))
        {
            throw new ArgumentException($"ClaseRiesgoLaboral inválido: {request.ClaseRiesgoLaboral}");
        }

        // Crear el servicio de liquidación
        var liquidacion = new LiquidacionNominaService(
            request.SalarioBasico, 
            tipoSalario, 
            request.Fecha);

        // Configurar parámetros
        liquidacion.ModificarRiesgoLaboral(claseRiesgo);
        liquidacion.ModificarViveCercaAlLugarDeTrabajo(request.ViveCercaAlTrabajo);

        if (request.PagosSalariales > 0)
            liquidacion.ModificarValorSalarial(request.PagosSalariales);

        if (request.PagosNoSalariales > 0)
            liquidacion.ModificarValorNoSalarial(request.PagosNoSalariales);

        // Registrar horas extras si existen
        if (request.HorasExtras != null)
        {
            if (request.HorasExtras.DiurnasOrdinarias > 0)
                liquidacion.RegistrarHorasExtras(TiposHorasExtra.DiurnaOrdinaria, request.HorasExtras.DiurnasOrdinarias);
            
            if (request.HorasExtras.DiurnasFestivas > 0)
                liquidacion.RegistrarHorasExtras(TiposHorasExtra.DiurnaFestiva, request.HorasExtras.DiurnasFestivas);
            
            if (request.HorasExtras.NocturnasOrdinarias > 0)
                liquidacion.RegistrarHorasExtras(TiposHorasExtra.NocturnaOrdinaria, request.HorasExtras.NocturnasOrdinarias);
            
            if (request.HorasExtras.NocturnasFestivas > 0)
                liquidacion.RegistrarHorasExtras(TiposHorasExtra.NocturnaFestiva, request.HorasExtras.NocturnasFestivas);
            
            if (request.HorasExtras.RecargosNocturnos > 0)
                liquidacion.RegistrarHorasExtras(TiposHorasExtra.RecargoNocturno, request.HorasExtras.RecargosNocturnos);
            
            if (request.HorasExtras.RecargosFestivos > 0)
                liquidacion.RegistrarHorasExtras(TiposHorasExtra.RecargoFestivo, request.HorasExtras.RecargosFestivos);
        }

        // Realizar liquidación
        var resumen = liquidacion.Liquidar();

        await Task.CompletedTask;

        return new
        {
            success = true,
            data = new
            {
                gastos = new
                {
                    salarioBasico = resumen.Gastos.SalarioBasico,
                    auxilioTransporte = resumen.Gastos.AuxilioTransporte,
                    pagosSalariales = resumen.Gastos.PagosSalariales,
                    pagosNoSalariales = resumen.Gastos.PagosNoSalariales,
                    horasExtrasYRecargos = resumen.Gastos.HorasExtrasYRecargos
                },
                totalGastos = resumen.TotalGastos,
                provisionEmpleador = new
                {
                    prestacionesSociales = resumen.ProvisionEmpleador.PrestacionesSociales.Select(p => new
                    {
                        nombre = p.Nombre,
                        valor = p.Valor,
                        descripcion = p.Descripcion
                    }),
                    seguridadSocial = resumen.ProvisionEmpleador.SeguridadSocial.Select(s => new
                    {
                        nombre = s.Nombre,
                        valor = s.Valor,
                        descripcion = s.Descripcion
                    })
                },
                totalProvisionEmpleador = resumen.TotalProvisionEmpleador,
                totalLiquidacion = resumen.TotalLiquidacion
            }
        };
    }
}