namespace AntSimulator.Colonies;

public class ColonyTraits
{
    public float Speed { get; set; } = Constants.DEFAULT_ANT_SPEED;
    public float PheromonesSensitivity { get; set; } = Constants.DEFAULT_PHEROMONE_SENSITIVITY;
    public float ExploreBias { get; set; } = Constants.DEFAULT_EXPLORE_BIAS;
    public float PheromoneDepositRate { get; set; } = Constants.DEFAULT_PHEROMONE_DEPOSIT_RATE;
    public float MaxEnergy { get; set; } = Constants.DEFAULT_ENERGY;
    public float EnergyRegenRate { get; set; } = Constants.ENERGY_PER_SECOND;
}
