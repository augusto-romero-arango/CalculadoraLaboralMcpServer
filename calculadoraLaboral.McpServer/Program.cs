using CalculadoraLaboral.McpServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// Deshabilitar completamente el logging para evitar interferencia con JSON-RPC
builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(LogLevel.None);

// Configurar servicios
builder.Services.AddSingleton<McpServer>();

var host = builder.Build();

// Obtener el servidor y ejecutar
var mcpServer = host.Services.GetRequiredService<McpServer>();
await mcpServer.RunAsync();