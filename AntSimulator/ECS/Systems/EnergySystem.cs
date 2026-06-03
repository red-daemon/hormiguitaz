using AntSimulator.Agents;

namespace AntSimulator.ECS.Systems;

public class EnergySystem : ISystem
{
    public void Update(float deltaTime, World world)
    {
        var ants = world.Ants.GetAntsMutable();
        var velocities = world.Ants.GetVelocities();

        for (int i = 0; i < world.Ants.EntityCount; i++)
        {
            if (ants[i].State == AntState.Dead) continue;

            float movementCost = velocities[i].Length() * 0.1f;
            ants[i].Energy -= movementCost * deltaTime;

            if (ants[i].Energy <= 0)
            {
                ants[i].State = AntState.Dead;
            }
        }
    }
}
