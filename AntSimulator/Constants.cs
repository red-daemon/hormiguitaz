namespace AntSimulator;

public static class Constants
{
    // Debug flags
    public const bool PHEROMONES_ENABLED = true;  // Toggle para desactivar feromonas en debug

    public const float DEFAULT_ANT_SPEED = 3f;
    public const float DEFAULT_PHEROMONE_SENSITIVITY = 0.8f;
    public const float DEFAULT_EXPLORE_BIAS = 0.3f;
    public const float DEFAULT_PHEROMONE_DEPOSIT_RATE = 0.5f;

    // Pheromone deposit rates
    public const float EXPLORE_DEPOSIT_RATE = 0.05f;    // Ligera pero duradera
    public const float RETURN_DEPOSIT_RATE = 1.0f;      // Fuerte, crea el puente
    public const float DEFAULT_ENERGY = 100f;
    public const float ENERGY_PER_SECOND = 5f;
    public const float PHEROMONE_DIFFUSION_RATE = 0.001f;
    public const float PHEROMONE_DIFFUSION_ORTHOGONAL_WEIGHT = 0.586f;  // ~59% - inverse euclidean distance (1.0 / 1.0)
    public const float PHEROMONE_DIFFUSION_DIAGONAL_WEIGHT = 0.414f;   // ~41% - inverse euclidean distance (1.0 / sqrt(2))

    public const float PHEROMONE_EVAPORATION_PERCENTAGE = 0.00001f;  // 5% evaporation
    public const float PHEROMONE_EVAPORATION_FIXED = 0.0001f;       // +0.01 fixed evaporation
}
