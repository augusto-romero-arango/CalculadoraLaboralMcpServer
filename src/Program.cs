using CalculadoraLaboral.McpServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// Configurar logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Warning); // Solo errores y warnings para no interferir con JSON-RPC

// Configurar servicios
builder.Services.AddSingleton<McpServer>();

var host = builder.Build();

// Obtener el servidor y ejecutar
var mcpServer = host.Services.GetRequiredService<McpServer>();
await mcpServer.RunAsync();