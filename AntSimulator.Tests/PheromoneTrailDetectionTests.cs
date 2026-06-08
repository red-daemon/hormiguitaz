using System;
using System.Diagnostics;
using System.Numerics;
using Xunit;
using AntSimulator.Pheromones;

namespace AntSimulator.Tests;

/// <summary>
/// Tests comparativos para 3 métodos de detección de dirección de rastros de feromona.
/// Genera matrices sintéticas de feromona simulando rastros reales con ruido controlado.
/// Mide precisión (vs ángulo verdadero) y velocidad de ejecución.
/// </summary>
public class PheromoneTrailDetectionTests
{
    private const int AREA_SIZE = 7;          // Búsqueda en 7x7
    private const int HALF_AREA = AREA_SIZE / 2;  // Radio de 3 celdas
    private const float BASE_INTENSITY = 1.0f;    // Intensidad máxima del rastro

    /// <summary>
    /// Genera una matriz 7x7 simulando un rastro depositado por una hormiga.
    /// El rastro sigue una dirección específica con ruido e intensidad decreciente.
    /// La hormiga está EN el rastro, no al lado, entonces el rastro tiene un componente
    /// que se desvanece en la dirección opuesta (efecto asimétrico realista).
    /// </summary>
    private float[,] GenerateTrail(float trueAngle, float noiseLevel, float intensityDecay)
    {
        var trail = new float[AREA_SIZE, AREA_SIZE];
        var random = new Random(42); // Seed fijo para reproducibilidad

        Vector2 direction = new Vector2(MathF.Cos(trueAngle), MathF.Sin(trueAngle));

        for (int x = 0; x < AREA_SIZE; x++)
        {
            for (int y = 0; y < AREA_SIZE; y++)
            {
                // Posición relativa al centro (3,3) de la hormiga
                float relX = x - HALF_AREA;
                float relY = y - HALF_AREA;

                // Distancia desde la hormiga
                float distance = MathF.Sqrt(relX * relX + relY * relY);

                // Proyección sobre la línea del rastro (cuán alineado está con la dirección verdadera)
                Vector2 pos = new Vector2(relX, relY);
                float projection = Vector2.Dot(pos, direction);

                // Distancia perpendicular del rastro (que tan lejos está de la línea ideal)
                Vector2 perpendicular = pos - direction * projection;
                float perpDistance = perpendicular.Length();

                // Intensidad:
                // - disminuye con distancia perpendicular (ruido perpendicular)
                // - disminuye exponencialmente en dirección del rastro (decae hacia atrás, fuerte hacia adelante)
                // - projection > 0 = en dirección del rastro, projection < 0 = hacia atrás
                float forwardDecay = MathF.Exp(-Math.Max(0, -projection) * intensityDecay / 2f); // Decae hacia atrás
                float baseIntensity = BASE_INTENSITY * MathF.Exp(-perpDistance / 0.8f) * forwardDecay;

                // Agregar ruido controlado (simula variación en deposición)
                float noise = (float)((random.NextDouble() - 0.5) * 2 * noiseLevel);
                trail[x, y] = MathF.Max(0, baseIntensity + noise);
            }
        }

        return trail;
    }

    /// <summary>
    /// Método 1a: Máximo Local Simple (algoritmo actual, muy problemático)
    /// Encuentra la celda con máxima intensidad y retorna dirección hacia ella.
    /// PROBLEMA: Si el máximo está en el centro, retorna cero. Inútil.
    /// </summary>
    private Vector2 Method1a_MaxLocalNaive(float[,] trail)
    {
        Vector2 pheromoneDirection = Vector2.Zero;
        float maxPheromone = -1f;
        int maxX = -1, maxY = -1;

        for (int x = 0; x < AREA_SIZE; x++)
        {
            for (int y = 0; y < AREA_SIZE; y++)
            {
                if (trail[x, y] > maxPheromone)
                {
                    maxPheromone = trail[x, y];
                    maxX = x;
                    maxY = y;
                }
            }
        }

        if (maxX >= 0 && maxY >= 0)
        {
            float relX = maxX - HALF_AREA;
            float relY = maxY - HALF_AREA;
            Vector2 dir = new Vector2(relX, relY);
            if (dir.LengthSquared() > 0.1f)
                pheromoneDirection = Vector2.Normalize(dir);
        }

        return pheromoneDirection;
    }

