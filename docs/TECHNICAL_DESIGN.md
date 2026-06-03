# 🔧 Technical Design Document - Ant Simulator

**Status:** Draft  
**Version:** 1.0  
**Last Updated:** 2026-06-02

---

## 1. Introducción

Este documento describe la arquitectura técnica del **Simulador Multi-Agente de Colonias de Hormigas**, un sistema de simulación basado en **Entity Component System (ECS) híbrido** con enfoque en rendimiento, extensibilidad y mantenibilidad.

### Audiencia
- Desarrolladores C#
- Arquitectos de software
- Contribuidores futuros

### Referencias
- PRD: `PRD Simulador Multi‑Agente de hormigas.md`

---

## 2. Stack Tecnológico

### Runtime & Framework
```
.NET 8
├── Lenguaje: C#
├── Plataformas: Windows, Linux, macOS
└── Target: Netcoreapp8.0
```

**Justificación:**
- ⚡ Rendimiento compilado (50-100x más rápido que Python)
- 🔒 Type-safe (reducir bugs)
- 🎯 SIMD support nativo
- 📦 Multiplataforma sin fricciones

### Gráficos
```
Raylib-CsLo (Raylib C# binding)
├── GPU rendering
├── Input handling
├── Audio (futuro)
└── Cross-platform
```

**Justificación:**
- Lightweight (vs MonoGame)
- Rápido de iterar en MVP
- Fácil migrar a MonoGame si es necesario
- Simple para visualizar grid + feromonas

### Testing
```
xUnit + Moq
├── Testing framework moderno
└── Mocking library
```

### Configuración
```
System.Text.Json (built-in)
├── Cero dependencias externas
└── Source generators para performance
```

### Control de Versiones & CI/CD
```
GitHub
├── Source control
└── GitHub Actions (tests automáticos)
```

---

## 3. Arquitectura General

### 3.1 Patrón: Entity Component System (ECS) Híbrido

**Concepto:**
- **Entity** = Índice entero en arrays paralelos
- **Component** = Struct con datos (NOT comportamiento)
- **System** = Lógica que opera sobre componentes
- **World** = Contenedor central que gestiona todo

**Ventajas de Array-of-Structs (AoS):**
```
✅ Cache locality extrema (datos contiguos en memoria)
✅ SIMD-friendly (procesar múltiples en paralelo CPU)
✅ Cero allocations en loop principal
✅ Fácil de paralelizar (iterar arrays)
✅ Predecible en memory layout
```

### 3.2 Jerarquía de Sistemas

```
World (contenedor central)
│
├─ AntArchetype (entidades + componentes hormigas)
├─ GridSystem (estado del entorno)
├─ PheromoneGrid (feromonas por tipo)
├─ Colonies (configuración de colonias)
│
└─ Systems (orden de ejecución)
   ├─ BehaviorSystem (decidir acciones)
   ├─ MovementSystem (aplicar movimiento)
   ├─ CollisionSystem (resolver colisiones)
   ├─ PheromoneSystem (difusión + evaporación)
   └─ EnergySystem (gastar energía)
```

---

## 4. Estructura del Proyecto

