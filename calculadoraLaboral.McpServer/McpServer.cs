using System.Text.Json;
using CalculadoraLaboral.McpServer.Infrastructure.JsonRpc;
using CalculadoraLaboral.McpServer.Tools;
using Microsoft.Extensions.Logging;

namespace CalculadoraLaboral.McpServer;

public class McpServer
{
    private readonly ILogger<McpServer> _logger;
    private readonly Dictionary<string, IToolHandler> _toolHandlers;

    public McpServer(ILogger<McpServer> logger)
    {
        _logger = logger;
        _toolHandlers = new Dictionary<string, IToolHandler>
        {
            { "calcular_nomina", new CalcularNominaToolHandler() },
            { "obtener_parametros_laborales", new ParametrosLaboralesToolHandler() },
            { "calcular_horas_extras", new HorasExtrasToolHandler() }
        };
    }

    public async Task RunAsync()
    {
        try
        {
            while (true)
            {
                var input = await Console.In.ReadLineAsync();
                if (input == null)
                    break;

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                var response = await ProcessRequestAsync(input);
                if (response != null)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = false
                    };
                    var responseJson = JsonSerializer.Serialize(response, options);
                    await Console.Out.WriteLineAsync(responseJson);
                    await Console.Out.FlushAsync();
                }
            }
        }
        catch (Exception)
        {
            // Silenciar errores para evitar interferencia con JSON-RPC
        }
    }

    private async Task<JsonRpcResponse?> ProcessRequestAsync(string input)
    {
        try
        {
            var request = JsonSerializer.Deserialize<JsonRpcRequest>(input);

            if (request == null)
            {
                return CreateErrorResponse(null, JsonRpcErrorCodes.InvalidRequest, "Invalid request format");
            }


            return request.Method switch
            {
                "initialize" => await HandleInitializeAsync(request),
                "notifications/initialized" => null, // Notification, no response needed
                "tools/list" => await HandleToolsListAsync(request),
                "tools/call" => await HandleToolCallAsync(request),
                _ => CreateErrorResponse(request.Id, JsonRpcErrorCodes.MethodNotFound, $"Method '{request.Method}' not found")
            };
        }
        catch (JsonException)
        {
            return CreateErrorResponse(null, JsonRpcErrorCodes.ParseError, "Parse error");
        }
        catch (Exception)
        {
            return CreateErrorResponse(null, JsonRpcErrorCodes.InternalError, "Internal error");
        }
    }

    private async Task<JsonRpcResponse> HandleInitializeAsync(JsonRpcRequest request)
    {
        await Task.CompletedTask;
        
        return new JsonRpcResponse
        {
            Id = request.Id,
            Result = new
            {
                protocolVersion = "2024-11-05",
                capabilities = new
                {
                    tools = new { }
                },
                serverInfo = new
                {
                    name = "calculadora-laboral-mcp",
                    version = "1.0.0"
                }
            }
        };
    }

    private async Task<JsonRpcResponse> HandleToolsListAsync(JsonRpcRequest request)
    {
        await Task.CompletedTask;

        var tools = _toolHandlers.Values.Select(handler => new
        {
            name = handler.Name,
            description = handler.Description,
            inputSchema = handler.Schema
        }).ToArray();

        return new JsonRpcResponse
        {
            Id = request.Id,
            Result = new { tools }
        };
    }

    private async Task<JsonRpcResponse> HandleToolCallAsync(JsonRpcRequest request)
    {
        try
        {
            if (!request.Params.HasValue)
            {
                return CreateErrorResponse(request.Id, JsonRpcErrorCodes.InvalidParams, "Missing params");
            }

            var paramsElement = request.Params.Value;
            
            if (!paramsElement.TryGetProperty("name", out var nameElement))
            {
                return CreateErrorResponse(request.Id, JsonRpcErrorCodes.InvalidParams, "Missing tool name");
            }

            var toolName = nameElement.GetString();
            if (string.IsNullOrEmpty(toolName) || !_toolHandlers.TryGetValue(toolName, out var handler))
            {
                return CreateErrorResponse(request.Id, JsonRpcErrorCodes.MethodNotFound, $"Tool '{toolName}' not found");
            }

            var arguments = paramsElement.TryGetProperty("arguments", out var argsElement) 
                ? argsElement 
                : new JsonElement();

            var result = await handler.HandleAsync(arguments);

            return new JsonRpcResponse
            {
                Id = request.Id,
                Result = new
                {
                    content = new[]
                    {
                        new
                        {
                            type = "text",
                            text = result
                        }
                    }
                }
            };
        }
        catch (Exception ex)
        {
            return CreateErrorResponse(request.Id, JsonRpcErrorCodes.InternalError, $"Tool execution error: {ex.Message}");
        }
    }

    private static JsonRpcResponse CreateErrorResponse(object? id, int code, string message, object? data = null)
    {
        return new JsonRpcResponse
        {
            Id = id,
            Error = new JsonRpcError
            {
                Code = code,
                Message = message,
                Data = data
            }
        };
    }
}