namespace AntSimulator;

public static class Constants
{
    public const float DEFAULT_ANT_SPEED = 3f;
    public const float DEFAULT_PHEROMONE_SENSITIVITY = 0.8f;
    public const float DEFAULT_EXPLORE_BIAS = 0.3f;
    public const float DEFAULT_PHEROMONE_DEPOSIT_RATE = 0.5f;
    public const float DEFAULT_ENERGY = 100f;
    public const float ENERGY_PER_SECOND = 5f;
    public const float PHEROMONE_DIFFUSION_RATE = 0.05f;
    public const float PHEROMONE_DIFFUSION_ORTHOGONAL_WEIGHT = 0.586f;  // ~59% - inverse euclidean distance (1.0 / 1.0)
    public const float PHEROMONE_DIFFUSION_DIAGONAL_WEIGHT = 0.414f;   // ~41% - inverse euclidean distance (1.0 / sqrt(2))

    public const float PHEROMONE_EVAPORATION_PERCENTAGE = 0.00001f;  // 5% evaporation
    public const float PHEROMONE_EVAPORATION_FIXED = 0.0001f;       // +0.01 fixed evaporation
}
