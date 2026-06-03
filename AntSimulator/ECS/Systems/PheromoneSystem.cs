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

            var depositRate = world.Colonies[ants[i].ColonyId].Traits.PheromoneDepositRate;

            if (ants[i].State == AntState.Returning)
            {
                // Strong pheromone trail when returning with food
                pheromones.Deposit(
                    x, y,
                    ants[i].ColonyId,
                    PheromoneType.Food,
                    depositRate * 0.5f
                );
            }
            else if (ants[i].State == AntState.Exploring)
            {
                // Weak exploration pheromone
                pheromones.Deposit(
                    x, y,
                    ants[i].ColonyId,
                    PheromoneType.Food,
                    depositRate * 0.05f
                );
            }
        }

        // Diffusion + evaporation
        pheromones.Update(deltaTime);
    }
}
