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
            if (ants[i].State == AntState.Dead || ants[i].State == AntState.Idle)
                continue;

            int x = (int)positions[i].X;
            int y = (int)positions[i].Y;
            var colonyId = ants[i].ColonyId;

            // EXPLORING: deposita EXPLORE
            if (ants[i].State == AntState.Exploring)
            {
                pheromones.Deposit(
                    x, y,
                    colonyId,
                    PheromoneType.Explore,
                    Constants.EXPLORE_DEPOSIT_RATE
                );
            }
            // WORKING con comida: deposita RETURN (refuerza puente)
            else if (ants[i].State == AntState.Working && ants[i].HasFood)
            {
                pheromones.Deposit(
                    x, y,
                    colonyId,
                    PheromoneType.Return,
                    Constants.RETURN_DEPOSIT_RATE
                );
            }
            // RETURNING: deposita RETURN (construye puente)
            else if (ants[i].State == AntState.Returning)
            {
                pheromones.Deposit(
                    x, y,
                    colonyId,
                    PheromoneType.Return,
                    Constants.RETURN_DEPOSIT_RATE
                );
            }
            // WORKING sin comida: NO deposita nada
        }

        // Diffusion + evaporation
        pheromones.Update(deltaTime);
    }
}
