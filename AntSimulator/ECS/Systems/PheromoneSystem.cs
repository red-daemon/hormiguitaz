using AntSimulator.Agents;
using AntSimulator.Pheromones;

namespace AntSimulator.ECS.Systems;

public class PheromoneSystem : ISystem
{
    public void Update(float deltaTime, World world)
    {
        var ants = world.Ants.GetAnts();
        var positions = world.Ants.GetPositions();
        var pheromones = world.Pheromones;

        // Deposit pheromones
        for (int i = 0; i < world.Ants.EntityCount; i++)
        {
            if (ants[i].State == AntState.Dead) continue;

            int x = (int)positions[i].X;
            int y = (int)positions[i].Y;

            // Deposit exploration pheromone
            if (ants[i].State == AntState.Exploring)
            {
                pheromones.Deposit(
                    x, y,
                    ants[i].ColonyId,
                    PheromoneType.Food,
                    world.Colonies[ants[i].ColonyId].Traits.PheromoneDepositRate * 0.1f
                );
            }
        }

        // Diffusion + evaporation
        pheromones.Update(deltaTime);
    }
}
