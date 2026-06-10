using System.Numerics;
using AntSimulator.Core;
using AntSimulator.ECS.Components;
using AntSimulator.Environment;
using AntSimulator.Pheromones;
using Xunit;
using Xunit.Abstractions;

namespace AntSimulator.Tests;

/// <summary>
/// Tests visuales para aislar y debuggear FindPheromoneDirectionByGradient
/// Genera SVGs con heatmaps 7x7 + vectores de gradiente
/// </summary>
public class PheromoneGradientVisualsTests
{
    private readonly ITestOutputHelper _output;
    private const int GRID_SIZE = 50;
    private const int HALF_GRID = GRID_SIZE / 2;
    private const int SEARCH_RADIUS = 3;
    private const int CELL_PIXELS = 30;

    public PheromoneGradientVisualsTests(ITestOutputHelper output)
    {
        _output = output;
        Directory.CreateDirectory("bin/Debug/visualizations");
    }

    [Fact]
    public void GenerateMeshHorizontalLine()
    {
        // Genera y guarda la malla
        var (grid, pheromones, antPos) = SetupMeshHorizontalLine();
        SaveMeshData(pheromones, "mesh_line_horizontal");
    }

    [Fact]
    public void VisualizeMeshHorizontalLine()
    {
        // Visualiza desde datos guardados
        var meshData = LoadMeshData("mesh_line_horizontal");
        VisualizeMeshFromData(meshData, "mesh_line_horizontal", 0f);
    }

    [Fact]
    public void GenerateMeshVerticalLine()
    {
        var (grid, pheromones, antPos) = SetupMeshVerticalLine();
        SaveMeshData(pheromones, "mesh_line_vertical");
    }

    [Fact]
    public void VisualizeMeshVerticalLine()
    {
        var meshData = LoadMeshData("mesh_line_vertical");
        VisualizeMeshFromData(meshData, "mesh_line_vertical", 90f);
    }

    [Fact]
    public void GenerateMeshDiagonalNESO()
    {
        var (grid, pheromones, antPos) = SetupMeshDiagonalNESO();
        SaveMeshData(pheromones, "mesh_line_diagonal_neso");
    }

    [Fact]
    public void VisualizeMeshDiagonalNESO()
    {
        var meshData = LoadMeshData("mesh_line_diagonal_neso");
        VisualizeMeshFromData(meshData, "mesh_line_diagonal_neso", 135f);
    }

    [Fact]
    public void GenerateMeshDiagonalNWSE()
    {
        var (grid, pheromones, antPos) = SetupMeshDiagonalNWSE();
        SaveMeshData(pheromones, "mesh_line_diagonal_nwse");
    }

    [Fact]
    public void VisualizeMeshDiagonalNWSE()
    {
        var meshData = LoadMeshData("mesh_line_diagonal_nwse");
        VisualizeMeshFromData(meshData, "mesh_line_diagonal_nwse", 45f);
    }

    [Fact]
    public void GenerateMeshOblique30()
    {
        var (grid, pheromones, antPos) = SetupMeshOblique30();
        SaveMeshData(pheromones, "mesh_line_oblique_30");
    }

    [Fact]
    public void VisualizeMeshOblique30()
    {
        var meshData = LoadMeshData("mesh_line_oblique_30");
        VisualizeMeshFromData(meshData, "mesh_line_oblique_30", 30f);
    }

    [Fact]
    public void GenerateMeshOblique60()
    {
        var (grid, pheromones, antPos) = SetupMeshOblique60();
        SaveMeshData(pheromones, "mesh_line_oblique_60");
    }

    [Fact]
    public void VisualizeMeshOblique60()
    {
        var meshData = LoadMeshData("mesh_line_oblique_60");
        VisualizeMeshFromData(meshData, "mesh_line_oblique_60", 60f);
    }

    [Fact]
    public void GenerateMeshOblique120()
    {
        var (grid, pheromones, antPos) = SetupMeshOblique120();
        SaveMeshData(pheromones, "mesh_line_oblique_120");
    }

    [Fact]
    public void VisualizeMeshOblique120()
    {
        var meshData = LoadMeshData("mesh_line_oblique_120");
        VisualizeMeshFromData(meshData, "mesh_line_oblique_120", 120f);
    }

    [Fact]
    public void GenerateMeshOblique15()
    {
        var (grid, pheromones, antPos) = SetupMeshOblique15();
        SaveMeshData(pheromones, "mesh_line_oblique_15");
    }

    [Fact]
    public void VisualizeMeshOblique15()
    {
        var meshData = LoadMeshData("mesh_line_oblique_15");
        VisualizeMeshFromData(meshData, "mesh_line_oblique_15", 15f);
    }

    [Fact]
    public void GenerateMeshOblique75()
    {
        var (grid, pheromones, antPos) = SetupMeshOblique75();
        SaveMeshData(pheromones, "mesh_line_oblique_75");
    }

    [Fact]
    public void VisualizeMeshOblique75()
    {
        var meshData = LoadMeshData("mesh_line_oblique_75");
        VisualizeMeshFromData(meshData, "mesh_line_oblique_75", 75f);
    }

    [Fact]
    public void GenerateMeshOblique330()
    {
        var (grid, pheromones, antPos) = SetupMeshOblique330();
        SaveMeshData(pheromones, "mesh_line_oblique_330");
    }

    [Fact]
    public void VisualizeMeshOblique330()
    {
        var meshData = LoadMeshData("mesh_line_oblique_330");
        VisualizeMeshFromData(meshData, "mesh_line_oblique_330", 330f);
    }

    [Fact]
    public void TestGradientHorizontalLine()
    {
        var meshData = LoadMeshData("mesh_line_horizontal");
        var grid = new GridSystem(7, 7);
        var pheromones = new PheromoneGrid(7, 7);
        LoadPheromoneDataFromMesh(pheromones, meshData);

        Vector2 antPos = new Vector2(3, 3);
        var gradient = FindPheromoneDirectionByGradient(antPos, pheromones, grid);
        VisualizeGradientResultFromData(meshData, gradient, Vector2.UnitX, "gradient_line_horizontal");
    }

    [Fact]
    public void TestGradientVerticalLine()
    {
        var meshData = LoadMeshData("mesh_line_vertical");
        var grid = new GridSystem(7, 7);
        var pheromones = new PheromoneGrid(7, 7);
        LoadPheromoneDataFromMesh(pheromones, meshData);

        Vector2 antPos = new Vector2(3, 3);
        var gradient = FindPheromoneDirectionByGradient(antPos, pheromones, grid);
        VisualizeGradientResultFromData(meshData, gradient, Vector2.UnitY, "gradient_line_vertical");
    }

