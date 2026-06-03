using AntSimulator.Colonies;
using AntSimulator.ECS.Archetypes;
using AntSimulator.ECS.Systems;
using AntSimulator.Environment;
using AntSimulator.Pheromones;

namespace AntSimulator;

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
    {
        _systems.Add(system);
    }

    public void Update(float deltaTime)
    {
        foreach (var system in _systems)
        {
            system.Update(deltaTime, this);
        }

        CurrentTick++;
    }
}
