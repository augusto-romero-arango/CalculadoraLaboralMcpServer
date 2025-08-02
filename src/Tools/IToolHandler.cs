using System.Text.Json;

namespace CalculadoraLaboral.McpServer.Tools;

public interface IToolHandler
{
    string Name { get; }
    string Description { get; }
    object Schema { get; }
    Task<object> HandleAsync(JsonElement arguments);
}