    [Fact]
    public void TestGradientDiagonalNESO()
    {
        var meshData = LoadMeshData("mesh_line_diagonal_neso");
        var grid = new GridSystem(7, 7);
        var pheromones = new PheromoneGrid(7, 7);
        LoadPheromoneDataFromMesh(pheromones, meshData);

        Vector2 antPos = new Vector2(3, 3);
        var gradient = FindPheromoneDirectionByGradient(antPos, pheromones, grid);
        Vector2 expectedDir = Vector2.Normalize(new Vector2(1, 1));
        VisualizeGradientResultFromData(meshData, gradient, expectedDir, "gradient_line_diagonal_neso");
    }

    [Fact]
    public void TestGradientDiagonalNWSE()
    {
        var meshData = LoadMeshData("mesh_line_diagonal_nwse");
        var grid = new GridSystem(7, 7);
        var pheromones = new PheromoneGrid(7, 7);
        LoadPheromoneDataFromMesh(pheromones, meshData);

        Vector2 antPos = new Vector2(3, 3);
        var gradient = FindPheromoneDirectionByGradient(antPos, pheromones, grid);
        Vector2 expectedDir = Vector2.Normalize(new Vector2(-1, 1));
        VisualizeGradientResultFromData(meshData, gradient, expectedDir, "gradient_line_diagonal_nwse");
    }

    [Fact]
    public void TestGradientOblique30()
    {
        var meshData = LoadMeshData("mesh_line_oblique_30");
        var grid = new GridSystem(7, 7);
        var pheromones = new PheromoneGrid(7, 7);
        LoadPheromoneDataFromMesh(pheromones, meshData);

        Vector2 antPos = new Vector2(3, 3);
        var gradient = FindPheromoneDirectionByGradient(antPos, pheromones, grid);
        float angle30 = 30f * MathF.PI / 180f;
        Vector2 expectedDir = new Vector2(MathF.Cos(angle30), MathF.Sin(angle30));
        VisualizeGradientResultFromData(meshData, gradient, expectedDir, "gradient_line_oblique_30");
    }

    [Fact]
    public void TestGradientOblique60()
    {
        var meshData = LoadMeshData("mesh_line_oblique_60");
        var grid = new GridSystem(7, 7);
        var pheromones = new PheromoneGrid(7, 7);
        LoadPheromoneDataFromMesh(pheromones, meshData);

        Vector2 antPos = new Vector2(2, 4);
        var gradient = FindPheromoneDirectionByGradient(antPos, pheromones, grid);
        float angle = 60f * MathF.PI / 180f;
        Vector2 expectedDir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
        VisualizeGradientResultFromData(meshData, gradient, expectedDir, "gradient_line_oblique_60");
    }

    [Fact]
    public void TestGradientOblique120()
    {
        var meshData = LoadMeshData("mesh_line_oblique_120");
        var grid = new GridSystem(7, 7);
        var pheromones = new PheromoneGrid(7, 7);
        LoadPheromoneDataFromMesh(pheromones, meshData);

        Vector2 antPos = new Vector2(4, 2);
        var gradient = FindPheromoneDirectionByGradient(antPos, pheromones, grid);
        float angle = 120f * MathF.PI / 180f;
        Vector2 expectedDir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
        VisualizeGradientResultFromData(meshData, gradient, expectedDir, "gradient_line_oblique_120");
    }

    [Fact]
    public void TestGradientOblique15()
    {
        var meshData = LoadMeshData("mesh_line_oblique_15");
        var grid = new GridSystem(7, 7);
        var pheromones = new PheromoneGrid(7, 7);
        LoadPheromoneDataFromMesh(pheromones, meshData);

        Vector2 antPos = new Vector2(1, 3);
        var gradient = FindPheromoneDirectionByGradient(antPos, pheromones, grid);
        float angle = 15f * MathF.PI / 180f;
        Vector2 expectedDir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
        VisualizeGradientResultFromData(meshData, gradient, expectedDir, "gradient_line_oblique_15");
    }

    [Fact]
    public void TestGradientOblique75()
    {
        var meshData = LoadMeshData("mesh_line_oblique_75");
        var grid = new GridSystem(7, 7);
        var pheromones = new PheromoneGrid(7, 7);
        LoadPheromoneDataFromMesh(pheromones, meshData);

        Vector2 antPos = new Vector2(5, 2);
        var gradient = FindPheromoneDirectionByGradient(antPos, pheromones, grid);
        float angle = 75f * MathF.PI / 180f;
        Vector2 expectedDir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
        VisualizeGradientResultFromData(meshData, gradient, expectedDir, "gradient_line_oblique_75");
    }

    [Fact]
    public void TestGradientOblique330()
    {
        var meshData = LoadMeshData("mesh_line_oblique_330");
        var grid = new GridSystem(7, 7);
        var pheromones = new PheromoneGrid(7, 7);
        LoadPheromoneDataFromMesh(pheromones, meshData);

        Vector2 antPos = new Vector2(4, 5);
        var gradient = FindPheromoneDirectionByGradient(antPos, pheromones, grid);
        float angle = 330f * MathF.PI / 180f;
        Vector2 expectedDir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
        VisualizeGradientResultFromData(meshData, gradient, expectedDir, "gradient_line_oblique_330");
    }

    private (GridSystem grid, PheromoneGrid pheromones, Vector2 antPos) SetupHorizontalTrail()
    {
        var grid = new GridSystem(GRID_SIZE, GRID_SIZE);
        var pheromones = new PheromoneGrid(GRID_SIZE, GRID_SIZE);

        Vector2 antPos = new Vector2(HALF_GRID, HALF_GRID);

        // Rastro horizontal: intensidad crece hacia la derecha
        for (int x = 0; x < GRID_SIZE; x++)
        {
            float intensity = (x - HALF_GRID) / (float)HALF_GRID * 0.5f + 0.5f;
            intensity = Math.Clamp(intensity, 0.1f, 1.0f);

            for (int y = HALF_GRID - 2; y <= HALF_GRID + 2; y++)
            {
                if (y >= 0 && y < GRID_SIZE)
                    pheromones.Deposit(x, y, 1, PheromoneType.Return, intensity);
            }
        }

        return (grid, pheromones, antPos);
    }

