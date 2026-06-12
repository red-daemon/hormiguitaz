using System;
using System.Collections.Generic;
using System.Numerics;
using System.IO;
using AntSimulator.Colonies;
using AntSimulator.Core;
using AntSimulator.ECS.Components;
using AntSimulator.Environment;
using AntSimulator.Pheromones;

namespace AntSimulator.Agents.Roles;

/// <summary>
/// Estrategia de comportamiento para hormigas obreras.
/// Máquina de estados: EXPLORING → WORKING → RETURNING → IDLE
/// </summary>
public class WorkerRole : IRoleStrategy
{
    public RoleDecision DecideAction(
        int id,
        Vector2 position,
        AntComponent ant,
        GridSystem grid,
        PheromoneGrid pheromones,
        ColonyTraits traits,
        Vector2 nestPosition)
    {
        Vector2 velocity = Vector2.Zero;
        AntState? newState = null;
        float newOrientation = ant.Orientation;
        bool? newHasFood = null;
        var currentCell = grid.GetCell((int)position.X, (int)position.Y);

        // FASE 1: Esperando en el nido (IDLE)
        if (currentCell.Type == CellType.Nest && ant.WaitTicksRemaining > 0)
        {
            velocity = Vector2.Zero;
            return new RoleDecision { Action = new AntAction { Velocity = velocity }, NewState = newState, NewOrientation = null };
        }

        // FASE 2: Saliendo del nido
        if (currentCell.Type == CellType.Nest && ant.WaitTicksRemaining == 0 && ant.Orientation < 0)
        {
            newOrientation = (float)(Random.Shared.NextDouble() * Math.PI * 2);
            Vector2 direction = new Vector2(MathF.Cos(newOrientation), MathF.Sin(newOrientation));
            Vector2 destPos = nestPosition;

            for (int step = 1; step < Math.Max(grid.Width, grid.Height); step++)
            {
                Vector2 searchPos = nestPosition + direction * step;
                int checkX = (int)searchPos.X;
                int checkY = (int)searchPos.Y;

                if (checkX < 0 || checkX >= grid.Width || checkY < 0 || checkY >= grid.Height)
                    break;

                var checkCell = grid.GetCell(checkX, checkY);
                if (checkCell.Type != CellType.Nest)
                {
                    destPos = new Vector2(checkX, checkY);
                    break;
                }
            }

            if (destPos != nestPosition)
            {
                // return new RoleDecision
                // {
                //     Action = new AntAction { Velocity = Vector2.Zero },
                //     NewOrientation = (float)5.4977871438,
                //     NewPosition = new Vector2(53, 47),
                //     NewState = AntState.Exploring
                // };
                // Para pruebas. Descomentar cuando este listo.
                return new RoleDecision
                {
                    Action = new AntAction { Velocity = Vector2.Zero },
                    NewOrientation = newOrientation,
                    NewPosition = destPos,
                    NewState = AntState.Exploring
                };
            }
        }

        // FASE 3: Movimiento base (orientación)
        if (newOrientation >= 0)
        {
            velocity = new Vector2(
                MathF.Cos(newOrientation),
                MathF.Sin(newOrientation)
            ) * traits.Speed;

            float deltaRotation = (float)((Random.Shared.NextDouble() - 0.5) * 2 * Math.PI / 180);
            newOrientation = MathUtils.NormalizeAngle(newOrientation + deltaRotation);
        }

        // === ESTADO: EXPLORING ===
        if (ant.State == AntState.Exploring && currentCell.Type != CellType.Nest)
        {
            // Si encuentra comida real → RETURNING (primero en crear puente)
            if (currentCell.Type == CellType.Food)
            {
                newState = AntState.Returning;
                newHasFood = true;
                // Cambio brusco de 180° cuando encuentra comida
                newOrientation = MathUtils.NormalizeAngle(ant.Orientation + MathF.PI);
                velocity = Vector2.Zero;
            }
            // Busca rastro RETURN con prioridad absoluta
            else if (Constants.PHEROMONES_ENABLED)
            {
                Vector2 returnDirection = FindPheromoneDirectionByGradient(position, pheromones, ant.ColonyId, PheromoneType.Return, grid, ant.Orientation);
                if (returnDirection.LengthSquared() > 0.1f && CheckPheromoneThreshold(position, pheromones, ant.ColonyId, PheromoneType.Return, grid, 0.05f))
                {
                    newState = AntState.Working;
                    newHasFood = false;
                    float targetAngle = MathF.Atan2(returnDirection.Y, returnDirection.X);
                    newOrientation = SmoothOrientation(ant.Orientation, targetAngle);
                    velocity = new Vector2(MathF.Cos(newOrientation), MathF.Sin(newOrientation)) * traits.Speed;
                }
                // Si no hay RETURN → usa FASE 3 (movimiento serpenteante)
            }
        }

        // === ESTADO: WORKING (sin comida) - siguiendo RETURN inverso hacia comida ===
        else if (ant.State == AntState.Working && !ant.HasFood && Constants.PHEROMONES_ENABLED)
        {
            Vector2 returnDirection = FindPheromoneDirectionByGradient(position, pheromones, ant.ColonyId, PheromoneType.Return, grid, ant.Orientation);

            if (returnDirection.LengthSquared() > 0.1f)
            {
                // Sigue RETURN en dirección INVERSA +180° (porque RETURN va nido→comida)
                float targetAngle = MathF.Atan2(returnDirection.Y, returnDirection.X) + MathF.PI;
                newOrientation = SmoothOrientation(ant.Orientation, targetAngle);
                velocity = new Vector2(MathF.Cos(newOrientation), MathF.Sin(newOrientation)) * traits.Speed;

                // Si encuentra comida real → toma carga
                if (currentCell.Type == CellType.Food)
                {
                    newHasFood = true;
                }
            }
            else
            {
                // Pierde rastro RETURN → vuelve a EXPLORING (se acabó la comida)
                newState = AntState.Exploring;
            }
        }

        // === ESTADO: WORKING (con comida) - siguiendo RETURN normal hacia nido ===
        else if (ant.State == AntState.Working && ant.HasFood && Constants.PHEROMONES_ENABLED)
        {
            // Llegó al nido
            if (currentCell.Type == CellType.Nest)
            {
                newState = AntState.Idle;
                newHasFood = false;
                velocity = Vector2.Zero;
                return new RoleDecision
                {
                    Action = new AntAction { Velocity = velocity },
                    NewState = newState,
                    NewOrientation = newOrientation,
                    NewHasFood = newHasFood
                };
            }

            Vector2 returnDirection = FindPheromoneDirectionByGradient(position, pheromones, ant.ColonyId, PheromoneType.Return, grid, ant.Orientation);

            if (returnDirection.LengthSquared() > 0.1f)
            {
                // Sigue RETURN normal (hacia nido) con suavizado
                float targetAngle = MathF.Atan2(returnDirection.Y, returnDirection.X);
                newOrientation = SmoothOrientation(ant.Orientation, targetAngle);
                velocity = new Vector2(MathF.Cos(newOrientation), MathF.Sin(newOrientation)) * traits.Speed;
            }
            // Si pierde rastro: usa FASE 3 (busca aleatoriamente hasta encontrar RETURN o Nest; ignora EXPLORE)
        }

        // === ESTADO: RETURNING - sigue EXPLORE inverso para construir puente ===
        else if (ant.State == AntState.Returning && ant.HasFood && Constants.PHEROMONES_ENABLED)
        {
            // Llegó al nido
            if (currentCell.Type == CellType.Nest)
            {
                newState = AntState.Idle;
                newHasFood = false;
                velocity = Vector2.Zero;
                return new RoleDecision
                {
                    Action = new AntAction { Velocity = velocity },
                    NewState = newState,
                    NewOrientation = newOrientation,
                    NewHasFood = newHasFood
                };
            }

            // Busca rastro EXPLORE en dirección INVERSA +180° (porque EXPLORE va nido→comida)
            Vector2 exploreDirection = FindPheromoneDirectionByGradient(position, pheromones, ant.ColonyId, PheromoneType.Explore, grid, ant.Orientation);

            if (exploreDirection.LengthSquared() > 0.1f)
            {
                // Invierte dirección (+180°) con suavizado
                float invertedAngle = MathF.Atan2(exploreDirection.Y, exploreDirection.X);
                invertedAngle = MathUtils.NormalizeAngle(invertedAngle);
                newOrientation = SmoothOrientation(ant.Orientation, invertedAngle);
                velocity = new Vector2(MathF.Cos(newOrientation), MathF.Sin(newOrientation)) * traits.Speed;
            }
            else
            {
                // Pierde rastro EXPLORE: usa FASE 3
                // Si encuentra RETURN → sigue normal
                Vector2 returnDirection = FindPheromoneDirectionByGradient(position, pheromones, ant.ColonyId, PheromoneType.Return, grid, ant.Orientation);
                if (returnDirection.LengthSquared() > 0.1f)
                {
                    float targetAngle = MathF.Atan2(returnDirection.Y, returnDirection.X);
                    newOrientation = SmoothOrientation(ant.Orientation, targetAngle);
                    velocity = new Vector2(MathF.Cos(newOrientation), MathF.Sin(newOrientation)) * traits.Speed;
                }
                // Si encuentra EXPLORE → sigue inverso
                else if (exploreDirection.LengthSquared() > 0.1f)
                {
                    float invertedAngle = MathF.Atan2(exploreDirection.Y, exploreDirection.X) + MathF.PI;
                    newOrientation = SmoothOrientation(ant.Orientation, invertedAngle);
                    velocity = new Vector2(MathF.Cos(newOrientation), MathF.Sin(newOrientation)) * traits.Speed;
                }
                // Si nada: usa FASE 3 (movimiento serpenteante)
            }
        }

        return new RoleDecision
        {
            Action = new AntAction { Velocity = velocity },
            NewState = newState,
            NewOrientation = newOrientation,
            NewHasFood = newHasFood
        };
    }

