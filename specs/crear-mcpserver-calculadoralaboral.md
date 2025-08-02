# Especificación: Servidor MCP Calculadora Laboral

## Descripción General
Este documento especifica la implementación de un servidor MCP (Model Context Protocol) en C# .NET 9 que extrae y reutiliza la lógica de cálculo laboral existente en el proyecto TypeScript `Bitakora.Calculadoras`. El servidor proporcionará herramientas para calcular costos de nómina de empleados según la legislación laboral colombiana.

## Objetivos
1. **Portar lógica de negocio**: Extraer y convertir los algoritmos de cálculo laboral de TypeScript a C#
2. **Crear servidor MCP**: Implementar un servidor que exponga las funcionalidades como herramientas MCP
3. **Mantener precisión**: Garantizar que los cálculos en C# produzcan resultados idénticos al código original
4. **Facilitar integración**: Proporcionar una API clara para uso con Claude Code y otros clientes MCP

## Arquitectura del Sistema

### Estructura del Proyecto
```
calculadoraLaboral.McpServer/
├── src/
│   ├── Program.cs                    # Punto de entrada del servidor MCP
│   ├── McpServer.cs                  # Implementación principal del servidor
│   ├── Tools/                        # Definición de herramientas MCP
│   │   ├── CalcularNominaToolHandler.cs
│   │   └── IToolHandler.cs
│   ├── Domain/                       # Lógica de negocio portada desde TS
│   │   ├── Models/
│   │   │   ├── TipoSalario.cs
│   │   │   ├── ClasesDeRiesgo.cs
│   │   │   ├── TiposHorasExtra.cs
│   │   │   ├── ResumenLiquidacion.cs
│   │   │   ├── GastoNomina.cs
│   │   │   └── ProvisionesEmpleador.cs
│   │   ├── Services/
│   │   │   ├── LiquidacionNominaService.cs
│   │   │   ├── SalarioService.cs
│   │   │   ├── RemuneracionService.cs
│   │   │   ├── AuxilioTransporteService.cs
│   │   │   ├── HorasExtrasService.cs
│   │   │   ├── PrestacionesSocialesService.cs
│   │   │   └── SeguridadSocialService.cs
│   │   └── Constants/
│   │       └── ParametrosAnuales.cs
│   └── Infrastructure/
│       ├── JsonRpc/                  # Manejo JSON-RPC
│       └── Logging/                  # Sistema de logging
├── calculadoraLaboral.McpServer.csproj
└── README.md
```

## Especificación Funcional

### Datos de Entrada

#### Parámetros Básicos del Empleado
- **salarioBasico** (decimal): Salario base mensual del empleado
- **tipoSalario** (enum): `Ordinario` o `Integral`
- **fecha** (DateTime): Fecha para determinar parámetros anuales (SMLV, auxilio transporte)

#### Información Laboral
- **viveCercaAlTrabajo** (bool): Determina si aplica auxilio de transporte
- **claseRiesgoLaboral** (enum): Clasificación de riesgo (I, II, III, IV, V)

#### Pagos Adicionales (Opcionales)
- **pagosSalariales** (decimal): Pagos que constituyen salario
- **pagosNoSalariales** (decimal): Pagos que no constituyen salario

#### Horas Extras (Opcional)
- **horasExtrasDiurnasOrdinarias** (int): Cantidad de horas extras diurnas ordinarias
- **horasExtrasDiurnasFestivas** (int): Cantidad de horas extras diurnas festivas
- **horasExtrasNocturnasOrdinarias** (int): Cantidad de horas extras nocturnas ordinarias
- **horasExtrasNocturnasFestivas** (int): Cantidad de horas extras nocturnas festivas
- **recargosNocturnos** (int): Cantidad de horas con recargo nocturno
- **recargosFestivos** (int): Cantidad de horas con recargo festivo

### Datos de Salida

#### Resumen de Liquidación
```json
{
  "gastos": {
    "salarioBasico": 1300000,
    "auxilioTransporte": 162000,
    "pagosSalariales": 0,
    "pagosNoSalariales": 0,
    "horasExtrasYRecargos": 0
  },
  "totalGastos": 1462000,
  "provisionEmpleador": {
    "prestacionesSociales": [
      {
        "nombre": "Prima",
        "valor": 121833.33,
        "descripcion": "Prima de servicios"
      },
      {
        "nombre": "Cesantías",
        "valor": 121833.33,
        "descripcion": "Cesantías"
      },
      {
        "nombre": "Vacaciones",
        "valor": 65000,
        "descripcion": "Vacaciones"
      },
      {
        "nombre": "Intereses de Cesantías",
        "valor": 14620,
        "descripcion": "Intereses sobre cesantías"
      }
    ],
    "seguridadSocial": [
      {
        "nombre": "Salud",
        "valor": 110500,
        "descripcion": "Aporte a salud por el empleador"
      },
      {
        "nombre": "Pensión",
        "valor": 156000,
        "descripcion": "Aporte a pensión por el empleador"
      },
      {
        "nombre": "ARL",
        "valor": 6630,
        "descripcion": "Administradora de Riesgos Laborales"
      },
      {
        "nombre": "Caja de Compensación",
        "valor": 52000,
        "descripcion": "Aporte a caja de compensación familiar"
      },
      {
        "nombre": "ICBF",
        "valor": 39000,
        "descripcion": "Instituto Colombiano de Bienestar Familiar"
      },
      {
        "nombre": "SENA",
        "valor": 26000,
        "descripcion": "Servicio Nacional de Aprendizaje"
      }
    ]
  },
  "totalProvisionEmpleador": 712416.66,
  "totalLiquidacion": 2174416.66
}
```