```
ant-simulator/
│
├── AntSimulator/                      # Proyecto principal
│   ├── Core/
│   │   ├── SimulationEngine.cs        # Loop principal, coordinador
│   │   ├── World.cs                   # Contenedor central ECS
│   │   └── Constants.cs               # Constantes globales
│   │
│   ├── ECS/
│   │   ├── Archetypes/
│   │   │   └── AntArchetype.cs        # Arrays paralelos de hormigas
│   │   │
│   │   ├── Components/
│   │   │   ├── Position.cs            # Vector2
│   │   │   ├── Velocity.cs            # Vector2
│   │   │   ├── AntComponent.cs        # State, energy, etc
│   │   │   └── PhysicsComponent.cs    # Mass, friction
│   │   │
│   │   └── Systems/
│   │       ├── ISystem.cs             # Interfaz base
│   │       ├── BehaviorSystem.cs      # Decisiones de agentes
│   │       ├── MovementSystem.cs      # Aplicar velocidades
│   │       ├── CollisionSystem.cs     # Detección de colisiones
│   │       ├── PheromoneSystem.cs     # Difusión + evaporación
│   │       └── EnergySystem.cs        # Metabolismo
│   │
│   ├── Agents/
│   │   ├── AntState.cs                # Enum de estados (Exploring, Returning, Idle, Dead)
│   │   ├── AntAction.cs               # Decisión de acción
│   │   ├── BehaviorDecider.cs         # Lógica de decisión (state machine)
│   │   └── Roles/
│   │       ├── IRoleStrategy.cs       # Interfaz extensible
│   │       ├── WorkerRole.cs          # Rol obrera
│   │       ├── SoldierRole.cs         # Rol soldado (Fase 2)
│   │       └── ExplorerRole.cs        # Rol explorador (Fase 2)
│   │
│   ├── Environment/
│   │   ├── GridSystem.cs              # Grid 2D de celdas
│   │   ├── Cell.cs                    # Estructura de celda
│   │   ├── CellType.cs                # Enum (Empty, Wall, Food, Nest, Excavable)
│   │   └── Materials.cs               # Tipos de material (dureza, excavabilidad)
│   │
│   ├── Pheromones/
│   │   ├── PheromoneGrid.cs           # Contenedor de capas
│   │   ├── PheromoneLayer.cs          # Una capa (comida, regreso, alerta)
│   │   ├── PheromoneType.cs           # Enum (Food, Return, Alert)
│   │   └── Physics.cs                 # Difusión + evaporación
│   │
│   ├── Colonies/
│   │   ├── Colony.cs                  # Definición de colonia
│   │   ├── ColonyTraits.cs            # Parámetros (speed, sensitivity, etc)
│   │   └── QueenEntity.cs             # Reina (Fase 2)
│   │
│   ├── Config/
│   │   ├── ConfigLoader.cs            # Cargar JSON
│   │   ├── SimulationConfig.cs        # Estructura de config
│   │   ├── GridConfig.cs              # Config del grid
│   │   ├── AntConfig.cs               # Config de hormigas
│   │   └── Schemas.cs                 # Validación
│   │
│   ├── Visualization/
│   │   ├── IRenderer.cs               # Interfaz abstracción
│   │   ├── RaylibRenderer.cs          # Implementación Raylib
│   │   ├── Camera.cs                  # Zoom + pan
│   │   └── ColorPalette.cs            # Colores por colonia + elementos
│   │
│   ├── Data/
│   │   ├── Metrics.cs                 # Cálculo de métricas (eficiencia, etc)
│   │   ├── Exporter.cs                # Export a CSV
│   │   └── SimulationRecorder.cs      # Grabar estados (Fase 3)
│   │
│   ├── Persistence/
│   │   ├── Serializer.cs              # Guardar estado (Fase 3)
│   │   └── Deserializer.cs            # Cargar estado (Fase 3)
│   │
│   ├── appsettings.json               # Config por defecto
│   ├── Program.cs                     # Entry point
│   └── AntSimulator.csproj
│
├── AntSimulator.Tests/
│   ├── ECS/
│   │   └── AntArchetypeTests.cs
│   ├── Environment/
│   │   └── GridSystemTests.cs
│   ├── Pheromones/
│   │   └── PheromoneGridTests.cs
│   ├── Colonies/
│   │   └── ColonyTraitsTests.cs
│   ├── Config/
│   │   └── ConfigLoaderTests.cs
│   └── AntSimulator.Tests.csproj
│
├── .github/
│   └── workflows/
│       └── dotnet.yml                 # CI: build + tests
│
├── .gitignore
├── ant-simulator.sln
├── README.md
└── docs/
    ├── PRD Simulador Multi‑Agente de hormigas.md
    └── TECHNICAL_DESIGN.md (este archivo)
```

---

## 5. Componentes Principales

### 5.1 Entity Component System

#### AntArchetype.cs
Contenedor de arrays paralelos para todas las hormigas.

```csharp
public class AntArchetype
{
    private Vector2[] _positions;
    private Vector2[] _velocities;
    private AntComponent[] _ants;
    private PhysicsComponent[] _physics;
    
    private int _count;                    // Número de entidades vivas
    private Queue<int> _freeIndices;      // Pool de IDs reciclados
    
    // Entity = índice en arrays
    public int CreateAnt(int colonyId, Vector2 pos) { ... }
    public void DestroyAnt(int id) { ... }
    
    // Acceso a componentes (con Span<T> para performance)
    public ReadOnlySpan<Vector2> GetPositions() { ... }
    public Span<Vector2> GetPositionsMutable() { ... }
    public ReadOnlySpan<AntComponent> GetAnts() { ... }
    public Span<AntComponent> GetAntsMutable() { ... }
}
```