    /// <summary>
    /// Detecta la dirección del rastro usando ajuste de línea por mínimos cuadrados ponderados (PCA).
    /// Valida contra la orientación actual para evitar cambios bruscos > 90°.
    /// </summary>
    private Vector2 FindPheromoneDirectionByGradient(Vector2 position, PheromoneGrid pheromones, int colonyId, PheromoneType type, GridSystem grid, float currentOrientation)
    {
        var points = new List<(float x, float y, float intensity)>();
        const int searchRadius = 3;
        const float threshold = 0.001f;

        // Recolectar puntos con feromonas significativas
        for (int x = -searchRadius; x <= searchRadius; x++)
        {
            for (int y = -searchRadius; y <= searchRadius; y++)
            {
                int gx = (int)position.X + x;
                int gy = (int)position.Y + y;

                if (gx < 0 || gx >= grid.Width || gy < 0 || gy >= grid.Height)
                    continue;

                float intensity = pheromones.GetPheromone(gx, gy, colonyId, type);
                if (intensity > threshold)
                    points.Add((gx, gy, intensity));
            }
        }

        if (points.Count < 2)
            return Vector2.Zero;

        // Ajuste de línea por mínimos cuadrados ponderados (PCA)
        float sumW = 0, sumX = 0, sumY = 0, sumXX = 0, sumXY = 0, sumYY = 0;

        foreach (var (x, y, intensity) in points)
        {
            sumW += intensity;
            sumX += x * intensity;
            sumY += y * intensity;
            sumXX += x * x * intensity;
            sumXY += x * y * intensity;
            sumYY += y * y * intensity;
        }

        float meanX = sumX / sumW;
        float meanY = sumY / sumW;

        // Matriz de covarianza
        float cov_xx = (sumXX / sumW) - (meanX * meanX);
        float cov_xy = (sumXY / sumW) - (meanX * meanY);
        float cov_yy = (sumYY / sumW) - (meanY * meanY);

        // Diagonalizar usando eigenvalues (similar a PCA)
        float trace = cov_xx + cov_yy;
        float det = cov_xx * cov_yy - cov_xy * cov_xy;
        float discriminant = trace * trace - 4 * det;

        if (discriminant < 0)
            return Vector2.Zero;

        float lambda1 = (trace + MathF.Sqrt(discriminant)) / 2;
        float eigX = cov_xy;
        float eigY = lambda1 - cov_xx;

        if (MathF.Abs(eigX) < 0.0001f && MathF.Abs(eigY) < 0.0001f)
            return Vector2.Zero;

        Vector2 direction = Vector2.Normalize(new Vector2(eigX, eigY));

        // Orientar basándose en los extremos: encontrar puntos más alejados en cada dirección
        float maxDist_forward = 0, maxDist_backward = 0;
        float maxIntensity_forward = 0, maxIntensity_backward = 0;

        foreach (var (x, y, intensity) in points)
        {
            float dist = (x - meanX) * direction.X + (y - meanY) * direction.Y;

            if (dist > maxDist_forward)
            {
                maxDist_forward = dist;
                maxIntensity_forward = intensity;
            }
            if (dist < maxDist_backward)
            {
                maxDist_backward = dist;
                maxIntensity_backward = intensity;
            }
        }

        // Orientar hacia el extremo con mayor intensidad
        if (maxIntensity_backward > maxIntensity_forward)
            direction = -direction;

        // Validar contra orientación actual: si el cambio es > 90°, mantener orientación actual
        if (direction.LengthSquared() > 0.01f)
        {
            float gradientAngle = MathF.Atan2(direction.Y, direction.X);
            float angleDiff = MathF.Abs(MathUtils.NormalizeAngle(gradientAngle - currentOrientation));

            if (angleDiff > MathF.PI / 2)
                return new Vector2(MathF.Cos(currentOrientation), MathF.Sin(currentOrientation));
        }

        return direction;
    }