    private (GridSystem grid, PheromoneGrid pheromones, Vector2 antPos) SetupVerticalTrail()
    {
        var grid = new GridSystem(GRID_SIZE, GRID_SIZE);
        var pheromones = new PheromoneGrid(GRID_SIZE, GRID_SIZE);

        Vector2 antPos = new Vector2(HALF_GRID, HALF_GRID);

        // Rastro vertical: intensidad crece hacia abajo
        for (int y = 0; y < GRID_SIZE; y++)
        {
            float intensity = (y - HALF_GRID) / (float)HALF_GRID * 0.5f + 0.5f;
            intensity = Math.Clamp(intensity, 0.1f, 1.0f);

            for (int x = HALF_GRID - 2; x <= HALF_GRID + 2; x++)
            {
                if (x >= 0 && x < GRID_SIZE)
                    pheromones.Deposit(x, y, 1, PheromoneType.Return, intensity);
            }
        }

        return (grid, pheromones, antPos);
    }

    private (GridSystem grid, PheromoneGrid pheromones, Vector2 antPos) SetupDiagonalTrail()
    {
        var grid = new GridSystem(GRID_SIZE, GRID_SIZE);
        var pheromones = new PheromoneGrid(GRID_SIZE, GRID_SIZE);

        Vector2 antPos = new Vector2(HALF_GRID, HALF_GRID);

        // Rastro diagonal
        for (int x = 0; x < GRID_SIZE; x++)
        {
            for (int y = 0; y < GRID_SIZE; y++)
            {
                float dist = MathF.Sqrt((x - HALF_GRID) * (x - HALF_GRID) + (y - HALF_GRID) * (y - HALF_GRID));
                if (dist < 15)
                {
                    // Intensidad mayor en diagonal (+45°)
                    float diagonal = ((x - HALF_GRID) + (y - HALF_GRID)) / 20f;
                    float intensity = 0.5f + diagonal * 0.3f;
                    intensity = Math.Clamp(intensity, 0.1f, 1.0f);
                    pheromones.Deposit(x, y, 1, PheromoneType.Return, intensity);
                }
            }
        }

        return (grid, pheromones, antPos);
    }

    private (GridSystem grid, PheromoneGrid pheromones, Vector2 antPos) SetupNoisyTrail()
    {
        var grid = new GridSystem(GRID_SIZE, GRID_SIZE);
        var pheromones = new PheromoneGrid(GRID_SIZE, GRID_SIZE);

        Vector2 antPos = new Vector2(HALF_GRID, HALF_GRID);
        var random = new Random(42); // Seed para reproducibilidad

        // Rastro horizontal con ruido
        for (int x = 0; x < GRID_SIZE; x++)
        {
            for (int y = 0; y < GRID_SIZE; y++)
            {
                float baseIntensity = (x - HALF_GRID) / (float)HALF_GRID * 0.5f + 0.5f;
                float noise = ((float)random.NextDouble() - 0.5f) * 0.3f;
                float intensity = Math.Clamp(baseIntensity + noise, 0.0f, 1.0f);

                if (intensity > 0.05f)
                    pheromones.Deposit(x, y, 1, PheromoneType.Return, intensity);
            }
        }

        return (grid, pheromones, antPos);
    }

    private (GridSystem grid, PheromoneGrid pheromones, Vector2 antPos) SetupBottomStripesNoisy()
    {
        // Grid 7x7 solo
        var grid = new GridSystem(7, 7);
        var pheromones = new PheromoneGrid(7, 7);
        Vector2 antPos = new Vector2(3, 3); // Centro

        var random = new Random(42);

        // Filas 5 y 6 (abajo) con gradiente izquierda→derecha + ruido
        for (int x = 0; x < 7; x++)
        {
            // Fila 6 (más abajo): intensidad crece de izq a der
            float intensity6 = x / 6f * 0.6f + 0.3f;
            intensity6 += ((float)random.NextDouble() - 0.5f) * 0.15f;
            intensity6 = Math.Clamp(intensity6, 0.1f, 1.0f);
            pheromones.Deposit(x, 6, 1, PheromoneType.Return, intensity6);

            // Fila 5: similar pero con más variación
            float intensity5 = x / 6f * 0.5f + 0.2f;
            intensity5 += ((float)random.NextDouble() - 0.5f) * 0.2f;
            intensity5 = Math.Clamp(intensity5, 0.05f, 0.8f);
            pheromones.Deposit(x, 5, 1, PheromoneType.Return, intensity5);
        }

        // Un poco de feromona dispersa en fila 4
        pheromones.Deposit(2, 4, 1, PheromoneType.Return, 0.15f);
        pheromones.Deposit(4, 4, 1, PheromoneType.Return, 0.12f);

        return (grid, pheromones, antPos);
    }

    private (GridSystem grid, PheromoneGrid pheromones, Vector2 antPos) SetupMeshHorizontalLine()
    {
        var grid = new GridSystem(7, 7);
        var pheromones = new PheromoneGrid(7, 7);

        // Línea horizontal recta: y=2, x=0 a 6
        int lineRow = 2;
        float lastValue = 0.5f;

        for (int x = 6; x >= 0; x--)
        {
            float intensity = lastValue * (float)Math.Pow(0.98, 6 - x);
            pheromones.Deposit(x, lineRow, 1, PheromoneType.Return, intensity);

            // Dispersión perpendicular (arriba/abajo)
            if (lineRow > 0)
                pheromones.Deposit(x, lineRow - 1, 1, PheromoneType.Return, intensity * 0.00005f);
            if (lineRow < 6)
                pheromones.Deposit(x, lineRow + 1, 1, PheromoneType.Return, intensity * 0.00005f);
        }

        Vector2 antPos = new Vector2(3, 4);
        return (grid, pheromones, antPos);
    }

    private (GridSystem grid, PheromoneGrid pheromones, Vector2 antPos) SetupMeshVerticalLine()
    {
        var grid = new GridSystem(7, 7);
        var pheromones = new PheromoneGrid(7, 7);

        // Línea vertical recta: x=4, y=0 a 6
        int lineCol = 4;
        float lastValue = 0.5f;

        for (int y = 6; y >= 0; y--)
        {
            float intensity = lastValue * (float)Math.Pow(0.98, 6 - y);
            pheromones.Deposit(lineCol, y, 1, PheromoneType.Return, intensity);

            // Dispersión perpendicular (izq/der)
            if (lineCol > 0)
                pheromones.Deposit(lineCol - 1, y, 1, PheromoneType.Return, intensity * 0.00005f);
            if (lineCol < 6)
                pheromones.Deposit(lineCol + 1, y, 1, PheromoneType.Return, intensity * 0.00005f);
        }

        Vector2 antPos = new Vector2(2, 3);
        return (grid, pheromones, antPos);
    }

