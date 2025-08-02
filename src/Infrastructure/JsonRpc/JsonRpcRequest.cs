using System.Text.Json;

namespace CalculadoraLaboral.McpServer.Infrastructure.JsonRpc;

public class JsonRpcRequest
{
    public string JsonRpc { get; set; } = "2.0";
    public string Method { get; set; } = string.Empty;
    public JsonElement? Params { get; set; }
    public object? Id { get; set; }
}

public class JsonRpcResponse
{
    public string JsonRpc { get; set; } = "2.0";
    public object? Result { get; set; }
    public JsonRpcError? Error { get; set; }
    public object? Id { get; set; }
}

public class JsonRpcError
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
}

public static class JsonRpcErrorCodes
{
    public const int ParseError = -32700;
    public const int InvalidRequest = -32600;
    public const int MethodNotFound = -32601;
    public const int InvalidParams = -32602;
    public const int InternalError = -32603;
}