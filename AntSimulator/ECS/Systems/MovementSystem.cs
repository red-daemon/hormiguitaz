using System.Numerics;
using AntSimulator.Agents;

namespace AntSimulator.ECS.Systems;

public class MovementSystem : ISystem
{
    public void Update(float deltaTime, World world)
    {
        var positions = world.Ants.GetPositionsMutable();
        var velocities = world.Ants.GetVelocities();
        var ants = world.Ants.GetAnts();
        var grid = world.Grid;

        for (int i = 0; i < world.Ants.EntityCount; i++)
        {
            if (ants[i].State == AntState.Dead) continue;

            positions[i] += velocities[i] * deltaTime;

            // Clamp to grid bounds
            positions[i] = Vector2.Clamp(
                positions[i],
                Vector2.Zero,
                new Vector2(grid.Width - 1, grid.Height - 1)
            );
        }
    }
}
