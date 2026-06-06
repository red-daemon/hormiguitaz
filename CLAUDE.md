# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

---

## Project Overview

**Ant Colony Multi-Agent Simulator** — a performance-focused simulation framework exploring emergent behavior in ant colonies through configurable traits, pheromone dynamics, and extensible agent roles.

**Key documents:**
- `docs/PRD Simulador Multi‑Agente de hormigas.md` — Product requirements and vision
- `docs/TECHNICAL_DESIGN.md` — Detailed architecture, design decisions, roadmap

---

## Architecture: ECS Hybrid (Array of Structs)

This is NOT traditional OOP. The core pattern:

```
Entity = integer index in parallel arrays
Component = struct (data only, no methods)
System = class with Update(deltaTime, world) logic
World = central container managing entities, components, systems
```

**Why this matters for development:**

- ✅ **Cache locality** → tight loops over arrays are extremely fast
- ✅ **Zero allocations** in main loop → deterministic, no GC pauses
- ✅ **Straightforward parallelization** → iterate array ranges
- ❌ **Different mental model** → not class hierarchies, not object references

**Key files to understand first:**
1. `Core/World.cs` — central orchestrator
2. `ECS/Archetypes/AntArchetype.cs` — parallel arrays, entity lifecycle
3. `ECS/Systems/ISystem.cs` + implementations — where logic lives
4. `ECS/Components/*.cs` — data structs

**Mental model shift:**
```
❌ ant.Position = new Vector2(10, 20)
✅ positions[antId] = new Vector2(10, 20)

❌ foreach (var ant in colony.Ants)
✅ foreach (var position in world.Ants.GetPositions())
```

---

## Common Development Commands

### Setup & Build

```bash
# Create solution (one-time)
dotnet new sln -n ant-simulator

# Build project
dotnet build

# Build release (optimized)
dotnet build -c Release

# Clean
dotnet clean
```

### Running

```bash
# Run simulator
dotnet run --project AntSimulator

# Run in release mode (recommended for performance testing)
dotnet run --project AntSimulator -c Release
```

### Testing

```bash
# Run all tests
dotnet test

# Run single test class
dotnet test --filter "ClassName"

# Run single test method
dotnet test --filter "FullyQualifiedName~MethodName"

# Run with verbose output
dotnet test --verbosity detailed

# Watch mode (re-run on changes)
dotnet watch test
```

### Debugging

```bash
# Run with debugger in VS Code
# Press F5 or use "Run and Debug" panel

# Run with additional logging
dotnet run --project AntSimulator -- --verbose
```

### Adding Dependencies

```bash
cd AntSimulator
dotnet add package PackageName --version X.Y.Z

cd ../AntSimulator.Tests
dotnet add package PackageName --version X.Y.Z
```

---

## Where to Add Things

### Adding a New System

1. Create `ECS/Systems/YourSystem.cs`
2. Implement `ISystem` interface with `Update(float deltaTime, World world)`
3. Register in `SimulationEngine` constructor or `Program.cs`:

```csharp
world.RegisterSystem(new YourSystem());
```

Systems execute sequentially in registration order. **Order matters** — put dependent systems after their dependencies.

### Adding a New Role (Agent Behavior)

1. Create `Agents/Roles/YourRole.cs` implementing `IRoleStrategy`
2. Implement `DecideAction()` to return `AntAction`
3. Assign in `AntArchetype.CreateAnt()` or dynamically

Roles are strategies — same interface, different implementations.

### Adding a New Component

1. Create struct in `ECS/Components/YourComponent.cs`
2. Add array to `AntArchetype`: `private YourComponent[] _yourComponents`
3. Add accessor method: `public Span<YourComponent> GetYourComponentsMutable()`
4. Initialize in `AntArchetype` constructor and `Resize()` method
5. Use in systems via `world.Ants.GetYourComponentsMutable()`

### Adding Configuration

1. Extend `Config/SimulationConfig.cs` or create new config class
2. Update `appsettings.json` with defaults
3. Load in `Config/ConfigLoader.cs` using `System.Text.Json`
4. Pass to World/Systems during initialization

---

## Performance Expectations

**Target:** 10,000 ants @ 60 FPS = ~167µs per tick

**Key bottlenecks to watch:**
- Pheromone diffusion (expensive array operations)
- Behavior decision-making (spatial queries)
- Rendering (Raylib batching)

