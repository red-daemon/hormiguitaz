using System.Numerics;
using AntSimulator.Agents;
using AntSimulator.Environment;
using AntSimulator.Pheromones;
using Raylib_CsLo;

namespace AntSimulator.Visualization;

public class RaylibRenderer : IRenderer
{
    private World _world;
    private int _gridWidth;
    private int _gridHeight;
    private int _screenWidth;
    private int _screenHeight;

    public RaylibRenderer(World world, int gridWidth, int gridHeight)
    {
        _world = world;
        _gridWidth = gridWidth;
        _gridHeight = gridHeight;
        _screenWidth = gridWidth;
        _screenHeight = gridHeight;

        Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
        Raylib.InitWindow(_screenWidth, _screenHeight, "Ant Simulator - Phase 1 MVP");
        Raylib.SetTargetFPS(60);
    }

    public void Render()
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(new Color(0, 0, 0, 255));

        DrawGrid();
        DrawPheromones();
        DrawAnts();
        DrawUI();

        Raylib.EndDrawing();
    }

    private void DrawGrid()
    {
        for (int x = 0; x < _gridWidth; x++)
        {
            for (int y = 0; y < _gridHeight; y++)
            {
                var cell = _world.Grid.GetCell(x, y);

                switch (cell.Type)
                {
                    case CellType.Food:
                        Raylib.DrawPixel(x, y, new Color(255, 255, 0, 255));
                        break;
                    case CellType.Nest:
                        Raylib.DrawPixel(x, y, new Color(255, 0, 0, 255));
                        break;
                    case CellType.Wall:
                        Raylib.DrawPixel(x, y, new Color(64, 64, 64, 255));
                        break;
                }
            }
        }
    }

    private void DrawPheromones()
    {
        var foodPheromone = new float[_gridWidth, _gridHeight];

        // Sample pheromone values (simplified)
        for (int x = 0; x < _gridWidth; x++)
        {
            for (int y = 0; y < _gridHeight; y++)
            {
                float pheromone = _world.Pheromones.GetPheromone(x, y, 1, PheromoneType.Food);
                foodPheromone[x, y] = pheromone;

                if (pheromone > 0.01f)
                {
                    byte intensity = (byte)(Math.Min(1f, pheromone) * 100);
                    Raylib.DrawPixel(x, y, new Color(intensity, intensity / 2, 0, 255));
                }
            }
        }
    }

    private void DrawAnts()
    {
        var positions = _world.Ants.GetPositions();
        var ants = _world.Ants.GetAnts();

        for (int i = 0; i < _world.Ants.EntityCount; i++)
        {
            if (ants[i].State == AntState.Dead) continue;

            var pos = positions[i];
            Raylib.DrawPixel((int)pos.X, (int)pos.Y, new Color(255, 255, 255, 255));
        }
    }

    private void DrawUI()
    {
        var textColor = new Color(255, 255, 255, 255);
        Raylib.DrawText($"Tick: {_world.CurrentTick}", 10, 10, 20, textColor);
        Raylib.DrawText($"Ants: {_world.Ants.EntityCount}", 10, 40, 20, textColor);

        if (_world.Colonies.TryGetValue(1, out var colony))
        {
            Raylib.DrawText($"Population: {colony.PopulationCount}", 10, 70, 20, textColor);
            Raylib.DrawText($"Nest: ({(int)colony.NestPosition.X}, {(int)colony.NestPosition.Y})", 10, 100, 20, textColor);
        }
    }
}
