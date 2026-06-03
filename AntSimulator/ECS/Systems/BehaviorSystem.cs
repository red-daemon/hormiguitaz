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
        var positions = world.Ants.GetPositions();
        var ants = world.Ants.GetAntsMutable();
        var velocities = world.Ants.GetVelocitiesMutable();
        var grid = world.Grid;
        var pheromones = world.Pheromones;

        for (int i = 0; i < world.Ants.EntityCount; i++)
        {
            if (ants[i].State == AntState.Dead) continue;

            var colony = world.Colonies[ants[i].ColonyId];
            var role = _roles[ants[i].ColonyId];

            var action = role.DecideAction(
                i,
                positions[i],
                ants[i],
                grid,
                pheromones,
                colony.Traits,
                colony.NestPosition
            );

            velocities[i] = action.Velocity;
        }
    }
}