    private (GridSystem grid, PheromoneGrid pheromones, Vector2 antPos) SetupMeshDiagonalNESO()
    {
        var grid = new GridSystem(7, 7);
        var pheromones = new PheromoneGrid(7, 7);

        // Línea diagonal recta: (0,6) a (6,0)
        float lastValue = 0.5f;

        for (int i = 6; i >= 0; i--)
        {
            int x = i;
            int y = 6 - i;

            float intensity = lastValue * (float)Math.Pow(0.98, 6 - i);
            pheromones.Deposit(x, y, 1, PheromoneType.Return, intensity);

            // Dispersión perpendicular a la diagonal (NW-SE direction)
            if (x > 0 && y < 6)
                pheromones.Deposit(x - 1, y + 1, 1, PheromoneType.Return, intensity * 0.00005f);
            if (x < 6 && y > 0)
                pheromones.Deposit(x + 1, y - 1, 1, PheromoneType.Return, intensity * 0.00005f);
        }

        Vector2 antPos = new Vector2(3, 4);
        return (grid, pheromones, antPos);
    }

    private (GridSystem grid, PheromoneGrid pheromones, Vector2 antPos) SetupMeshDiagonalNWSE()
    {
        var grid = new GridSystem(7, 7);
        var pheromones = new PheromoneGrid(7, 7);

        // Línea diagonal recta: (0,0) a (6,6)
        float lastValue = 0.5f;

        for (int i = 6; i >= 0; i--)
        {
            int x = i;
            int y = i;

            float intensity = lastValue * (float)Math.Pow(0.98, 6 - i);
            pheromones.Deposit(x, y, 1, PheromoneType.Return, intensity);

            // Dispersión perpendicular a la diagonal (NE-SO direction)
            if (x > 0 && y > 0)
                pheromones.Deposit(x - 1, y - 1, 1, PheromoneType.Return, intensity * 0.00005f);
            if (x < 6 && y < 6)
                pheromones.Deposit(x + 1, y + 1, 1, PheromoneType.Return, intensity * 0.00005f);
        }

        Vector2 antPos = new Vector2(2, 5);
        return (grid, pheromones, antPos);
    }

    private (GridSystem grid, PheromoneGrid pheromones, Vector2 antPos) SetupMeshOblique30()
    {
        var grid = new GridSystem(7, 7);
        var pheromones = new PheromoneGrid(7, 7);

        // Línea oblicua recta a ~30°: (0,0.5) a (6,3.5), redondeado
        float lastValue = 0.5f;
        var points = new[] { (0, 0), (1, 0), (2, 1), (3, 2), (4, 2), (5, 3), (6, 3) };

        for (int idx = points.Length - 1; idx >= 0; idx--)
        {
            var (x, y) = points[idx];
            float intensity = lastValue * (float)Math.Pow(0.98, points.Length - 1 - idx);
            pheromones.Deposit(x, y, 1, PheromoneType.Return, intensity);

            // Dispersión perpendicular
            if (x > 0)
                pheromones.Deposit(x - 1, y, 1, PheromoneType.Return, intensity * 0.00005f);
            if (x < 6)
                pheromones.Deposit(x + 1, y, 1, PheromoneType.Return, intensity * 0.00005f);
            if (y > 0)
                pheromones.Deposit(x, y - 1, 1, PheromoneType.Return, intensity * 0.00005f);
            if (y < 6)
                pheromones.Deposit(x, y + 1, 1, PheromoneType.Return, intensity * 0.00005f);
        }

        Vector2 antPos = new Vector2(4, 2);
        return (grid, pheromones, antPos);
    }

    private (GridSystem grid, PheromoneGrid pheromones, Vector2 antPos) SetupMeshOblique60()
    {
        var grid = new GridSystem(7, 7);
        var pheromones = new PheromoneGrid(7, 7);

        // Línea a 60°: casi vertical (más Y que X)
        float lastValue = 0.5f;
        var points = new[] { (0, 0), (0, 1), (1, 2), (1, 3), (1, 4), (2, 5), (2, 6) };

        for (int idx = points.Length - 1; idx >= 0; idx--)
        {
            var (x, y) = points[idx];
            float intensity = lastValue * (float)Math.Pow(0.98, points.Length - 1 - idx);
            pheromones.Deposit(x, y, 1, PheromoneType.Return, intensity);

            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = x + dx, ny = y + dy;
                    if (nx >= 0 && nx < 7 && ny >= 0 && ny < 7)
                        pheromones.Deposit(nx, ny, 1, PheromoneType.Return, intensity * 0.00005f);
                }
        }

