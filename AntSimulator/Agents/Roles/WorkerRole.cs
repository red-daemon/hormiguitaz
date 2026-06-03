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
            // Check for food nearby
            var foodPheromone = pheromones.GetPheromone((int)position.X, (int)position.Y, ant.ColonyId, PheromoneType.Food);

            if (foodPheromone > 0.1f)
            {
                // Found food, switch to returning
                return new AntAction { Velocity = Vector2.Zero };
            }

            // Random exploration with bias
            float angle = (float)((_random.NextDouble() - 0.5) * Math.PI * 2);
            velocity = new Vector2(
                MathF.Cos(angle),
                MathF.Sin(angle)
            ) * traits.Speed;

            // Apply explore bias towards undiscovered areas
            if (_random.NextDouble() < traits.ExploreBias)
            {
                velocity *= 1.2f;
            }
        }
        else if (ant.State == AntState.Returning)
        {
            // Head back to nest
            Vector2 direction = nestPosition - position;
            if (direction.LengthSquared() > 0.1f)
            {
                velocity = Vector2.Normalize(direction) * traits.Speed;
            }

            // Check if reached nest
            if (Vector2.Distance(position, nestPosition) < 5f)
            {
                return new AntAction { Velocity = Vector2.Zero };
            }
        }

        return new AntAction { Velocity = velocity };
    }
}