**Características:**
- ✅ Reciclaje de IDs (Queue de índices libres)
- ✅ Redimensionamiento automático (dobla capacidad)
- ✅ Acceso eficiente via Span<T>
- ✅ Separación component → array

### 5.2 Componentes

#### Position & Velocity
```csharp
public struct Position { public Vector2 Value; }
public struct Velocity { public Vector2 Value; }
```

#### AntComponent
```csharp
public struct AntComponent
{
    public AntState State;              // Exploring, Returning, etc
    public int ColonyId;
    public float Energy;                // 0-100
    public float PheromoneCarry;        // 0-1 (carga de comida)
    public int TicksInState;            // Para timeouts
    public IRoleStrategy Role;          // Worker, Soldier, etc
}
```

#### PhysicsComponent
```csharp
public struct PhysicsComponent
{
    public float Mass;
    public float Friction;
    public float MaxSpeed;
}
```

### 5.3 Sistemas

#### ISystem.cs (Interfaz base)
```csharp
public interface ISystem
{
    void Update(float deltaTime, World world);
}
```

#### BehaviorSystem.cs
**Responsabilidad:** Decidir acciones basadas en estado actual.

```csharp
public class BehaviorSystem : ISystem
{
    public void Update(float deltaTime, World world)
    {
        var positions = world.Ants.GetPositions();
        var ants = world.Ants.GetAntsMutable();
        var grid = world.Grid;
        var pheromones = world.Pheromones;
        
        for (int i = 0; i < world.Ants.EntityCount; i++)
        {
            if (ants[i].State == AntState.Dead) continue;
            
            // Decidir acción basada en estado
            var action = ants[i].Role.DecideAction(
                id: i,
                position: positions[i],
                ant: ants[i],
                grid: grid,
                pheromones: pheromones,
                traits: world.Colonies[ants[i].ColonyId].Traits
            );
            
            // Aplicar acción a velocidad (será consumida por MovementSystem)
            // ...
        }
    }
}
```

#### MovementSystem.cs
**Responsabilidad:** Actualizar posiciones basadas en velocidades.

```csharp
public class MovementSystem : ISystem
{
    public void Update(float deltaTime, World world)
    {
        var positions = world.Ants.GetPositionsMutable();
        var velocities = world.Ants.GetVelocitiesMutable();
        var grid = world.Grid;
        
        for (int i = 0; i < world.Ants.EntityCount; i++)
        {
            // Aplicar velocidad
            positions[i] += velocities[i] * deltaTime;
            
            // Clamp a límites del grid
            positions[i] = Vector2.Clamp(
                positions[i],
                Vector2.Zero,
                new Vector2(grid.Width - 1, grid.Height - 1)
            );
        }
    }
}
```

#### PheromoneSystem.cs
**Responsabilidad:** Deposición, difusión y evaporación de feromonas.

```csharp
public class PheromoneSystem : ISystem
{
    public void Update(float deltaTime, World world)
    {
        var ants = world.Ants.GetAnts();
        var positions = world.Ants.GetPositions();
        var pheromones = world.Pheromones;
        
        // 1. Deposición
        for (int i = 0; i < world.Ants.EntityCount; i++)
        {
            if (ants[i].PheromoneCarry > 0)
            {
                int x = (int)positions[i].X;
                int y = (int)positions[i].Y;
                pheromones.Deposit(
                    x, y,
                    ants[i].ColonyId,
                    PheromoneType.Food,
                    ants[i].PheromoneCarry * world.Colonies[ants[i].ColonyId].Traits.PheromoneDepositRate
                );
            }
        }
        
        // 2. Difusión + evaporación
        pheromones.Update(deltaTime);
    }
}
```

#### EnergySystem.cs
**Responsabilidad:** Metabolismo de hormigas.

