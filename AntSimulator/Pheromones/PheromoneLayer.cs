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

            // Diffuse
            var temp = new float[_width, _height];
            Array.Copy(layer, temp, layer.Length);

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    float diffused = layer[x, y] * (1 - Constants.PHEROMONE_DIFFUSION_RATE);

                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            if (dx == 0 && dy == 0) continue;

                            int nx = x + dx;
                            int ny = y + dy;

                            if (nx >= 0 && nx < _width && ny >= 0 && ny < _height)
                            {
                                diffused += layer[nx, ny] * Constants.PHEROMONE_DIFFUSION_RATE / 8f;
                            }
                        }
                    }

                    temp[x, y] = diffused;
                }
            }

            // Evaporate
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    temp[x, y] *= (1 - Constants.PHEROMONE_EVAPORATION_RATE);
                }
            }

            _coloniesData[colonyId] = temp;
        }
    }
}
