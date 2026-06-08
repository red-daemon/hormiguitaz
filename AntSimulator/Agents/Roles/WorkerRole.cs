using System.Numerics;
using AntSimulator.Colonies;
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
            newOrientation += deltaRotation;
        }

        // === ESTADO: EXPLORING ===
        if (ant.State == AntState.Exploring && currentCell.Type != CellType.Nest)
        {
            // Si encuentra comida real → RETURNING (primero en crear puente)
            if (currentCell.Type == CellType.Food)
            {
                newState = AntState.Returning;
                newHasFood = true;
            }
            // Busca rastro RETURN con prioridad absoluta
            else if (Constants.PHEROMONES_ENABLED)
            {
                Vector2 returnDirection = FindMaxPheromone(position, pheromones, ant.ColonyId, PheromoneType.Return, grid);
                if (returnDirection.LengthSquared() > 0.1f)
                {
                    newState = AntState.Working;
                    newHasFood = false;
                    newOrientation = MathF.Atan2(returnDirection.Y, returnDirection.X);
                    velocity = returnDirection * traits.Speed;
                }
                // Si no hay RETURN, busca rastro EXPLORE
                else
                {
                    Vector2 foodDirection = FindMaxPheromone(position, pheromones, ant.ColonyId, PheromoneType.Food, grid);
                    if (foodDirection.LengthSquared() > 0.1f && CheckPheromoneThreshold(position, pheromones, ant.ColonyId, PheromoneType.Food, grid, 0.05f))
                    {
                        newOrientation = MathF.Atan2(foodDirection.Y, foodDirection.X);
                        velocity = foodDirection * traits.Speed;
                    }
                }
            }
        }

        // === ESTADO: WORKING (sin comida) - siguiendo RETURN hacia comida ===
        else if (ant.State == AntState.Working && !ant.HasFood && Constants.PHEROMONES_ENABLED)
        {
            Vector2 returnDirection = FindMaxPheromone(position, pheromones, ant.ColonyId, PheromoneType.Return, grid);

            if (returnDirection.LengthSquared() > 0.1f)
            {
                // Sigue RETURN normal (hacia comida)
                newOrientation = MathF.Atan2(returnDirection.Y, returnDirection.X);
                velocity = returnDirection * traits.Speed;

                // Si encuentra comida real → toma carga
                if (currentCell.Type == CellType.Food)
                {
                    newHasFood = true;
                }
            }
            else
            {
                // Pierde rastro RETURN → vuelve a EXPLORING
                newState = AntState.Exploring;
            }
        }

        // === ESTADO: WORKING (con comida) - siguiendo RETURN hacia nido ===
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

            Vector2 returnDirection = FindMaxPheromone(position, pheromones, ant.ColonyId, PheromoneType.Return, grid);

            if (returnDirection.LengthSquared() > 0.1f)
            {
                // Sigue RETURN normal (hacia nido)
                newOrientation = MathF.Atan2(returnDirection.Y, returnDirection.X);
                velocity = returnDirection * traits.Speed;
            }
            // Si pierde rastro: usa FASE 3 pero busca Nest o RETURN (ignora EXPLORE)
        }

        // === ESTADO: RETURNING (constructora del puente) ===
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

            // Busca rastro EXPLORE (propio) en dirección INVERSA +180°
            Vector2 exploreDirection = FindMaxPheromone(position, pheromones, ant.ColonyId, PheromoneType.Food, grid);

            if (exploreDirection.LengthSquared() > 0.1f)
            {
                // Invierte dirección (+180°)
                float invertedAngle = MathF.Atan2(exploreDirection.Y, exploreDirection.X) + MathF.PI;
                newOrientation = invertedAngle;
                velocity = new Vector2(MathF.Cos(invertedAngle), MathF.Sin(invertedAngle)) * traits.Speed;
            }
            else
            {
                // Pierde rastro: busca ciegamente con FASE 3
                // Si encuentra RETURN → sigue normal
                Vector2 returnDirection = FindMaxPheromone(position, pheromones, ant.ColonyId, PheromoneType.Return, grid);
                if (returnDirection.LengthSquared() > 0.1f)
                {
                    newOrientation = MathF.Atan2(returnDirection.Y, returnDirection.X);
                    velocity = returnDirection * traits.Speed;
                }
                // Si encuentra EXPLORE → sigue inverso
                else if (exploreDirection.LengthSquared() > 0.1f)
                {
                    float invertedAngle = MathF.Atan2(exploreDirection.Y, exploreDirection.X) + MathF.PI;
                    newOrientation = invertedAngle;
                    velocity = new Vector2(MathF.Cos(invertedAngle), MathF.Sin(invertedAngle)) * traits.Speed;
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

    private Vector2 FindMaxPheromone(Vector2 position, PheromoneGrid pheromones, int colonyId, PheromoneType type, GridSystem grid)
    {
        Vector2 pheromoneDirection = Vector2.Zero;
        float maxPheromone = 0f;

        for (int dx = -3; dx <= 3; dx++)
        {
            for (int dy = -3; dy <= 3; dy++)
            {
                int nx = (int)position.X + dx;
                int ny = (int)position.Y + dy;

                if (nx < 0 || nx >= grid.Width || ny < 0 || ny >= grid.Height)
                    continue;

                float pheromone = pheromones.GetPheromone(nx, ny, colonyId, type);

                if (pheromone > maxPheromone)
                {
                    maxPheromone = pheromone;
                    Vector2 dir = new Vector2(nx - position.X, ny - position.Y);
                    if (dir.LengthSquared() > 0.1f)
                    {
                        pheromoneDirection = Vector2.Normalize(dir);
                    }
                }
            }
        }

        return pheromoneDirection;
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
}