```csharp
public class EnergySystem : ISystem
{
    public void Update(float deltaTime, World world)
    {
        var ants = world.Ants.GetAntsMutable();
        var velocities = world.Ants.GetVelocities();
        
        for (int i = 0; i < world.Ants.EntityCount; i++)
        {
            // Costo de movimiento
            float movementCost = velocities[i].Length() * 
                                 world.Colonies[ants[i].ColonyId].Traits.Speed * 
                                 0.1f;
            
            ants[i].Energy -= movementCost * deltaTime;
            
            // Muerte por falta de energía
            if (ants[i].Energy <= 0)
                ants[i].State = AntState.Dead;
        }
    }
}
```

### 5.4 GridSystem

```csharp
public class GridSystem
{
    private Cell[,] _grid;
    public int Width { get; }
    public int Height { get; }
    
    public Cell GetCell(int x, int y) { ... }
    public void SetCell(int x, int y, Cell cell) { ... }
    public bool IsWalkable(int x, int y) { ... }
    public void Excavate(int x, int y) { ... }
}

public struct Cell
{
    public CellType Type;           // Empty, Wall, Food, Nest
    public float FoodAmount;        // 0-100
    public bool IsExcavated;        // Modificado por hormigas
    public int ColonyNestId;        // Referencia a colonia si es nido
}

public enum CellType { Empty, Wall, Food, Nest, Excavable }
```

### 5.5 PheromoneGrid

```csharp
public class PheromoneGrid
{
    private Dictionary<PheromoneType, PheromoneLayer> _layers;
    
    public PheromoneGrid(int width, int height)
    {
        _layers[PheromoneType.Food] = new PheromoneLayer(width, height);
        _layers[PheromoneType.Return] = new PheromoneLayer(width, height);
        _layers[PheromoneType.Alert] = new PheromoneLayer(width, height);
    }
    
    public float GetPheromone(int x, int y, int colonyId, PheromoneType type) { ... }
    public void Deposit(int x, int y, int colonyId, PheromoneType type, float amount) { ... }
    public void Update(float deltaTime) { ... }  // Difusión + evaporación
}

public class PheromoneLayer
{
    // Arrays 2D por colonia para evitar interferencia
    private Dictionary<int, float[,]> _coloniesData;  // [x, y] → intensidad
    
    public void Diffuse(float diffusionRate) { ... }
    public void Evaporate(float evaporationRate) { ... }
}

public enum PheromoneType { Food, Return, Alert }
```

### 5.6 Colony & ColonyTraits

```csharp
public class Colony
{
    public int Id { get; }
    public Vector2 NestPosition { get; set; }
    public ColonyTraits Traits { get; set; }
    public int PopulationCount { get; private set; }
    
    public void IncrementPopulation() { ... }
    public void DecrementPopulation() { ... }
}

public class ColonyTraits
{
    // Configurables desde JSON
    public float Speed { get; set; } = 1.0f;
    public float PheromonesSensitivity { get; set; } = 0.8f;
    public float ExloreBias { get; set; } = 0.3f;
    public float PheromoneDepositRate { get; set; } = 0.5f;
    public float Aggression { get; set; } = 0.2f;
    public float EnergyRegenRate { get; set; } = 5.0f;
    public float MaxEnergy { get; set; } = 100.0f;
    
    // Métodos auxiliares
    public static ColonyTraits LoadFromJson(string json) { ... }
}
```

### 5.7 World (Contenedor Central)

```csharp
public class World
{
    public AntArchetype Ants { get; }
    public GridSystem Grid { get; }
    public PheromoneGrid Pheromones { get; }
    public Dictionary<int, Colony> Colonies { get; }
    
    private List<ISystem> _systems;
    public int CurrentTick { get; private set; }
    
    public World(int gridWidth, int gridHeight)
    {
        Ants = new AntArchetype();
        Grid = new GridSystem(gridWidth, gridHeight);
        Pheromones = new PheromoneGrid(gridWidth, gridHeight);
        Colonies = new Dictionary<int, Colony>();
        _systems = new List<ISystem>();
        CurrentTick = 0;
    }
    
    public void RegisterSystem(ISystem system) 
        => _systems.Add(system);
    
    public void Update(float deltaTime)
    {
        // Ejecutar sistemas en orden
        foreach (var system in _systems)
            system.Update(deltaTime, this);
        
        CurrentTick++;
    }
}
```