**Optimization approach:**
1. Profile with `dotnet-bench` before optimizing
2. Use `Span<T>` everywhere in hot loops
3. Avoid allocations in `ISystem.Update()`
4. Cache frequently accessed data (traits, grid references)

**Memory layout:**
- 1,000 ants ≈ 56KB total (Position + Velocity + AntComponent + Physics arrays)
- Cache line friendly (64 bytes)
- GPU SIMD works well with contiguous arrays

---

## Extending the Simulation (Roadmap)

### Phases 1-2 (MVP + Multi-colonia)
- Single-threaded, simple behaviors
- Basic pheromone layers
- Configurable traits from JSON

### Phase 3 (Interactivity + Data Export)
- Pause/resume, speed control
- CSV export for analysis
- Persistence (save/load state)

### Phase 4 (Framework + Evolution)
- Genetic algorithm traits mutation
- Generational experiments
- Plugin system for custom roles/systems

**Before each phase:** Check `TECHNICAL_DESIGN.md` section 10 for design details.

---

## Code Style & Conventions

**C# naming:**
- `PascalCase` for public members, types, methods
- `_camelCase` for private fields
- `camelCase` for local variables

**ECS conventions:**
- Structs for components (no methods)
- Classes for systems, managers, utilities
- Interfaces for strategies (IRoleStrategy, ISystem, IRenderer)
- Enums for state (AntState, CellType, PheromoneType)

**Constants:**
- Global constants in `Core/Constants.cs`
- Use `readonly` for class-level constants
- Names in `UPPER_SNAKE_CASE`

**Tests:**
- Filename: `*Tests.cs`
- Organized by namespace mirror
- Use xUnit `[Fact]` for parameterless, `[Theory]` for parameterized
- Assertions: `Assert.*` methods

**Comentarios (Español):**
- ✅ **OBLIGATORIO:** Comentarios en español para clases, métodos y lógica compleja
- ✅ **Nivel de clase:** Comentario antes de `public class` explicando propósito
- ✅ **Nivel de método:** Comentario antes de método explicando qué hace, parámetros y valor de retorno
- ✅ **Lógica compleja:** Comentarios inline en español explicando por qué (no el qué)
- ❌ NO auto-explicar código legible: `// incrementar contador` está prohibido si el código ya lo dice
- ❌ NO comentarios en inglés en el cuerpo (metadata en ingles está bien: `[Fact]`, namespaces)

---

## Quick Debugging Tips

**Check entity count grows:**
```csharp
Console.WriteLine($"Ants: {world.Ants.EntityCount}");
```

**Inspect component values:**
```csharp
var positions = world.Ants.GetPositions();
Console.WriteLine($"Ant 0 pos: {positions[0]}");
```

**Profile a system:**
```csharp
var sw = System.Diagnostics.Stopwatch.StartNew();
world.Update(0.016f);
sw.Stop();
Console.WriteLine($"Tick took {sw.ElapsedMilliseconds}ms");
```

**Verify traits are loaded:**
```csharp
var colony = world.Colonies[1];
Console.WriteLine($"Speed: {colony.Traits.Speed}");
```

---

## Useful References

- **System.Numerics.Vector2** — Use `Vector2.Distance()`, `Vector2.Lerp()`, normalization
- **Raylib-CsLo** — Drawing primitives, colors, camera control (check Raylib docs, binding is direct)
- **xUnit** — Test framework (facts, theories, fixtures)
- **System.Text.Json** — Config deserialization (source generators for performance)

---

## Next Steps When Starting

1. **Read** `docs/TECHNICAL_DESIGN.md` section 3-5 for architecture details
2. **Create** the solution and project files with commands above
3. **Implement** Phase 1 MVP in order: World → AntArchetype → GridSystem → first Systems
4. **Test** each component incrementally
5. **Reference** this file when adding new patterns

---

## Notes for Contributors

- **Single-threaded by design** — simpler debugging, deterministic behavior
- **One World per simulation** — globals are anti-patterns; pass World to all systems
- **Traits are immutable at runtime (Phases 1-3)** — prepare for mutation in Phase 4 with copy-on-write if needed
- **Rendering is abstracted** — `IRenderer` interface allows Raylib/MonoGame/headless swaps
- **Config-driven over code-driven** — traits, grid size, spawn positions from JSON
