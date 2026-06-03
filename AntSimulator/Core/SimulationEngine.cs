using System.Numerics;
using AntSimulator.Colonies;
using AntSimulator.ECS.Systems;
using AntSimulator.Environment;
using AntSimulator.Visualization;
using Raylib_CsLo;

namespace AntSimulator.Core;

public class SimulationEngine
{
    private World _world;
    private IRenderer _renderer;
    private float _deltaTime = 0.016f;
    private bool _isRunning = true;

    public SimulationEngine(int gridWidth, int gridHeight, int antCount)
    {
        _world = new World(gridWidth, gridHeight);
        _renderer = new RaylibRenderer(_world, gridWidth, gridHeight);

        InitializeWorld(gridWidth, gridHeight, antCount);
        RegisterSystems();
    }

    private void InitializeWorld(int gridWidth, int gridHeight, int antCount)
    {
        // Create main colony
        var nestPos = new Vector2(gridWidth / 2, gridHeight / 2);
        var traits = new ColonyTraits();
        var colony = new Colony(1, nestPos, traits);
        _world.Colonies.Add(1, colony);

        // Create ants
        for (int i = 0; i < antCount; i++)
        {
            var randomPos = new Vector2(
                (float)(Random.Shared.NextDouble() * gridWidth),
                (float)(Random.Shared.NextDouble() * gridHeight)
            );
            _world.Ants.CreateAnt(1, randomPos);
            colony.IncrementPopulation();
        }

        // Add some food to the grid
        for (int i = 0; i < 5; i++)
        {
            int x = Random.Shared.Next(0, gridWidth);
            int y = Random.Shared.Next(0, gridHeight);
            var cell = _world.Grid.GetCell(x, y);
            cell.Type = CellType.Food;
            cell.FoodAmount = 100f;
            _world.Grid.SetCell(x, y, cell);
        }

        // Mark nest
        var nestCell = _world.Grid.GetCell((int)nestPos.X, (int)nestPos.Y);
        nestCell.Type = CellType.Nest;
        nestCell.ColonyNestId = 1;
        _world.Grid.SetCell((int)nestPos.X, (int)nestPos.Y, nestCell);
    }

    private void RegisterSystems()
    {
        _world.RegisterSystem(new BehaviorSystem());
        _world.RegisterSystem(new MovementSystem());
        _world.RegisterSystem(new PheromoneSystem());
        _world.RegisterSystem(new EnergySystem());
    }

    public void Run()
    {
        while (_isRunning && !Raylib.WindowShouldClose())
        {
            _world.Update(_deltaTime);
            _renderer.Render();
        }

        Raylib.CloseWindow();
    }
}
