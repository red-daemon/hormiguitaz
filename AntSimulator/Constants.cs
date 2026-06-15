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
    public const float EXPLORE_DEPOSIT_RATE = 1.0f;     // Gradiente lineal de 20k ticks (1.0 → 0)
    public const float RETURN_DEPOSIT_RATE = 1.0f;      // Fuerte, crea el puente (sin límite)
    public const float DEFAULT_ENERGY = 300f;
    public const float ENERGY_PER_SECOND = 5f;
    public const float PHEROMONE_DIFFUSION_RATE = 0.00001f;            // Decay lineal muy bajo: 0.00001 por tick
    public const float PHEROMONE_DIFFUSION_ORTHOGONAL_WEIGHT = 0.586f;  // ~59% - inverse euclidean distance (1.0 / 1.0)
    public const float PHEROMONE_DIFFUSION_DIAGONAL_WEIGHT = 0.414f;   // ~41% - inverse euclidean distance (1.0 / sqrt(2))

    public const float PHEROMONE_EVAPORATION_PERCENTAGE = 0f;          // Desuso
    public const float PHEROMONE_EVAPORATION_FIXED = 0.00004f;         // Decay lineal: 0.00004 por tick
                                                                       // Total: 0.00001 + 0.00004 = 0.00005 (llega a 0 en 20k ticks)

    // Colorización de feromonas: saturación de color para el renderer
    // Determinado empíricamente con PheromoneDepositAnalysisTests:
    // - Hormiga deposita ~18.79 unidades promedio al cruzar una casilla
    // - 10 hormigas × 18.79 = ~188 unidades para saturar el color
    // - El color visual no se vuelve más intenso aunque la feromona acumule más
    public const float PHEROMONE_COLOR_SATURATION = 188f;
}