        Vector2 antPos = new Vector2(2, 4);
        return (grid, pheromones, antPos);
    }

    private (GridSystem grid, PheromoneGrid pheromones, Vector2 antPos) SetupMeshOblique120()
    {
        var grid = new GridSystem(7, 7);
        var pheromones = new PheromoneGrid(7, 7);

        // Línea a 120°: diagonal hacia arriba-izquierda
        float lastValue = 0.5f;
        var points = new[] { (6, 0), (5, 1), (4, 2), (3, 2), (2, 3), (1, 4), (0, 5) };

        for (int idx = points.Length - 1; idx >= 0; idx--)
        {
            var (x, y) = points[idx];
            float intensity = lastValue * (float)Math.Pow(0.98, points.Length - 1 - idx);
            pheromones.Deposit(x, y, 1, PheromoneType.Return, intensity);

            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = x + dx, ny = y + dy;
                    if (nx >= 0 && nx < 7 && ny >= 0 && ny < 7)
                        pheromones.Deposit(nx, ny, 1, PheromoneType.Return, intensity * 0.00005f);
                }
        }

        Vector2 antPos = new Vector2(4, 2);
        return (grid, pheromones, antPos);
    }

    private (GridSystem grid, PheromoneGrid pheromones, Vector2 antPos) SetupMeshOblique15()
    {
        var grid = new GridSystem(7, 7);
        var pheromones = new PheromoneGrid(7, 7);

        // Línea a 15°: casi horizontal, levemente arriba
        float lastValue = 0.5f;
        var points = new[] { (0, 2), (1, 2), (2, 1), (3, 1), (4, 0), (5, 0), (6, 0) };

        for (int idx = points.Length - 1; idx >= 0; idx--)
        {
            var (x, y) = points[idx];
            float intensity = lastValue * (float)Math.Pow(0.98, points.Length - 1 - idx);
            pheromones.Deposit(x, y, 1, PheromoneType.Return, intensity);

            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = x + dx, ny = y + dy;
                    if (nx >= 0 && nx < 7 && ny >= 0 && ny < 7)
                        pheromones.Deposit(nx, ny, 1, PheromoneType.Return, intensity * 0.00005f);
                }
        }

        Vector2 antPos = new Vector2(1, 3);
        return (grid, pheromones, antPos);
    }

    private (GridSystem grid, PheromoneGrid pheromones, Vector2 antPos) SetupMeshOblique75()
    {
        var grid = new GridSystem(7, 7);
        var pheromones = new PheromoneGrid(7, 7);

        // Línea a 75°: casi vertical, levemente derecha
        float lastValue = 0.5f;
        var points = new[] { (4, 0), (5, 1), (5, 2), (6, 3), (6, 4), (6, 5), (6, 6) };

        for (int idx = points.Length - 1; idx >= 0; idx--)
        {
            var (x, y) = points[idx];
            float intensity = lastValue * (float)Math.Pow(0.98, points.Length - 1 - idx);
            pheromones.Deposit(x, y, 1, PheromoneType.Return, intensity);

            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = x + dx, ny = y + dy;
                    if (nx >= 0 && nx < 7 && ny >= 0 && ny < 7)
                        pheromones.Deposit(nx, ny, 1, PheromoneType.Return, intensity * 0.00005f);
                }
        }

        Vector2 antPos = new Vector2(5, 2);
        return (grid, pheromones, antPos);
    }

    private (GridSystem grid, PheromoneGrid pheromones, Vector2 antPos) SetupMeshOblique330()
    {
        var grid = new GridSystem(7, 7);
        var pheromones = new PheromoneGrid(7, 7);

        // Línea a 330° (-30°): diagonal hacia abajo-derecha
        float lastValue = 0.5f;
        var points = new[] { (0, 6), (1, 5), (2, 5), (3, 4), (4, 3), (5, 2), (6, 1) };

        for (int idx = points.Length - 1; idx >= 0; idx--)
        {
            var (x, y) = points[idx];
            float intensity = lastValue * (float)Math.Pow(0.98, points.Length - 1 - idx);
            pheromones.Deposit(x, y, 1, PheromoneType.Return, intensity);

            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = x + dx, ny = y + dy;
                    if (nx >= 0 && nx < 7 && ny >= 0 && ny < 7)
                        pheromones.Deposit(nx, ny, 1, PheromoneType.Return, intensity * 0.00005f);
                }
        }

        Vector2 antPos = new Vector2(4, 5);
        return (grid, pheromones, antPos);
    }

    private void VisualizeMesh(GridSystem grid, PheromoneGrid pheromones, Vector2 antPos, string testName)
    {
        var svg = GenerateMeshSvg(grid, pheromones, antPos, testName);
        string filename = $"bin/Debug/visualizations/{testName}.svg";
        File.WriteAllText(filename, svg);
        _output.WriteLine($"✓ Malla guardada: {testName}.svg");
    }

    private void VisualizeGradientResult(GridSystem grid, PheromoneGrid pheromones, Vector2 antPos, Vector2 gradient, Vector2 expectedDir, string testName)
    {
        var svg = GenerateSvg(grid, pheromones, antPos, gradient, expectedDir, testName);
        string filename = $"bin/Debug/visualizations/{testName}.svg";
        File.WriteAllText(filename, svg);

        float errorAngle = GetAngleBetweenVectors(gradient, expectedDir) * 180f / MathF.PI;
        _output.WriteLine($"✓ Gradiente guardado: {testName}.svg");
        _output.WriteLine($"  Calculado: {GetAngleDegrees(gradient):F1}° | Esperado: {GetAngleDegrees(expectedDir):F1}° | Error: {errorAngle:F1}°");
    }

    private string GenerateMeshSvg(GridSystem grid, PheromoneGrid pheromones, Vector2 antPos, string testName)
    {
        int centerX = (int)antPos.X;
        int centerY = (int)antPos.Y;

        // Área visible: 7x7 alrededor de la hormiga
        int minX = centerX - SEARCH_RADIUS;
        int maxX = centerX + SEARCH_RADIUS;
        int minY = centerY - SEARCH_RADIUS;
        int maxY = centerY + SEARCH_RADIUS;

        int svgWidth = (SEARCH_RADIUS * 2 + 1) * CELL_PIXELS + 100;
        int svgHeight = (SEARCH_RADIUS * 2 + 1) * CELL_PIXELS + 100;

        var svg = new System.Text.StringBuilder();
        svg.AppendLine($@"<?xml version=""1.0"" encoding=""UTF-8""?>
<svg width=""{svgWidth}"" height=""{svgHeight}"" xmlns=""http://www.w3.org/2000/svg"">");

        // Fondo
        svg.AppendLine($@"<rect width=""{svgWidth}"" height=""{svgHeight}"" fill=""#f5f5f5""/>");

        // Título
        svg.AppendLine($@"<text x=""20"" y=""30"" font-size=""20"" font-weight=""bold"">{testName}</text>");

        // Grid con heatmap
        int offsetX = 50;
        int offsetY = 60;

        float maxIntensity = 0;
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                if (x >= 0 && x < grid.Width && y >= 0 && y < grid.Height)
                {
                    float intensity = pheromones.GetPheromone(x, y, 1, PheromoneType.Return);
                    maxIntensity = Math.Max(maxIntensity, intensity);
                }
            }
        }

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                if (x >= 0 && x < grid.Width && y >= 0 && y < grid.Height)
                {
                    float intensity = pheromones.GetPheromone(x, y, 1, PheromoneType.Return);
                    float normalized = maxIntensity > 0 ? intensity / maxIntensity : 0;

                    string color = GetHeatmapColor(normalized);
                    int pixelX = offsetX + (x - minX) * CELL_PIXELS;
                    int pixelY = offsetY + (y - minY) * CELL_PIXELS;

                    svg.AppendLine($@"<rect x=""{pixelX}"" y=""{pixelY}"" width=""{CELL_PIXELS}"" height=""{CELL_PIXELS}"" fill=""{color}"" stroke=""#999"" stroke-width=""1""/>");

                    // Intensidad como texto
                    svg.AppendLine($@"<text x=""{pixelX + CELL_PIXELS/2}"" y=""{pixelY + CELL_PIXELS/2 + 5}"" font-size=""10"" text-anchor=""middle"" fill=""#333"">{intensity:F3}</text>");
                }
            }
        }

        // Hormiga (centro)
        int antPixelX = offsetX + SEARCH_RADIUS * CELL_PIXELS + CELL_PIXELS / 2;
        int antPixelY = offsetY + SEARCH_RADIUS * CELL_PIXELS + CELL_PIXELS / 2;
        svg.AppendLine($@"<circle cx=""{antPixelX}"" cy=""{antPixelY}"" r=""8"" fill=""#ff6600"" stroke=""#000"" stroke-width=""2""/>");

        svg.AppendLine("</svg>");
        return svg.ToString();
    }

    private string GenerateSvg(GridSystem grid, PheromoneGrid pheromones, Vector2 antPos, Vector2 gradient, Vector2 expectedDir, string testName)
    {
        int centerX = (int)antPos.X;
        int centerY = (int)antPos.Y;

        // Área visible: 7x7 alrededor de la hormiga
        int minX = centerX - SEARCH_RADIUS;
        int maxX = centerX + SEARCH_RADIUS;
        int minY = centerY - SEARCH_RADIUS;
        int maxY = centerY + SEARCH_RADIUS;

        int svgWidth = (SEARCH_RADIUS * 2 + 1) * CELL_PIXELS + 100;
        int svgHeight = (SEARCH_RADIUS * 2 + 1) * CELL_PIXELS + 150;

        var svg = new System.Text.StringBuilder();
        svg.AppendLine($@"<?xml version=""1.0"" encoding=""UTF-8""?>
<svg width=""{svgWidth}"" height=""{svgHeight}"" xmlns=""http://www.w3.org/2000/svg"">");

        // Fondo
        svg.AppendLine($@"<rect width=""{svgWidth}"" height=""{svgHeight}"" fill=""#f5f5f5""/>");

        // Título
        svg.AppendLine($@"<text x=""20"" y=""30"" font-size=""20"" font-weight=""bold"">{testName}</text>");

        // Grid con heatmap
        int offsetX = 50;
        int offsetY = 60;

        float maxIntensity = 0;
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                if (x >= 0 && x < grid.Width && y >= 0 && y < grid.Height)
                {
                    float intensity = pheromones.GetPheromone(x, y, 1, PheromoneType.Return);
                    maxIntensity = Math.Max(maxIntensity, intensity);
                }
            }
        }

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                if (x >= 0 && x < grid.Width && y >= 0 && y < grid.Height)
                {
                    float intensity = pheromones.GetPheromone(x, y, 1, PheromoneType.Return);
                    float normalized = maxIntensity > 0 ? intensity / maxIntensity : 0;

                    string color = GetHeatmapColor(normalized);
                    int pixelX = offsetX + (x - minX) * CELL_PIXELS;
                    int pixelY = offsetY + (y - minY) * CELL_PIXELS;

                    svg.AppendLine($@"<rect x=""{pixelX}"" y=""{pixelY}"" width=""{CELL_PIXELS}"" height=""{CELL_PIXELS}"" fill=""{color}"" stroke=""#999"" stroke-width=""1""/>");

                    // Intensidad como texto
                    svg.AppendLine($@"<text x=""{pixelX + CELL_PIXELS/2}"" y=""{pixelY + CELL_PIXELS/2 + 5}"" font-size=""10"" text-anchor=""middle"" fill=""#333"">{normalized:F2}</text>");
                }
            }
        }

        // Hormiga (centro)
        int antPixelX = offsetX + SEARCH_RADIUS * CELL_PIXELS + CELL_PIXELS / 2;
        int antPixelY = offsetY + SEARCH_RADIUS * CELL_PIXELS + CELL_PIXELS / 2;
        svg.AppendLine($@"<circle cx=""{antPixelX}"" cy=""{antPixelY}"" r=""8"" fill=""#ff6600"" stroke=""#000"" stroke-width=""2""/>");

        // Vector gradiente (rojo)
        DrawVector(svg, antPixelX, antPixelY, gradient, 50, "#ff0000", "Gradiente");

        // Vector esperado (verde)
        DrawVector(svg, antPixelX, antPixelY, expectedDir, 50, "#00cc00", "Esperado");

        // Leyenda
        int legendY = offsetY + (SEARCH_RADIUS * 2 + 2) * CELL_PIXELS + 20;
        svg.AppendLine($@"<line x1=""50"" y1=""{legendY}"" x2=""100"" y2=""{legendY}"" stroke=""#ff0000"" stroke-width=""3""/>");
        svg.AppendLine($@"<text x=""110"" y=""{legendY + 5}"" font-size=""12"">Gradiente: {GetAngleDegrees(gradient):F1}°</text>");

        svg.AppendLine($@"<line x1=""50"" y1=""{legendY + 25}"" x2=""100"" y2=""{legendY + 25}"" stroke=""#00cc00"" stroke-width=""3""/>");
        svg.AppendLine($@"<text x=""110"" y=""{legendY + 30}"" font-size=""12"">Esperado: {GetAngleDegrees(expectedDir):F1}°</text>");

        svg.AppendLine("</svg>");
        return svg.ToString();
    }

    private void DrawVector(System.Text.StringBuilder svg, int startX, int startY, Vector2 direction, int length, string color, string label)
    {
        if (direction.LengthSquared() < 0.01f) return;

        Vector2 normalized = Vector2.Normalize(direction);
        int endX = startX + (int)(normalized.X * length);
        int endY = startY + (int)(normalized.Y * length);

        // Flecha
        svg.AppendLine($@"<line x1=""{startX}"" y1=""{startY}"" x2=""{endX}"" y2=""{endY}"" stroke=""{color}"" stroke-width=""3"" marker-end=""url(#{color}Arrow)""/>");
    }

    private string GetHeatmapColor(float normalized)
    {
        // Azul (bajo) → Rojo (alto)
        if (normalized < 0.33f)
            return $"rgb({(int)(normalized * 3 * 255)}, 100, {(int)(255 - normalized * 3 * 100)})"; // Azul→Cyan
        else if (normalized < 0.66f)
            return $"rgb({(int)((normalized - 0.33f) * 3 * 255)}, {(int)(200 - (normalized - 0.33f) * 3 * 100)}, 50)"; // Cyan→Amarillo
        else
            return $"rgb(255, {(int)(200 - (normalized - 0.66f) * 3 * 100)}, 0)"; // Amarillo→Rojo
    }

    private float GetAngleDegrees(Vector2 vector)
    {
        return MathF.Atan2(vector.Y, vector.X) * 180f / MathF.PI;
    }

    private float GetAngleBetweenVectors(Vector2 v1, Vector2 v2)
    {
        if (v1.LengthSquared() < 0.01f || v2.LengthSquared() < 0.01f)
            return MathF.PI;

        v1 = Vector2.Normalize(v1);
        v2 = Vector2.Normalize(v2);

        float dotProduct = Vector2.Dot(v1, v2);
        dotProduct = Math.Clamp(dotProduct, -1f, 1f);
        return MathF.Acos(dotProduct);
    }

    private Dictionary<(int x, int y), float> LoadMeshData(string meshName)
    {
        string filename = $"bin/Debug/visualizations/{meshName}.txt";
        var data = new Dictionary<(int, int), float>();

        if (!File.Exists(filename))
            return data;

        foreach (var line in File.ReadLines(filename))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            // Formato: (x,y) = 0.5
            var parts = line.Split('=');
            if (parts.Length != 2) continue;

            var coords = parts[0].Trim().Trim('(', ')').Split(',');
            if (coords.Length != 2) continue;

            if (int.TryParse(coords[0].Trim(), out int x) &&
                int.TryParse(coords[1].Trim(), out int y) &&
                float.TryParse(parts[1].Trim(), out float value))
            {
                data[(x, y)] = value;
            }
        }

        return data;
    }

    private void SaveMeshData(PheromoneGrid pheromones, string meshName)
    {
        var lines = new List<string>();
        for (int x = 0; x < 7; x++)
        {
            for (int y = 0; y < 7; y++)
            {
                float value = pheromones.GetPheromone(x, y, 1, PheromoneType.Return);
                if (value > 0.00001f)
                    lines.Add($"({x},{y}) = {value:F6}");
            }
        }

        string filename = $"bin/Debug/visualizations/{meshName}.txt";
        File.WriteAllLines(filename, lines);
        _output.WriteLine($"✓ Malla guardada: {meshName}.txt ({lines.Count} casillas)");
    }

    private void VisualizeMeshFromData(Dictionary<(int x, int y), float> meshData, string testName, float? expectedAngleDegrees = null)
    {
        var svg = GenerateMeshSvgFromData(meshData, testName, expectedAngleDegrees);
        string filename = $"bin/Debug/visualizations/{testName}.svg";
        File.WriteAllText(filename, svg);
        _output.WriteLine($"✓ Visualización guardada: {testName}.svg");
    }

    private void LoadPheromoneDataFromMesh(PheromoneGrid pheromones, Dictionary<(int x, int y), float> meshData)
    {
        foreach (var ((x, y), value) in meshData)
        {
            pheromones.Deposit(x, y, 1, PheromoneType.Return, value);
        }
    }

    private void VisualizeGradientResultFromData(Dictionary<(int x, int y), float> meshData, Vector2 gradient, Vector2 expectedDir, string testName)
    {
        var svg = GenerateSvgFromData(meshData, gradient, expectedDir, testName);
        string filename = $"bin/Debug/visualizations/{testName}.svg";
        File.WriteAllText(filename, svg);

        float errorAngle = GetAngleBetweenVectors(gradient, expectedDir) * 180f / MathF.PI;
        _output.WriteLine($"✓ Gradiente guardado: {testName}.svg");
        _output.WriteLine($"  Calculado: {GetAngleDegrees(gradient):F1}° | Esperado: {GetAngleDegrees(expectedDir):F1}° | Error: {errorAngle:F1}°");
    }

    private string GenerateSvgFromData(Dictionary<(int x, int y), float> meshData, Vector2 gradient, Vector2 expectedDir, string testName)
    {
        int minX = 0, maxX = 6, minY = 0, maxY = 6;
        int svgWidth = (maxX - minX + 1) * CELL_PIXELS + 100;
        int svgHeight = (maxY - minY + 1) * CELL_PIXELS + 150;

        var svg = new System.Text.StringBuilder();
        svg.AppendLine($@"<?xml version=""1.0"" encoding=""UTF-8""?>
<svg width=""{svgWidth}"" height=""{svgHeight}"" xmlns=""http://www.w3.org/2000/svg"">");

        svg.AppendLine($@"<rect width=""{svgWidth}"" height=""{svgHeight}"" fill=""#f5f5f5""/>");
        svg.AppendLine($@"<text x=""20"" y=""30"" font-size=""20"" font-weight=""bold"">{testName}</text>");

        int offsetX = 50;
        int offsetY = 60;

        float maxIntensity = meshData.Values.Max();

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                float intensity = meshData.ContainsKey((x, y)) ? meshData[(x, y)] : 0f;
                float normalized = maxIntensity > 0 ? intensity / maxIntensity : 0;

                string color = GetHeatmapColor(normalized);
                int pixelX = offsetX + (x - minX) * CELL_PIXELS;
                int pixelY = offsetY + (y - minY) * CELL_PIXELS;

                svg.AppendLine($@"<rect x=""{pixelX}"" y=""{pixelY}"" width=""{CELL_PIXELS}"" height=""{CELL_PIXELS}"" fill=""{color}"" stroke=""#999"" stroke-width=""1""/>");
                svg.AppendLine($@"<text x=""{pixelX + CELL_PIXELS/2}"" y=""{pixelY + CELL_PIXELS/2 + 5}"" font-size=""9"" text-anchor=""middle"" fill=""#333"">{intensity:F3}</text>");
            }
        }

        // Hormiga en centro (3, 3)
        int antPixelX = offsetX + 3 * CELL_PIXELS + CELL_PIXELS / 2;
        int antPixelY = offsetY + 3 * CELL_PIXELS + CELL_PIXELS / 2;
        svg.AppendLine($@"<circle cx=""{antPixelX}"" cy=""{antPixelY}"" r=""8"" fill=""#ff6600"" stroke=""#000"" stroke-width=""2""/>");

        // Vector gradiente (rojo)
        DrawVector(svg, antPixelX, antPixelY, gradient, 50, "#ff0000", "Gradiente");
        // Vector esperado (verde)
        DrawVector(svg, antPixelX, antPixelY, expectedDir, 50, "#00cc00", "Esperado");

        // Leyenda
        int legendY = offsetY + (SEARCH_RADIUS * 2 + 2) * CELL_PIXELS + 20;
        svg.AppendLine($@"<line x1=""50"" y1=""{legendY}"" x2=""100"" y2=""{legendY}"" stroke=""#ff0000"" stroke-width=""3""/>");
        svg.AppendLine($@"<text x=""110"" y=""{legendY + 5}"" font-size=""12"">Gradiente: {GetAngleDegrees(gradient):F1}°</text>");

        svg.AppendLine($@"<line x1=""50"" y1=""{legendY + 25}"" x2=""100"" y2=""{legendY + 25}"" stroke=""#00cc00"" stroke-width=""3""/>");
        svg.AppendLine($@"<text x=""110"" y=""{legendY + 30}"" font-size=""12"">Esperado: {GetAngleDegrees(expectedDir):F1}°</text>");

        svg.AppendLine("</svg>");
        return svg.ToString();
    }

    private string GenerateMeshSvgFromData(Dictionary<(int x, int y), float> meshData, string testName, float? expectedAngleDegrees = null)
    {
        int minX = 0, maxX = 6, minY = 0, maxY = 6;

        int svgWidth = (maxX - minX + 1) * CELL_PIXELS + 100;
        int svgHeight = (maxY - minY + 1) * CELL_PIXELS + 150;

        var svg = new System.Text.StringBuilder();
        svg.AppendLine($@"<?xml version=""1.0"" encoding=""UTF-8""?>
<svg width=""{svgWidth}"" height=""{svgHeight}"" xmlns=""http://www.w3.org/2000/svg"">");

        svg.AppendLine($@"<rect width=""{svgWidth}"" height=""{svgHeight}"" fill=""#f5f5f5""/>");
        svg.AppendLine($@"<text x=""20"" y=""30"" font-size=""20"" font-weight=""bold"">{testName}</text>");

        int offsetX = 50;
        int offsetY = 60;

        float maxIntensity = meshData.Values.Max();

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                float intensity = meshData.ContainsKey((x, y)) ? meshData[(x, y)] : 0f;
                float normalized = maxIntensity > 0 ? intensity / maxIntensity : 0;

                string color = GetHeatmapColor(normalized);
                int pixelX = offsetX + (x - minX) * CELL_PIXELS;
                int pixelY = offsetY + (y - minY) * CELL_PIXELS;

                svg.AppendLine($@"<rect x=""{pixelX}"" y=""{pixelY}"" width=""{CELL_PIXELS}"" height=""{CELL_PIXELS}"" fill=""{color}"" stroke=""#999"" stroke-width=""1""/>");
                svg.AppendLine($@"<text x=""{pixelX + CELL_PIXELS/2}"" y=""{pixelY + CELL_PIXELS/2 + 5}"" font-size=""9"" text-anchor=""middle"" fill=""#333"">{intensity:F3}</text>");
            }
        }

        // Dibujar línea de orientación esperada (si se proporciona)
        if (expectedAngleDegrees.HasValue)
        {
            float angleRad = expectedAngleDegrees.Value * MathF.PI / 180f;
            Vector2 direction = new Vector2(MathF.Cos(angleRad), MathF.Sin(angleRad));

            // Calcular puntos de inicio y fin de la línea
            int centerPixelX = offsetX + 3 * CELL_PIXELS + CELL_PIXELS / 2;
            int centerPixelY = offsetY + 3 * CELL_PIXELS + CELL_PIXELS / 2;

            // Extender 50 píxeles en ambas direcciones
            int x1 = (int)(centerPixelX - direction.X * 50);
            int y1 = (int)(centerPixelY - direction.Y * 50);
            int x2 = (int)(centerPixelX + direction.X * 50);
            int y2 = (int)(centerPixelY + direction.Y * 50);

            svg.AppendLine($@"<line x1=""{x1}"" y1=""{y1}"" x2=""{x2}"" y2=""{y2}"" stroke=""#00ff00"" stroke-width=""3"" stroke-dasharray=""5,5"" opacity=""0.9""/>");
        }

        // Hormiga en centro (3, 3)
        int antPixelX = offsetX + 3 * CELL_PIXELS + CELL_PIXELS / 2;
        int antPixelY = offsetY + 3 * CELL_PIXELS + CELL_PIXELS / 2;
        svg.AppendLine($@"<circle cx=""{antPixelX}"" cy=""{antPixelY}"" r=""8"" fill=""#ff6600"" stroke=""#000"" stroke-width=""2""/>");

        // Leyenda
        if (expectedAngleDegrees.HasValue)
        {
            int legendY = offsetY + (SEARCH_RADIUS * 2 + 2) * CELL_PIXELS + 20;
            svg.AppendLine($@"<line x1=""50"" y1=""{legendY}"" x2=""100"" y2=""{legendY}"" stroke=""#00ff00"" stroke-width=""3"" stroke-dasharray=""5,5""/>");
            svg.AppendLine($@"<text x=""110"" y=""{legendY + 5}"" font-size=""12"">Orientación: {expectedAngleDegrees.Value:F1}°</text>");
        }

        svg.AppendLine("</svg>");
        return svg.ToString();
    }

    private Vector2 FindPheromoneDirectionByGradient(Vector2 position, PheromoneGrid pheromones, GridSystem grid)
    {
        float gradX = 0f, gradY = 0f;
        const int searchRadius = SEARCH_RADIUS;

        for (int x = -searchRadius; x <= searchRadius; x++)
        {
            for (int y = -searchRadius; y <= searchRadius; y++)
            {
                int gx = (int)position.X + x;
                int gy = (int)position.Y + y;

                if (gx < 0 || gx >= grid.Width || gy < 0 || gy >= grid.Height)
                    continue;

                float fRight = pheromones.GetPheromone(Math.Min(gx + 1, grid.Width - 1), gy, 1, PheromoneType.Return);
                float fLeft = pheromones.GetPheromone(Math.Max(gx - 1, 0), gy, 1, PheromoneType.Return);
                gradX += (fRight - fLeft);

                float fDown = pheromones.GetPheromone(gx, Math.Min(gy + 1, grid.Height - 1), 1, PheromoneType.Return);
                float fUp = pheromones.GetPheromone(gx, Math.Max(gy - 1, 0), 1, PheromoneType.Return);
                gradY += (fDown - fUp);
            }
        }

        Vector2 gradient = new Vector2(gradX, gradY);
        if (gradient.LengthSquared() > 0.1f)
            return Vector2.Normalize(gradient);

        return Vector2.Zero;
    }
}
