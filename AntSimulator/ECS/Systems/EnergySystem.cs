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

            // Minimal energy cost (base metabolism)
            float baseCost = 0.5f;
            float movementCost = velocities[i].Length() * 0.001f;
            float totalCost = (baseCost + movementCost) * deltaTime;

            ants[i].Energy -= totalCost;

            if (ants[i].Energy <= 0)
            {
                ants[i].State = AntState.Dead;
                world.Ants.DestroyAnt(i);
            }
        }
    }
}
