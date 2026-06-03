# 🐜 Hormiguitaz - Simulador Multi-Agente de Colonias de Hormigas

Un simulador de alta rendimiento basado en agentes que modela colonias de hormigas en un entorno 2D discreto, enfocado en explorar comportamiento emergente mediante interacción local, feromonas y recursos dinámicos.

**Arquitectura**: ECS Híbrido (Entity Component System) con arrays de estructuras para máxima eficiencia.

---

## 📋 Estado del Proyecto

### Fase 1: MVP (EN PROGRESO) ✏️

- ✅ Estructura ECS base con arrays paralelos
- ✅ Sistemas: Comportamiento, Movimiento, Feromonas, Energía
- ✅ Grid 2D con tipos de celda (Vacío, Pared, Comida, Nido)
- ✅ Feromonas con difusión y evaporación
- ✅ Rol Worker básico (exploración + retorno)
- ✅ Visualización con Raylib (grid, hormigas, heatmap)
- ✅ 3 tests unitarios pasando
- ✅ Detectar comida y cambiar estado a Returning
- ✅ Detectar nido y volver a Exploring
- ✅ Depositar rastros más fuertes al regresar
- ✅ Energía balanceada
- ⏳ Alimentar hormigas al encontrar comida (regenerar energía)
- ⏳ Mejorar precisión de visualización

**Meta**: Simulación funcional de 500-1000 hormigas @ 60 FPS

### Fase 2: Extensibilidad (Planeado)
- Multi-colonias
- 3 tipos de feromonas (Comida, Retorno, Alerta)
- Roles adicionales (Soldado, Explorador)
- Configuración JSON
- Indexación espacial

### Fase 3: Interactividad + Data (Planeado)
- Pausa/Reanudar
- Control de velocidad
- Exportar a CSV
- Persistencia

### Fase 4: Framework + Evolución (Planeado)
- Mutación de traits
- Algoritmo genético
- Generaciones automáticas

---

## 🛠️ Requisitos

- **.NET 10.0** (o superior)
- **C# 12+**
- Windows, Linux o macOS

### Dependencias

- `Raylib-CsLo` (4.2.0.9) - Renderizado gráfico
- `xUnit` - Testing
- `Moq` - Mocking

---

## ⚡ Quick Start

### 1. Clonar el repositorio

```bash
git clone https://github.com/red-daemon/hormiguitaz.git
cd hormiguitaz
```

### 2. Compilar

```bash
dotnet build
```

### 3. Ejecutar simulación

```bash
dotnet run --project AntSimulator
```

### 4. Ejecutar tests

```bash
dotnet test
```

### 5. Build optimizado (Release)

```bash
dotnet build -c Release
dotnet run --project AntSimulator -c Release
```

---

## 🏗️ Arquitectura

### ECS Híbrido: Array of Structs

```
Entidad = índice entero en arrays paralelos
Componente = struct (datos, sin métodos)
Sistema = clase con lógica que itera arrays
World = contenedor central
```

**Ventajas:**
- 🔥 Cache locality extrema
- ⚡ SIMD-friendly
- 🚀 Cero allocations en loop principal
- 📊 Predecible y determinístico

### Jerarquía de Sistemas (Orden de ejecución)

```
Tick N:
  1. BehaviorSystem → decide velocidades basadas en sensores
  2. MovementSystem → actualiza posiciones
  3. PheromoneSystem → deposita y difunde feromonas
  4. EnergySystem → consume energía, marca muertes
  5. Render → dibuja frame
```

### Estructura de Carpetas

```
AntSimulator/
├── Core/
│   └── SimulationEngine.cs
├── ECS/
│   ├── Archetypes/
│   │   └── AntArchetype.cs (arrays paralelos)
│   ├── Components/
│   │   ├── Position.cs
│   │   ├── Velocity.cs
│   │   ├── AntComponent.cs
│   │   └── PhysicsComponent.cs
│   └── Systems/
│       ├── ISystem.cs (interfaz)
│       ├── BehaviorSystem.cs
│       ├── MovementSystem.cs
│       ├── PheromoneSystem.cs
│       └── EnergySystem.cs
├── Agents/
│   ├── AntState.cs (Exploring, Returning, Dead)
│   ├── AntAction.cs
│   └── Roles/
│       ├── IRoleStrategy.cs
│       └── WorkerRole.cs
├── Environment/
│   ├── GridSystem.cs
│   ├── Cell.cs
│   └── CellType.cs
├── Pheromones/
│   ├── PheromoneGrid.cs
│   ├── PheromoneLayer.cs
│   └── PheromoneType.cs
├── Colonies/
│   ├── Colony.cs
│   └── ColonyTraits.cs
├── Visualization/
│   ├── IRenderer.cs
│   └── RaylibRenderer.cs
├── World.cs (contenedor central)
├── Constants.cs
└── Program.cs
```

