# Calculadora Laboral MCP Server

Servidor MCP (Model Context Protocol) en C# .NET 9 para calcular costos de nómina de empleados según la legislación laboral colombiana.

## Características

- **Cálculo completo de nómina**: Salario básico, auxilio de transporte, horas extras, prestaciones sociales y seguridad social
- **Tipos de salario**: Ordinario e Integral
- **Horas extras**: Diferentes tipos de horas extras y recargos
- **Prestaciones sociales**: Prima, cesantías, vacaciones e intereses
- **Seguridad social**: Salud, pensión, ARL y parafiscales (CCF, ICBF, SENA)
- **Parámetros actualizados**: SMLV y auxilio de transporte por año (2022-2026)

## Instalación y Ejecución

### Prerequisitos
- .NET 9.0 SDK

### Compilar y ejecutar
```bash
cd calculadoraLaboral.McpServer
dotnet build
dotnet run
```

## Configuración MCP

Para usar con Claude Code, agregar al archivo de configuración MCP:

```json
{
  "mcpServers": {
    "calculadora-laboral": {
      "command": "dotnet",
      "args": ["run", "--project", "Z:/Experimentos/calculadoraLaboral/calculadoraLaboral.McpServer"],
      "cwd": "Z:/Experimentos/calculadoraLaboral/calculadoraLaboral.McpServer"
    }
  }
}
```

## Herramientas Disponibles

### calcular_nomina

Calcula el costo total de nómina de un empleado.

**Parámetros requeridos:**
- `salarioBasico` (number): Salario básico mensual
- `tipoSalario` (string): "Ordinario" o "Integral"
- `fecha` (string): Fecha en formato YYYY-MM-DD
- `viveCercaAlTrabajo` (boolean): Si vive cerca al trabajo
- `claseRiesgoLaboral` (string): "I", "II", "III", "IV" o "V"

**Parámetros opcionales:**
- `pagosSalariales` (number): Pagos adicionales salariales
- `pagosNoSalariales` (number): Pagos adicionales no salariales
- `horasExtras` (object): Cantidad de horas extras por tipo

**Ejemplo de uso:**
```json
{
  "salarioBasico": 1300000,
  "tipoSalario": "Ordinario",
  "fecha": "2024-01-01",
  "viveCercaAlTrabajo": false,
  "claseRiesgoLaboral": "I",
  "horasExtras": {
    "diurnasOrdinarias": 10,
    "nocturnasOrdinarias": 5
  }
}
```

## Estructura del Proyecto

```
src/
├── Program.cs                     # Punto de entrada
├── McpServer.cs                   # Servidor MCP principal
├── Tools/                         # Herramientas MCP
│   ├── IToolHandler.cs
│   └── CalcularNominaToolHandler.cs
├── Domain/                        # Lógica de negocio
│   ├── Models/                    # Modelos de datos
│   ├── Services/                  # Servicios de cálculo
│   └── Constants/                 # Constantes y parámetros
└── Infrastructure/               # Infraestructura
    └── JsonRpc/                  # JSON-RPC
```

## Validaciones

- Salario mínimo para salarios ordinarios
- Salario mínimo de 13 SMLV para salarios integrales
- Validación de horas extras no negativas
- Validación de clases de riesgo laboral válidas

## Notas Técnicas

- Todos los cálculos están basados en la legislación laboral colombiana
- Los valores se redondean según las reglas estándar de nómina
- El auxilio de transporte aplica solo para salarios hasta 2 SMLV
- Las prestaciones sociales para salario integral se calculan sobre el 70% del salario

## Mantenimiento

Para actualizar parámetros anuales, editar:
- `src/Domain/Constants/ParametrosAnuales.cs`

Para actualizar tarifas de seguridad social:
- `src/Domain/Constants/TarifasSeguridadSocial.cs`