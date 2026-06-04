using System.Numerics;
using AntSimulator.Agents;
using AntSimulator.Agents.Roles;
using AntSimulator.ECS.Components;

namespace AntSimulator.ECS.Systems;

public class BehaviorSystem : ISystem
{
    private Dictionary<int, IRoleStrategy> _roles = new()
    {
        { 1, new WorkerRole() }
    };

    public void Update(float deltaTime, World world)
    {
        var positions = world.Ants.GetPositionsMutable();
        var ants = world.Ants.GetAntsMutable();
        var velocities = world.Ants.GetVelocitiesMutable();
        var grid = world.Grid;
        var pheromones = world.Pheromones;

        for (int i = 0; i < world.Ants.EntityCount; i++)
        {
            if (ants[i].State == AntState.Dead) continue;

            // Decrement wait time if still waiting
            if (ants[i].WaitTicksRemaining > 0)
            {
                ants[i].WaitTicksRemaining--;
            }

            var colony = world.Colonies[ants[i].ColonyId];
            var role = _roles[ants[i].ColonyId];

            // Check if this tick the ant is about to leave nest for first time
            bool isLeavingNest = (ants[i].WaitTicksRemaining == 0 && ants[i].Orientation < 0);

            var decision = role.DecideAction(
                i,
                positions[i],
                ants[i],
                grid,
                pheromones,
                colony.Traits,
                colony.NestPosition
            );

            velocities[i] = decision.Action.Velocity;

            // Apply state change if any
            if (decision.NewState.HasValue)
            {
                ants[i].State = decision.NewState.Value;
            }

            // Apply orientation change if any
            if (decision.NewOrientation.HasValue)
            {
                ants[i].Orientation = decision.NewOrientation.Value;
            }

            // Apply position change if any
            if (decision.NewPosition.HasValue)
            {
                positions[i] = decision.NewPosition.Value;
            }
        }
    }
}