### 5.8 SimulationEngine

```csharp
public class SimulationEngine
{
    private World _world;
    private IRenderer _renderer;
    private Metrics _metrics;
    
    private float _deltaTime = 0.016f;  // 60 FPS
    private bool _isRunning = true;
    private bool _isPaused = false;
    private float _simulationSpeed = 1.0f;
    
    public SimulationEngine(SimulationConfig config)
    {
        _world = new World(config.Grid.Width, config.Grid.Height);
        _renderer = new RaylibRenderer(_world);
        _metrics = new Metrics();
        
        InitializeWorld(config);
    }
    
    public void Run()
    {
        while (_isRunning && !Raylib.WindowShouldClose())
        {
            HandleInput();
            
            if (!_isPaused)
                _world.Update(_deltaTime * _simulationSpeed);
            
            _metrics.Update(_world);
            _renderer.Render(_world, _metrics);
        }
    }
    
    private void HandleInput() { ... }
}
```

---

## 6. Decisiones de Diseño

### 6.1 ¿Por qué Entity = Índice?

**Decisión:** Cada hormiga es representada por un índice entero (int id).

**Alternativas consideradas:**
| Opción | Pros | Contras |
|--------|------|---------|
| ID único (GUID) | Fácil debugging | Lento, requiere mapping |
| Referencia de objeto | Intuitivio | Heap allocation, GC pressure |
| **Índice de array** | Rápido, cache-friendly | Requiere pool de IDs |

**Justificación:** Con 10,000 hormigas, el índice es **óptimo para rendimiento**. El pool de IDs (Queue) resuelve el problema de fragmentación.

### 6.2 ¿Por qué Structs para componentes?

**Decisión:** Los componentes son `struct`, NO `class`.

**Ventajas:**
- ✅ Stack allocation (faster)
- ✅ Contiguity en arrays (cache L1 hit)
- ✅ No hay indirection pointers
- ✅ Determinístico (no GC)

**Desventaja:**
- ❌ Copias implícitas (mitigado con Span<T>)

### 6.3 ¿Por qué Span<T> para acceso?

**Decisión:** Los sistemas acceden a arrays via `Span<T>` y `ReadOnlySpan<T>`.

```csharp
// En BehaviorSystem:
var positions = world.Ants.GetPositions();        // ReadOnlySpan
for (int i = 0; i < world.Ants.EntityCount; i++)
{
    var pos = positions[i];  // Sin boxing, sin allocation
}
```

**Razón:** Span<T> es "ventana a un array" sin allocations.

### 6.4 ¿Por qué ColonyId en AntComponent?

**Decisión:** Cada hormiga tiene `int ColonyId` para saber a qué colonia pertenece.

**Alternativa:** Organizar Ants por colonia en múltiples Archetypes.

**Razón:** Un Archetype central es más simple. El colonyId permite queries globales (ej: "todos los Ants").

### 6.5 ¿Un thread o múltiples?

**Decisión:** **Un thread principal** (por ahora).

**Justificación:**
- ✅ Simplifica debugging
- ✅ Evita race conditions
- ✅ Determinístico
- ✅ Raylib es single-threaded

**Futuro:** Si profiler muestra bottleneck, paralelizar sistemas con `Parallel.For` sobre ranges del array.

### 6.6 ¿Raylib o MonoGame?

**Decisión:** **Raylib-CsLo para MVP**.

**Razón:**
- ⚡ Ligero, simple
- 🎯 Directo para grid + heatmaps
- 🚀 Prototipado rápido
- 🔄 Fácil migrar a MonoGame si es necesario

---

## 7. Flujo de Simulación

### Orden de Ejecución por Tick

```
Tick N:
├─ 1. BehaviorSystem.Update()
│    └─ Examina grid, feromonas, decide velocidades
│
├─ 2. MovementSystem.Update()
│    └─ Aplica velocidades → actualiza posiciones
│
├─ 3. CollisionSystem.Update()
│    └─ Resuelve colisiones, come comida
│
├─ 4. PheromoneSystem.Update()
│    ├─ Deposita feromonas donde hay hormigas
│    └─ Difusión + evaporación
│
├─ 5. EnergySystem.Update()
│    └─ Resta energía, marca muertes
│
├─ 6. Metrics.Update()
│    └─ Calcula eficiencia, población, etc
│
└─ 7. Render()
     └─ Dibuja grid, hormigas, feromonas, UI
```

