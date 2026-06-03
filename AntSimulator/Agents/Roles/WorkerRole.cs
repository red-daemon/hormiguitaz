using System.Numerics;
using AntSimulator.Colonies;
using AntSimulator.ECS.Components;
using AntSimulator.Environment;
using AntSimulator.Pheromones;

namespace AntSimulator.Agents.Roles;

public class WorkerRole : IRoleStrategy
{
    private static readonly Random _random = new();

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
        var currentCell = grid.GetCell((int)position.X, (int)position.Y);

        // PHASE 1: Waiting in nest - NO MOVEMENT
        if (currentCell.Type == CellType.Nest && ant.WaitTicksRemaining > 0)
        {
            velocity = Vector2.Zero;
            // Return immediately - no orientation, no state change
            return new RoleDecision { Action = new AntAction { Velocity = velocity }, NewState = newState };
        }

        // PHASE 2: Leaving nest (first time)
        if (currentCell.Type == CellType.Nest && ant.WaitTicksRemaining == 0 && ant.Orientation < 0)
        {
            // Assign random orientation (0 to 2π)
            ant.Orientation = (float)(_random.NextDouble() * Math.PI * 2);
        }

        // PHASE 3: Moving by orientation (leaving or exploring with orientation)
        if (ant.Orientation >= 0)
        {
            // Calculate velocity based on orientation
            velocity = new Vector2(
                MathF.Cos(ant.Orientation),
                MathF.Sin(ant.Orientation)
            ) * traits.Speed;

            // Add random delta rotation (-1° to +1° in radians for smooth serpentine motion)
            float deltaRotation = (float)((_random.NextDouble() - 0.5) * 2 * Math.PI / 180);  // ±1° in radians
            ant.Orientation += deltaRotation;

            // Clamp orientation to 0-2π
            if (ant.Orientation < 0) ant.Orientation += (float)(Math.PI * 2);
            if (ant.Orientation >= Math.PI * 2) ant.Orientation -= (float)(Math.PI * 2);
        }

        // Check if on food cell
        if (ant.State == AntState.Exploring && currentCell.Type == CellType.Food)
        {
            newState = AntState.Returning;
        }

        // Check if reached nest
        if (ant.State == AntState.Returning && Vector2.Distance(position, nestPosition) < 3f)
        {
            newState = AntState.Exploring;
        }

        if (ant.State == AntState.Exploring && currentCell.Type != CellType.Nest)
        {
            // Sample pheromones in neighboring cells (outside nest)
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

                    float pheromone = pheromones.GetPheromone(nx, ny, ant.ColonyId, PheromoneType.Food);

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

            // If found strong pheromone trail, override orientation
            if (maxPheromone > 0.05f && pheromoneDirection.LengthSquared() > 0.1f)
            {
                // Switch to pheromone following
                ant.Orientation = MathF.Atan2(pheromoneDirection.Y, pheromoneDirection.X);
                velocity = pheromoneDirection * traits.Speed;
            }
        }
        else if (ant.State == AntState.Returning)
        {
            // Head back to nest
            Vector2 direction = nestPosition - position;
            if (direction.LengthSquared() > 1f)
            {
                velocity = Vector2.Normalize(direction) * traits.Speed;
                ant.Orientation = MathF.Atan2(direction.Y, direction.X);
            }
            else
            {
                velocity = Vector2.Zero;
            }
        }

        return new RoleDecision { Action = new AntAction { Velocity = velocity }, NewState = newState };
    }
}
