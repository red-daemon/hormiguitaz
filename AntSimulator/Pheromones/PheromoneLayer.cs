namespace AntSimulator.Pheromones;

public class PheromoneLayer
{
    private Dictionary<int, float[,]> _coloniesData;
    private int _width;
    private int _height;

    public PheromoneLayer(int width, int height)
    {
        _width = width;
        _height = height;
        _coloniesData = new Dictionary<int, float[,]>();
    }

    public float GetPheromone(int x, int y, int colonyId)
    {
        if (x < 0 || x >= _width || y < 0 || y >= _height)
            return 0f;

        if (!_coloniesData.ContainsKey(colonyId))
            return 0f;

        return _coloniesData[colonyId][x, y];
    }

    public void Deposit(int x, int y, int colonyId, float amount)
    {
        if (x < 0 || x >= _width || y < 0 || y >= _height)
            return;

        if (!_coloniesData.ContainsKey(colonyId))
            _coloniesData[colonyId] = new float[_width, _height];

        _coloniesData[colonyId][x, y] = Math.Min(1f, _coloniesData[colonyId][x, y] + amount);
    }

    public void Update(float deltaTime)
    {
        foreach (var colonyId in _coloniesData.Keys.ToList())
        {
            var layer = _coloniesData[colonyId];

            // Diffusion with anisotropic weights (stronger in orthogonal directions)
            var temp = new float[_width, _height];
            Array.Copy(layer, temp, layer.Length);

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    float diffused = layer[x, y] * (1 - Constants.PHEROMONE_DIFFUSION_RATE);

                    // Orthogonal neighbors (up/down/left/right): 70% of diffusion rate / 4
                    float orthogonalShare = Constants.PHEROMONE_DIFFUSION_RATE * Constants.PHEROMONE_DIFFUSION_ORTHOGONAL_WEIGHT / 4f;
                    // Diagonal neighbors: 30% of diffusion rate / 4
                    float diagonalShare = Constants.PHEROMONE_DIFFUSION_RATE * Constants.PHEROMONE_DIFFUSION_DIAGONAL_WEIGHT / 4f;

                    // Orthogonal: (0,1), (0,-1), (1,0), (-1,0)
                    int[] orthX = { 0, 0, 1, -1 };
                    int[] orthY = { 1, -1, 0, 0 };
                    for (int i = 0; i < 4; i++)
                    {
                        int nx = x + orthX[i];
                        int ny = y + orthY[i];
                        if (nx >= 0 && nx < _width && ny >= 0 && ny < _height)
                        {
                            diffused += layer[nx, ny] * orthogonalShare;
                        }
                    }

                    // Diagonal: (1,1), (1,-1), (-1,1), (-1,-1)
                    int[] diagX = { 1, 1, -1, -1 };
                    int[] diagY = { 1, -1, 1, -1 };
                    for (int i = 0; i < 4; i++)
                    {
                        int nx = x + diagX[i];
                        int ny = y + diagY[i];
                        if (nx >= 0 && nx < _width && ny >= 0 && ny < _height)
                        {
                            diffused += layer[nx, ny] * diagonalShare;
                        }
                    }

                    temp[x, y] = diffused;
                }
            }

            // Evaporation: hybrid model (percentage + fixed amount)
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    float percentageEvaporation = temp[x, y] * Constants.PHEROMONE_EVAPORATION_PERCENTAGE;
                    temp[x, y] = Math.Max(0f, temp[x, y] - percentageEvaporation - Constants.PHEROMONE_EVAPORATION_FIXED);
                }
            }

            _coloniesData[colonyId] = temp;
        }
    }
}
