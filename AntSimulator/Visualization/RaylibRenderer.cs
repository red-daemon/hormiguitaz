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
        Raylib.ClearBackground(new Color(20, 20, 20, 255));

        DrawPheromones();
        DrawGrid();
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
                        Raylib.DrawPixel(x, y, new Color(255, 255, 100, 255));
                        break;
                    case CellType.Nest:
                        Raylib.DrawRectangle(x - 2, y - 2, 5, 5, new Color(255, 50, 50, 255));
                        break;
                    case CellType.Wall:
                        Raylib.DrawPixel(x, y, new Color(100, 100, 100, 255));
                        break;
                }
            }
        }
    }

    private void DrawPheromones()
    {
        for (int x = 0; x < _gridWidth; x++)
        {
            for (int y = 0; y < _gridHeight; y++)
            {
                float pheromone = _world.Pheromones.GetPheromone(x, y, 1, PheromoneType.Food);

                if (pheromone > 0.001f)
                {
                    byte intensity = (byte)(Math.Min(1f, pheromone) * 200);
                    Raylib.DrawPixel(x, y, new Color(intensity, intensity / 3, 0, 128));
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
            int px = (int)pos.X;
            int py = (int)pos.Y;

            if (px >= 0 && px < _gridWidth && py >= 0 && py < _gridHeight)
            {
                Raylib.DrawPixel(px, py, new Color(220, 220, 220, 255));
            }
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
