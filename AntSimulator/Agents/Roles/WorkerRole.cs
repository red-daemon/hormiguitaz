using System.Numerics;
using AntSimulator.Colonies;
using AntSimulator.ECS.Components;
using AntSimulator.Environment;
using AntSimulator.Pheromones;

namespace AntSimulator.Agents.Roles;

public class WorkerRole : IRoleStrategy
{
    private static readonly Random _random = new();

    public AntAction DecideAction(
        int id,
        Vector2 position,
        AntComponent ant,
        GridSystem grid,
        PheromoneGrid pheromones,
        ColonyTraits traits,
        Vector2 nestPosition)
    {
        Vector2 velocity = Vector2.Zero;

        if (ant.State == AntState.Exploring)
        {
            // Sample pheromones in neighboring cells
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

            // If found strong pheromone trail, follow it
            if (maxPheromone > 0.05f && pheromoneDirection.LengthSquared() > 0.1f)
            {
                velocity = pheromoneDirection * traits.Speed;
            }
            else
            {
                // Random walk with slight bias
                float angle = (float)((_random.NextDouble() - 0.5) * Math.PI * 2);
                velocity = new Vector2(
                    MathF.Cos(angle),
                    MathF.Sin(angle)
                ) * traits.Speed;
            }
        }
        else if (ant.State == AntState.Returning)
        {
            // Head back to nest
            Vector2 direction = nestPosition - position;
            if (direction.LengthSquared() > 1f)
            {
                velocity = Vector2.Normalize(direction) * traits.Speed;
            }
            else
            {
                velocity = Vector2.Zero;
            }
        }

        return new AntAction { Velocity = velocity };
    }
}