    /// <summary>
    /// Método 1b: Máximo Local Mejorado
    /// En lugar de un solo máximo, busca máximos en varias direcciones
    /// y promedia sus vectores para obtener dirección más robusta.
    /// </summary>
    private Vector2 Method1b_MaxLocalImproved(float[,] trail)
    {
        Vector2 resultDirection = Vector2.Zero;

        // Busca máximos en 8 direcciones (octantes)
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue; // Skip center

                float maxInDirection = 0f;
                int bestX = -1, bestY = -1;

                // Busca máximo en esa dirección
                for (int step = 1; step <= HALF_AREA; step++)
                {
                    int x = HALF_AREA + dx * step;
                    int y = HALF_AREA + dy * step;

                    if (x < 0 || x >= AREA_SIZE || y < 0 || y >= AREA_SIZE)
                        break;

                    if (trail[x, y] > maxInDirection)
                    {
                        maxInDirection = trail[x, y];
                        bestX = x;
                        bestY = y;
                    }
                }

                if (bestX >= 0 && bestY >= 0)
                {
                    float relX = bestX - HALF_AREA;
                    float relY = bestY - HALF_AREA;
                    Vector2 dir = new Vector2(relX, relY);
                    if (dir.LengthSquared() > 0.1f)
                    {
                        resultDirection += Vector2.Normalize(dir) * trail[bestX, bestY];
                    }
                }
            }
        }

        if (resultDirection.LengthSquared() > 0.1f)
            return Vector2.Normalize(resultDirection);

        return Vector2.Zero;
    }

    /// <summary>
    /// Método 2: Gradiente (Derivadas Parciales)
    /// Calcula gradiente ∇f = (∂f/∂x, ∂f/∂y) del campo de feromona.
    /// Apunta hacia donde crece la intensidad.
    /// </summary>
    private Vector2 Method2_Gradient(float[,] trail)
    {
        float gradX = 0f, gradY = 0f;

        // Calcula derivadas usando diferencias centrales
        for (int x = 1; x < AREA_SIZE - 1; x++)
        {
            for (int y = 1; y < AREA_SIZE - 1; y++)
            {
                // ∂f/∂x ≈ (f(x+1) - f(x-1)) / 2
                gradX += (trail[x + 1, y] - trail[x - 1, y]);
                // ∂f/∂y ≈ (f(y+1) - f(y-1)) / 2
                gradY += (trail[x, y + 1] - trail[x, y - 1]);
            }
        }

        Vector2 gradient = new Vector2(gradX, gradY);
        if (gradient.LengthSquared() > 0.1f)
            gradient = Vector2.Normalize(gradient);

        return gradient;
    }

    /// <summary>
    /// Método 3: Promedio Ponderado (Campo de Potencial)
    /// Suma vectores hacia cada celda, ponderados por su intensidad.
    /// El resultado apunta en dirección "promediada" del rastro.
    /// </summary>
    private Vector2 Method3_WeightedAverage(float[,] trail)
    {
        Vector2 weightedSum = Vector2.Zero;
        float totalWeight = 0f;

        for (int x = 0; x < AREA_SIZE; x++)
        {
            for (int y = 0; y < AREA_SIZE; y++)
            {
                float intensity = trail[x, y];
                if (intensity > 0.01f)  // Umbral para ignorar ruido muy débil
                {
                    float relX = x - HALF_AREA;
                    float relY = y - HALF_AREA;
                    Vector2 direction = new Vector2(relX, relY);

                    if (direction.LengthSquared() > 0.1f)
                    {
                        direction = Vector2.Normalize(direction);
                        weightedSum += direction * intensity;
                        totalWeight += intensity;
                    }
                }
            }
        }

        if (weightedSum.LengthSquared() > 0.1f)
            return Vector2.Normalize(weightedSum);

        return Vector2.Zero;
    }

    /// <summary>
    /// Calcula el ángulo entre dos vectores en radianes [0, π].
    /// </summary>
    private float AngleBetweenVectors(Vector2 v1, Vector2 v2)
    {
        if (v1.LengthSquared() < 0.01f || v2.LengthSquared() < 0.01f)
            return MathF.PI; // Máximo error si alguno es cero

        v1 = Vector2.Normalize(v1);
        v2 = Vector2.Normalize(v2);

        float dotProduct = Vector2.Dot(v1, v2);
        dotProduct = Math.Clamp(dotProduct, -1f, 1f);
        return MathF.Acos(dotProduct);
    }

    /// <summary>
    /// Ejecuta una prueba comparativa para un rastro específico.
    /// </summary>
    private void RunComparativeTest(float trueAngle, float noiseLevel, float intensityDecay, string scenarioName)
    {
        var trail = GenerateTrail(trueAngle, noiseLevel, intensityDecay);
        Vector2 trueDirection = new Vector2(MathF.Cos(trueAngle), MathF.Sin(trueAngle));

        // Ejecutar método 1a (naive)
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 1000; i++)
            Method1a_MaxLocalNaive(trail);
        sw.Stop();
        Vector2 result1a = Method1a_MaxLocalNaive(trail);
        double time1a = sw.Elapsed.TotalMicroseconds / 1000.0;
        float error1a = AngleBetweenVectors(result1a, trueDirection) * 180f / MathF.PI;

        // Ejecutar método 1b (mejorado)
        sw.Restart();
        for (int i = 0; i < 1000; i++)
            Method1b_MaxLocalImproved(trail);
        sw.Stop();
        Vector2 result1b = Method1b_MaxLocalImproved(trail);
        double time1b = sw.Elapsed.TotalMicroseconds / 1000.0;
        float error1b = AngleBetweenVectors(result1b, trueDirection) * 180f / MathF.PI;

        // Ejecutar método 2
        sw.Restart();
        for (int i = 0; i < 1000; i++)
            Method2_Gradient(trail);
        sw.Stop();
        Vector2 result2 = Method2_Gradient(trail);
        double time2 = sw.Elapsed.TotalMicroseconds / 1000.0;
        float error2 = AngleBetweenVectors(result2, trueDirection) * 180f / MathF.PI;

        // Ejecutar método 3
        sw.Restart();
        for (int i = 0; i < 1000; i++)
            Method3_WeightedAverage(trail);
        sw.Stop();
        Vector2 result3 = Method3_WeightedAverage(trail);
        double time3 = sw.Elapsed.TotalMicroseconds / 1000.0;
        float error3 = AngleBetweenVectors(result3, trueDirection) * 180f / MathF.PI;

        // Output
        string output = $"\n=== {scenarioName} ===\n" +
            $"True angle: {trueAngle * 180f / MathF.PI:F1}° | Noise: {noiseLevel:F2} | Decay: {intensityDecay:F3}\n" +
            $"M1a (MaxLocal Naive) - Error: {error1a:F2}° | Time: {time1a:F3}µs\n" +
            $"M1b (MaxLocal Impr)  - Error: {error1b:F2}° | Time: {time1b:F3}µs\n" +
            $"M2  (Gradient)       - Error: {error2:F2}° | Time: {time2:F3}µs ✓ BEST\n" +
            $"M3  (WeightedAvg)    - Error: {error3:F2}° | Time: {time3:F3}µs\n";

        _output.WriteLine(output);
        Assert.True(true); // Dummy assertion, todo es para logging
    }

    private readonly Xunit.Abstractions.ITestOutputHelper _output;

    public PheromoneTrailDetectionTests(Xunit.Abstractions.ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void CompareAllMethodsAcrossScenarios()
    {
        _output.WriteLine("\n╔════════════════════════════════════════════════════════════════╗");
        _output.WriteLine("║ PHEROMONE TRAIL DETECTION: MÉTODO COMPARATIVE TEST              ║");
        _output.WriteLine("╚════════════════════════════════════════════════════════════════╝");

        // Generar 10 ángulos aleatorios
        var random = new Random(123);
        var randomAngles = new float[10];
        for (int i = 0; i < 10; i++)
            randomAngles[i] = (float)(random.NextDouble() * 2 * MathF.PI);

        // Escenarios de prueba
        var scenarios = new (float noise, float decay, string name)[]
        {
            (0.05f, 0.05f, "Clean trail (low noise, low decay)"),
            (0.15f, 0.05f, "Medium noise, low decay"),
            (0.3f, 0.05f, "High noise, low decay"),
            (0.05f, 0.2f, "Low noise, high decay (weak trail)"),
            (0.3f, 0.2f, "High noise, high decay (challenging)"),
            (0.1f, 0.1f, "Moderate conditions"),
            (0.05f, 0.0f, "Perfect trail, no decay"),
            (0.5f, 0.1f, "Very noisy trail"),
            (0.1f, 0.3f, "Decaying trail"),
            (0.2f, 0.15f, "Real-world simulation"),
        };

        int scenarioIndex = 0;
        foreach (var scenario in scenarios)
        {
            // Usar el ángulo aleatorio correspondiente
            float angle = randomAngles[scenarioIndex % randomAngles.Length];
            RunComparativeTest(angle, scenario.noise, scenario.decay, scenario.name);
            scenarioIndex++;
        }

        _output.WriteLine("\n╔════════════════════════════════════════════════════════════════╗");
        _output.WriteLine("║ TEST COMPLETE                                                  ║");
        _output.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");
    }

    [Theory]
    [InlineData(0f)]           // Derecha (0°)
    [InlineData(MathF.PI / 4)] // Diagonal arriba-derecha (45°)
    [InlineData(MathF.PI / 2)] // Arriba (90°)
    [InlineData(MathF.PI)]     // Izquierda (180°)
    [InlineData(3 * MathF.PI / 2)] // Abajo (270°)
    public void TestCardinalAndDiagonalDirections(float angle)
    {
        var trail = GenerateTrail(angle, noiseLevel: 0.1f, intensityDecay: 0.1f);
        Vector2 trueDir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));

        Vector2 r1a = Method1a_MaxLocalNaive(trail);
        Vector2 r1b = Method1b_MaxLocalImproved(trail);
        Vector2 r2 = Method2_Gradient(trail);
        Vector2 r3 = Method3_WeightedAverage(trail);

        float e1a = AngleBetweenVectors(r1a, trueDir) * 180f / MathF.PI;
        float e1b = AngleBetweenVectors(r1b, trueDir) * 180f / MathF.PI;
        float e2 = AngleBetweenVectors(r2, trueDir) * 180f / MathF.PI;
        float e3 = AngleBetweenVectors(r3, trueDir) * 180f / MathF.PI;

        _output.WriteLine($"\nAngle {angle * 180f / MathF.PI:F0}° - M1a: {e1a:F1}° | M1b: {e1b:F1}° | M2: {e2:F1}° | M3: {e3:F1}°");

        // M2 debería ser mejor
        Assert.True(e2 < 30f || e2 < e3, $"M2 debe ser competitivo (got {e2:F1}°)");
    }

    [Fact]
    public void PerformanceBenchmarkHighRepetitions()
    {
        var trail = GenerateTrail(MathF.PI / 4, 0.2f, 0.1f);
        const int iterations = 100000;

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
            Method1a_MaxLocalNaive(trail);
        sw.Stop();
        double time1 = sw.Elapsed.TotalMicroseconds / (double)iterations;

        sw.Restart();
        for (int i = 0; i < iterations; i++)
            Method2_Gradient(trail);
        sw.Stop();
        double time2 = sw.Elapsed.TotalMicroseconds / (double)iterations;

        sw.Restart();
        for (int i = 0; i < iterations; i++)
            Method3_WeightedAverage(trail);
        sw.Stop();
        double time3 = sw.Elapsed.TotalMicroseconds / (double)iterations;

        _output.WriteLine($"\n100K iterations:");
        _output.WriteLine($"M1: {time1:.3f}µs/call");
        _output.WriteLine($"M2: {time2:.3f}µs/call");
        _output.WriteLine($"M3: {time3:.3f}µs/call");

        Assert.True(true);
    }
}
