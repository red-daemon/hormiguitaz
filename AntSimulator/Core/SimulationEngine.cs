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

    public SimulationEngine(int gridWidth, int gridHeight, int antCount, int cellPixelSize = 1)
    {
        _world = new World(gridWidth, gridHeight);
        _renderer = new RaylibRenderer(_world, gridWidth, gridHeight, cellPixelSize);

        InitializeWorld(gridWidth, gridHeight, antCount);
        RegisterSystems();
    }

    private void InitializeWorld(int gridWidth, int gridHeight, int antCount)
    {
        // Create main colony at center
        var nestPos = new Vector2(gridWidth / 2, gridHeight / 2);
        var traits = new ColonyTraits();
        var colony = new Colony(1, nestPos, traits);
        _world.Colonies.Add(1, colony);

        // Mark nest at center first
        for (int nx = (int)nestPos.X - 2; nx <= (int)nestPos.X + 2; nx++)
        {
            for (int ny = (int)nestPos.Y - 2; ny <= (int)nestPos.Y + 2; ny++)
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

        // Create ants INSIDE nest
        for (int i = 0; i < antCount; i++)
        {
            // Spawnear en una celda del nido
            int nestX = (int)nestPos.X + Random.Shared.Next(-2, 3);
            int nestY = (int)nestPos.Y + Random.Shared.Next(-2, 3);
            nestX = Math.Clamp(nestX, (int)nestPos.X - 2, (int)nestPos.X + 2);
            nestY = Math.Clamp(nestY, (int)nestPos.Y - 2, (int)nestPos.Y + 2);

            var spawnPos = new Vector2(nestX, nestY);
            int antId = _world.Ants.CreateAnt(1, spawnPos);

            // Assign random wait time (0-60 ticks)
            var ants = _world.Ants.GetAntsMutable();
            ants[antId].WaitTicksRemaining = Random.Shared.Next(0, 61);
            ants[antId].Orientation = -1;  // Not set until leaves nest

            colony.IncrementPopulation();
        }

        // Add food patches in corners
        int[] foodX = { 15, 85, 15, 85 };
        int[] foodY = { 15, 15, 85, 85 };

        for (int i = 0; i < 4; i++)
        {
            // Create food patches
            for (int fx = foodX[i] - 3; fx <= foodX[i] + 3; fx++)
            {
                for (int fy = foodY[i] - 3; fy <= foodY[i] + 3; fy++)
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
