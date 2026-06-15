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
    private int _cellPixelSize;
    private int _screenWidth;
    private int _screenHeight;

    public RaylibRenderer(World world, int gridWidth, int gridHeight, int cellPixelSize = 1)
    {
        _world = world;
        _gridWidth = gridWidth;
        _gridHeight = gridHeight;
        _cellPixelSize = cellPixelSize;
        _screenWidth = gridWidth * cellPixelSize;
        _screenHeight = gridHeight * cellPixelSize;

        Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
        Raylib.InitWindow(_screenWidth, _screenHeight, "Ant Simulator - Phase 1 MVP");
        Raylib.SetTargetFPS(60);
    }

    public void SetWorld(World world)
    {
        _world = world;
    }

    public void Render()
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(new Color(33, 33, 43, 255));

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
                int px = x * _cellPixelSize;
                int py = y * _cellPixelSize;

                switch (cell.Type)
                {
                    case CellType.Food:
                        Raylib.DrawRectangle(px, py, _cellPixelSize, _cellPixelSize, new Color(255, 255, 100, 255));
                        break;
                    case CellType.Nest:
                        Raylib.DrawRectangle(px, py, _cellPixelSize, _cellPixelSize, new Color(255, 50, 50, 255));
                        break;
                    case CellType.Wall:
                        Raylib.DrawRectangle(px, py, _cellPixelSize, _cellPixelSize, new Color(100, 100, 100, 255));
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
                int px = x * _cellPixelSize;
                int py = y * _cellPixelSize;

                // Dibujar feromona de exploración (NARANJA)
                float explorePheromone = _world.Pheromones.GetPheromone(x, y, 1, PheromoneType.Explore);
                if (explorePheromone > 0f)
                {
                    float normalizedValue = explorePheromone / Constants.PHEROMONE_COLOR_SATURATION;
                    int intensity = (int)(Math.Clamp(normalizedValue, 0f, 1f) * 255);
                    Raylib.DrawRectangle(px, py, _cellPixelSize, _cellPixelSize, new Color(255, 165, 0, intensity));
                }

                // Dibujar feromona de retorno (VERDE)
                float returnPheromone = _world.Pheromones.GetPheromone(x, y, 1, PheromoneType.Return);
                if (returnPheromone > 0f)
                {
                    float normalizedValue = returnPheromone / Constants.PHEROMONE_COLOR_SATURATION;
                    int intensity = (int)(Math.Clamp(normalizedValue, 0f, 1f) * 255);
                    Raylib.DrawRectangle(px, py, _cellPixelSize, _cellPixelSize, new Color(100, 255, 100, intensity));
                }

                // Dibujar feromona de alerta (ROJO)
                float alertPheromone = _world.Pheromones.GetPheromone(x, y, 1, PheromoneType.Alert);
                if (alertPheromone > 0f)
                {
                    float normalizedValue = alertPheromone / Constants.PHEROMONE_COLOR_SATURATION;
                    int intensity = (int)(Math.Clamp(normalizedValue, 0f, 1f) * 255);
                    Raylib.DrawRectangle(px, py, _cellPixelSize, _cellPixelSize, new Color(255, 0, 0, intensity));
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
            int cellX = (int)pos.X;
            int cellY = (int)pos.Y;

            if (cellX >= 0 && cellX < _gridWidth && cellY >= 0 && cellY < _gridHeight)
            {
                float centerX = cellX * _cellPixelSize + _cellPixelSize / 2;
                float centerY = cellY * _cellPixelSize + _cellPixelSize / 2;

                // Draw ant based on whether it has orientation (has exited nest)
                if (ants[i].Orientation >= 0)
                {
                    // Ant has orientation - draw as rotated rectangle (white oval)
                    float width = 6f;
                    float height = 3f;
                    float rotation = ants[i].Orientation * 180f / (float)Math.PI;  // Convert to degrees

                    Raylib.DrawRectanglePro(
                        new Rectangle(centerX, centerY, width, height),
                        new Vector2(width / 2, height / 2),
                        rotation,
                        new Color(220, 220, 220, 255)
                    );
                }
                else
                {
                    // Ant waiting in nest - draw as larger gray circle
                    Raylib.DrawCircle((int)centerX, (int)centerY, 3, new Color(120, 120, 120, 255));
                }
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