## Especificación Técnica

### Herramientas MCP Expuestas

#### calcular_nomina
**Descripción**: Calcula el costo total de nómina de un empleado incluyendo gastos directos y provisiones del empleador.

**Parámetros**:
```json
{
  "type": "object",
  "properties": {
    "salarioBasico": {
      "type": "number",
      "description": "Salario básico mensual del empleado"
    },
    "tipoSalario": {
      "type": "string",
      "enum": ["Ordinario", "Integral"],
      "description": "Tipo de salario del empleado"
    },
    "fecha": {
      "type": "string",
      "format": "date",
      "description": "Fecha para parámetros anuales (formato: YYYY-MM-DD)"
    },
    "viveCercaAlTrabajo": {
      "type": "boolean",
      "description": "Si el empleado vive cerca al lugar de trabajo (afecta auxilio de transporte)"
    },
    "claseRiesgoLaboral": {
      "type": "string",
      "enum": ["I", "II", "III", "IV", "V"],
      "description": "Clasificación de riesgo laboral"
    },
    "pagosSalariales": {
      "type": "number",
      "description": "Pagos adicionales que constituyen salario",
      "default": 0
    },
    "pagosNoSalariales": {
      "type": "number", 
      "description": "Pagos adicionales que no constituyen salario",
      "default": 0
    },
    "horasExtras": {
      "type": "object",
      "properties": {
        "diurnasOrdinarias": {"type": "number", "default": 0},
        "diurnasFestivas": {"type": "number", "default": 0},
        "nocturnasOrdinarias": {"type": "number", "default": 0},
        "nocturnasFestivas": {"type": "number", "default": 0},
        "recargosNocturnos": {"type": "number", "default": 0},
        "recargosFestivos": {"type": "number", "default": 0}
      }
    }
  },
  "required": ["salarioBasico", "tipoSalario", "fecha", "viveCercaAlTrabajo", "claseRiesgoLaboral"]
}
```

## Implementación de Servicios

### Servicios de Cálculo
1. **SalarioService**: Manejo de salario básico y tipo de salario
2. **AuxilioTransporteService**: Cálculo de auxilio de transporte según SMLV
3. **RemuneracionService**: Gestión de pagos salariales y no salariales
4. **HorasExtrasService**: Cálculo de horas extras y recargos
5. **PrestacionesSocialesService**: Cálculo de prima, cesantías, vacaciones e intereses
6. **SeguridadSocialService**: Cálculo de aportes a salud, pensión, ARL y parafiscales

### Constantes y Parámetros
- **ParametrosAnuales**: SMLV y auxilio de transporte por año
- **TarifasSeguridadSocial**: Porcentajes de aportes a seguridad social
- **FactoresRiesgoLaboral**: Factores de riesgo para ARL

## Configuración y Despliegue

### Dependencias .NET
- **.NET 9.0**: Framework base
- **System.Text.Json**: Serialización JSON
- **Microsoft.Extensions.Logging**: Sistema de logging
- **Microsoft.Extensions.DependencyInjection**: Inyección de dependencias

### Configuración del Servidor MCP
```json
{
  "mcpServers": {
    "calculadora-laboral": {
      "command": "dotnet",
      "args": ["run", "--project", "calculadoraLaboral.McpServer"],
      "cwd": "Z:/Experimentos/calculadoraLaboral/calculadoraLaboral.McpServer"
    }
  }
}
```

## Validación y Testing

### Casos de Prueba
1. **Salario mínimo ordinario**: Validar cálculos con SMLV
2. **Salario integral**: Verificar cálculos con salario superior a 10 SMLV
3. **Horas extras**: Comprobar diferentes tipos de horas extras
4. **Clases de riesgo**: Validar cálculo de ARL para cada clase
5. **Comparación con TypeScript**: Verificar equivalencia de resultados

### Criterios de Aceptación
- Los cálculos deben ser idénticos al código TypeScript original
- El servidor debe responder en menos de 1 segundo para cálculos simples
- Manejo robusto de errores y validación de entrada
- Logging detallado para debugging

## Mantenimiento y Evolución

### Actualizaciones Anuales
- **SMLV**: Actualizar salario mínimo legal vigente
- **Auxilio de transporte**: Actualizar valor del auxilio
- **Tarifas**: Revisar porcentajes de seguridad social y parafiscales

### Extensiones Futuras
- Cálculo de liquidación final
- Soporte para diferentes tipos de contrato
- Integración con sistemas de nómina
- API REST adicional

## Documentación de Uso

### Ejemplo de Uso con Claude Code
```
Calcular la nómina de un empleado con:
- Salario básico: $1,300,000
- Tipo: Ordinario  
- Vive lejos del trabajo
- Riesgo laboral clase I
- Sin horas extras
- Fecha: 2024-01-01
```

### Comando MCP
```json
{
  "method": "tools/call",
  "params": {
    "name": "calcular_nomina",
    "arguments": {
      "salarioBasico": 1300000,
      "tipoSalario": "Ordinario",
      "fecha": "2024-01-01",
      "viveCercaAlTrabajo": false,
      "claseRiesgoLaboral": "I"
    }
  }
}
```

## Referencias
- Código fuente original: `Bitakora.Calculadoras/src/CostosDeNomina/`
- Documentación MCP: https://modelcontextprotocol.io/
- Guía MCP C#: https://modelcontextprotocol.io/quickstart/server#c%23