    /// <summary>
    /// Interpola (suaviza) la orientación actual hacia una orientación objetivo.
    /// Evita giros bruscos, creando una transición natural.
    /// </summary>
    private float SmoothOrientation(float currentOrientation, float targetOrientation, float smoothingFactor = 0.15f)
    {
        // Normalizar ángulos a [0, 2π)
        // currentOrientation = MathUtils.NormalizeAngle(currentOrientation);
        // targetOrientation = MathUtils.NormalizeAngle(targetOrientation);

        // Calcular diferencia angular (siempre tomar el camino más corto)
        float diff = targetOrientation - currentOrientation;
        if (diff > MathF.PI) diff -= 2 * MathF.PI;
        if (diff < -MathF.PI) diff += 2 * MathF.PI;

        // Interpolar
        return MathUtils.NormalizeAngle(currentOrientation + diff * smoothingFactor);
    }

    private bool CheckPheromoneThreshold(Vector2 position, PheromoneGrid pheromones, int colonyId, PheromoneType type, GridSystem grid, float threshold)
    {
        for (int dx = -3; dx <= 3; dx++)
        {
            for (int dy = -3; dy <= 3; dy++)
            {
                int nx = (int)position.X + dx;
                int ny = (int)position.Y + dy;

                if (nx < 0 || nx >= grid.Width || ny < 0 || ny >= grid.Height)
                    continue;

                if (pheromones.GetPheromone(nx, ny, colonyId, type) > threshold)
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// [DEBUG ONLY] Guarda TODA la malla 7x7 de feromona a un archivo .txt para análisis.
    /// Captura los 49 puntos sin filtrar por umbral.
    /// Llamar manualmente desde el debugger cuando necesites capturar datos.
    /// </summary>
    private int DebugSaveMeshToFile(Vector2 position, PheromoneGrid pheromones, int colonyId, PheromoneType type, GridSystem grid)
    {
        System.Console.WriteLine($"Guardando malla");
        try
        {
            string debugDir = @"C:\Users\bgaxiola\OneDrive - Capgemini\Projects\Ants\ant_simulator\AntSimulator.Tests\bin\Debug\net10.0\bin\Debug\visualizations";
            Directory.CreateDirectory(debugDir);

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
            string filename = Path.Combine(debugDir, $"mesh_{colonyId}_{type}_{timestamp}.txt");

            var lines = new List<string>();
            lines.Add($"# Centro: ({position.X:F1}, {position.Y:F1}) | Colonia: {colonyId} | Tipo: {type}");
            lines.Add($"# Malla 7x7 completa (49 puntos)");
            lines.Add("");

            const int searchRadius = 3;
            int pointCount = 0;

            // Iterar todos los 49 puntos sin filtrar
            for (int x = -searchRadius; x <= searchRadius; x++)
            {
                for (int y = -searchRadius; y <= searchRadius; y++)
                {
                    int gx = (int)position.X + x;
                    int gy = (int)position.Y + y;

                    if (gx >= 0 && gx < grid.Width && gy >= 0 && gy < grid.Height)
                    {
                        float intensity = pheromones.GetPheromone(gx, gy, colonyId, type);
                        lines.Add($"({gx},{gy}) = {intensity:F6}");
                        pointCount++;
                    }
                }
            }

            File.WriteAllLines(filename, lines);
            System.Console.WriteLine($"✓ Malla guardada en: {filename}");
            return 1;
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"✗ Error: {ex.GetType().Name} - {ex.Message}");
            return 0;
        }
    }
}
