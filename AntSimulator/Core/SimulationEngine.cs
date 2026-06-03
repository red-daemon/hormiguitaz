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

        // Create ants near nest
        for (int i = 0; i < antCount; i++)
        {
            var randomPos = new Vector2(
                (float)(gridWidth / 2 + (Random.Shared.NextDouble() - 0.5) * 50),
                (float)(gridHeight / 2 + (Random.Shared.NextDouble() - 0.5) * 50)
            );
            randomPos = Vector2.Clamp(randomPos, Vector2.Zero, new Vector2(gridWidth - 1, gridHeight - 1));
            _world.Ants.CreateAnt(1, randomPos);
            colony.IncrementPopulation();
        }

        // Add multiple food patches
        for (int i = 0; i < 8; i++)
        {
            int x = Random.Shared.Next(50, gridWidth - 50);
            int y = Random.Shared.Next(50, gridHeight - 50);

            // Create a small cluster of food
            for (int fx = x - 5; fx <= x + 5; fx++)
            {
                for (int fy = y - 5; fy <= y + 5; fy++)
                {
                    if (fx >= 0 && fx < gridWidth && fy >= 0 && fy < gridHeight)
                    {
                        var cell = _world.Grid.GetCell(fx, fy);
                        cell.Type = CellType.Food;
                        cell.FoodAmount = 100f;
                        _world.Grid.SetCell(fx, fy, cell);
                    }
                }
            }
        }

        // Mark nest
        for (int nx = (int)nestPos.X - 3; nx <= (int)nestPos.X + 3; nx++)
        {
            for (int ny = (int)nestPos.Y - 3; ny <= (int)nestPos.Y + 3; ny++)
            {
                if (nx >= 0 && nx < gridWidth && ny >= 0 && ny < gridHeight)
                {
                    var nestCell = _world.Grid.GetCell(nx, ny);
                    nestCell.Type = CellType.Nest;
                    nestCell.ColonyNestId = 1;
                    _world.Grid.SetCell(nx, ny, nestCell);
                }
            }
        }
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