**Razón del orden:** Las decisiones se hacen con información del tick anterior (feromonas "viejas" guían). Esto evita inestabilidad.

---

## 8. Consideraciones de Performance

### 8.1 Memory Layout

```
AntArchetype en memoria (continuo):
[Pos[0], Pos[1], ..., Pos[N]] ← 2KB (1000 ants × 2 floats × 4 bytes)
[Vel[0], Vel[1], ..., Vel[N]] ← 2KB
[Ant[0], Ant[1], ..., Ant[N]] ← 40KB (1000 ants × 40 bytes/struct)
[Phys[0], Phys[1], ..., Phys[N]] ← 12KB

Total: ~56KB para 1000 hormigas
Cache line: 64 bytes → cabe mucho en L1/L2
```

### 8.2 Budget de rendimiento (target)

```
10,000 ants @ 60 FPS
= ~167 microsegundos por tick

Breakdown estimado:
├─ BehaviorSystem: 50µs    (sensores, decisiones)
├─ MovementSystem: 10µs    (sumar velocidades)
├─ CollisionSystem: 30µs   (checks en grid)
├─ PheromoneSystem: 40µs   (difusión)
├─ EnergySystem: 10µs      (cálculos simples)
└─ Rendering: 20µs         (Raylib)
   Total: ~160µs ✅
```

### 8.3 Optimizaciones críticas

| Bottleneck | Solución |
|------------|----------|
| **Búsqueda de comida cercana** | Spatial hash / QuadTree (Fase 2) |
| **Difusión de feromonas** | Stencil convolution (SIMD) |
| **Colisiones** | Spatial partition |
| **Rendering** | Batch drawing |

### 8.4 Profiling

Usar **BenchmarkDotNet** para medir sistemas:

```csharp
[MemoryDiagnoser]
public class SimulationBench
{
    [Benchmark]
    public void BehaviorSystemTick() { ... }
}
```

---

## 9. Extensibilidad

### 9.1 Agregar un nuevo Sistema

1. Implementar `ISystem`
2. Registrar en `World.RegisterSystem()`
3. Ejecuta en el loop principal

**Ejemplo:** Agregar GravitySystem

```csharp
public class GravitySystem : ISystem
{
    public void Update(float deltaTime, World world)
    {
        var velocities = world.Ants.GetVelocitiesMutable();
        for (int i = 0; i < world.Ants.EntityCount; i++)
            velocities[i] += new Vector2(0, -9.8f) * deltaTime;
    }
}

// En Program.cs:
world.RegisterSystem(new GravitySystem());
```

### 9.2 Agregar un nuevo Rol

1. Implementar `IRoleStrategy`
2. Override `DecideAction()`
3. Usar en `AntComponent.Role`

```csharp
public class ScoutRole : IRoleStrategy
{
    public AntAction DecideAction(int id, Vector2 pos, AntComponent ant, 
                                  GridSystem grid, PheromoneGrid pheromones,
                                  ColonyTraits traits)
    {
        // Lógica de explorador
        return new AntAction { Velocity = ... };
    }
}
```

### 9.3 Agregar un nuevo Componente

1. Crear struct (ej: `SensingComponent`)
2. Agregar array a `AntArchetype`
3. Acceder en sistemas vía `GetSensingMutable()`

```csharp
public class AntArchetype
{
    private SensingComponent[] _sensing;
    
    public Span<SensingComponent> GetSensingMutable() => 
        new Span<SensingComponent>(_sensing, 0, _count);
}
```

---

## 10. Roadmap Técnico

### Fase 1 (MVP) - Semana 1-2

- [x] Estructura ECS base
- [x] AntArchetype con arrays paralelos
- [x] GridSystem
- [x] PheromoneLayer simple
- [x] BehaviorSystem básico (1 rol: Worker)
- [x] MovementSystem
- [x] Raylib integration
- [ ] Tests unitarios básicos

**Entregable:** Simulación funcional con 1 colonia, 100-1000 ants explorando y depositando feromonas.

### Fase 2 (Extensibilidad) - Semana 3-4