---

## 📊 Métricas de Rendimiento (Target)

```
10,000 ants @ 60 FPS
= ~167 microsegundos por tick

Budget estimado:
├─ BehaviorSystem: 50µs
├─ MovementSystem: 10µs
├─ PheromoneSystem: 40µs
├─ EnergySystem: 10µs
└─ Rendering: 20µs
   Total: ~160µs ✅
```

**Memory Layout (1000 hormigas):**
- ~56 KB total
- Cache line friendly (64 bytes)

---

## 🚀 Cómo Extender

### Agregar un nuevo Sistema

1. Crear clase que implemente `ISystem`
2. Override `Update(float deltaTime, World world)`
3. Registrar en `SimulationEngine.RegisterSystems()`

```csharp
public class NuevoSystem : ISystem
{
    public void Update(float deltaTime, World world)
    {
        var ants = world.Ants.GetAntsMutable();
        for (int i = 0; i < world.Ants.EntityCount; i++)
        {
            // lógica aquí
        }
    }
}
```

### Agregar un nuevo Rol

1. Implementar `IRoleStrategy`
2. Override `DecideAction()`
3. Retornar `AntAction` con velocidad

```csharp
public class MiRol : IRoleStrategy
{
    public AntAction DecideAction(int id, Vector2 position, AntComponent ant, 
                                   GridSystem grid, PheromoneGrid pheromones,
                                   ColonyTraits traits, Vector2 nestPosition)
    {
        return new AntAction { Velocity = /* ... */ };
    }
}
```

---

## 🧪 Testing

```bash
# Todos los tests
dotnet test

# Test específico
dotnet test --filter "AntArchetypeTests"

# Watch mode
dotnet watch test
```

**Tests actuales:**
- ✅ `CreateAnt_AddedToArray_CountIncreases`
- ✅ `DestroyAnt_RemovedFromActive_StateIsDead`
- ✅ `World_RegistersSystem_StoresItInOrder`

---

## 📚 Documentación Completa

Para más detalles técnicos, ver:
- **`docs/TECHNICAL_DESIGN.md`** — Arquitectura, decisiones de diseño, roadmap
- **`docs/PRD Simulador Multi‑Agente de hormigas.md`** — Requisitos y visión
- **`CLAUDE.md`** — Guía de desarrollo para Claude Code

---

## 🎮 Controles (Raylib)

- **ESC** — Cerrar simulación
- **Window resizable** — Ajustar tamaño de ventana

*(Más controles en Fase 3)*

---

## 📝 Notas de Desarrollo

### Convenciones de Código

- **PascalCase** — miembros públicos, tipos, métodos
- **_camelCase** — campos privados
- **camelCase** — variables locales
- **ALL_CAPS** — constantes globales
- **Structs** — componentes (data only)
- **Classes** — sistemas, managers, utilities
- **Interfaces** — estrategias (ISystem, IRoleStrategy)

### Performance Crítico

- Usar `Span<T>` para acceso a arrays (sin allocations)
- Evitar LINQ en loops principales
- Evitar boxing/unboxing
- Usar arrays paralelos en lugar de objetos

### Git Workflow

```bash
# Feature branch
git checkout -b feat/descripcion

# Commit descriptivo
git commit -m "feat: descripción

Detalles adicionales si es necesario"

# Push
git push origin feat/descripcion

# PR en GitHub
```

---

## 🤝 Contribuir

1. Fork el repositorio
2. Crear una rama (`git checkout -b feat/mi-feature`)
3. Commit cambios (`git commit -am 'Add mi-feature'`)
4. Push a la rama (`git push origin feat/mi-feature`)
5. Abrir un Pull Request

---

## 📜 Licencia

MIT

---

## 📧 Contacto

**Red Daemon** - baruch.gaxiola@gmail.com

---

**Última actualización**: 2026-06-03 (Fase 1 MVP - Comportamiento funcional)