- [ ] Multi-colonias
- [ ] 3 feromonas (Food, Return, Alert)
- [ ] Sistema de roles (Worker, Soldier, Explorer)
- [ ] ConfigLoader (JSON → World)
- [ ] Spatial indexing (QuadTree para queries)
- [ ] Tests de integración

**Entregable:** 2-3 colonias compitiendo, configurables por JSON.

### Fase 3 (Interactividad + Data) - Semana 5-6

- [ ] Pausa / reanudar
- [ ] Cambiar velocidad
- [ ] UI para métricas en tiempo real
- [ ] Exportar a CSV
- [ ] Persistencia (guardar/cargar)

**Entregable:** Simulación interactiva, análisis de eficiencia.

### Fase 4 (Framework + Evolución) - Semana 7+

- [ ] Preparación para evolución genética
- [ ] Traits mutation system
- [ ] Generaciones automáticas
- [ ] Documentación para extensión
- [ ] Contribución abierta

**Entregable:** Base para experimentación científica.

---

## 11. Testing Strategy

### Unit Tests (Fase 1)

```csharp
// AntArchetypeTests.cs
[Fact]
public void CreateAnt_AddedToArray_CountIncreases()
{
    var archetype = new AntArchetype();
    archetype.CreateAnt(1, Vector2.Zero);
    Assert.Equal(1, archetype.EntityCount);
}

[Fact]
public void DestroyAnt_RemovedFromActive_CountDecreases()
{
    var archetype = new AntArchetype();
    int id = archetype.CreateAnt(1, Vector2.Zero);
    archetype.DestroyAnt(id);
    Assert.Equal(0, archetype.EntityCount);
}
```

### Integration Tests (Fase 2)

```csharp
// SimulationIntegrationTests.cs
[Fact]
public void Simulation_1000Ants_RunsWithoutCrash()
{
    var config = new SimulationConfig();
    var world = new World(500, 500);
    
    for (int i = 0; i < 1000; i++)
        world.Ants.CreateAnt(1, new Vector2(250, 250));
    
    for (int tick = 0; tick < 100; tick++)
        world.Update(0.016f);
    
    Assert.Equal(1000, world.Ants.EntityCount);
}
```

### Performance Tests (Fase 4)

```csharp
// SimulationPerfTests.cs
[Benchmark]
[Arguments(1000)]
[Arguments(10000)]
public void Update_N_Ants(int antCount)
{
    var world = new World(500, 500);
    for (int i = 0; i < antCount; i++)
        world.Ants.CreateAnt(1, new Vector2(250, 250));
    
    world.Update(0.016f);
}
```

---

## 12. Apéndices

### A. Dependencias Externas

```xml
<!-- AntSimulator.csproj -->
<ItemGroup>
    <PackageReference Include="Raylib-CsLo" Version="5.0.0" />
    <PackageReference Include="System.Numerics.Vectors" Version="4.5.0" />
</ItemGroup>

<!-- AntSimulator.Tests.csproj -->
<ItemGroup>
    <PackageReference Include="xunit" Version="2.7.0" />
    <PackageReference Include="Moq" Version="4.20.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
</ItemGroup>
```

### B. Convenciones de código

- **Naming:** PascalCase (públicos), _camelCase (privados)
- **Structs:** Para componentes, data-only
- **Classes:** Para sistemas, managers
- **Interfaces:** ISystem, IRoleStrategy
- **Enums:** AntState, CellType, PheromoneType
- **Constants:** ALL_CAPS en Constants.cs

### C. Glosario

| Término | Definición |
|---------|-----------|
| **Entity** | Hormiga individual, representada por índice en arrays |
| **Component** | Datos asociados a una entidad (Position, Velocity, etc) |
| **System** | Lógica que opera sobre componentes |
| **World** | Contenedor central que gestiona entidades, componentes, sistemas |
| **Archetype** | Patrón de componentes que comparten todas las hormigas |
| **Traits** | Parámetros de colonia (speed, sensitivity, etc) |
| **Pheromone** | Campo químico que guía comportamiento |
| **Role** | Estrategia de comportamiento (Worker, Soldier, Explorer) |

---

**Documento preparado para desarrollo en C# + .NET 8 + Raylib.**

**Última revisión:** 2026-06-02
