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

        // Sin límite: acumula según deposite la hormiga
        _coloniesData[colonyId][x, y] += amount;
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
                    // Decay lineal por difusión: resta fija (no multiplicación)
                    float diffused = Math.Max(0f, layer[x, y] - Constants.PHEROMONE_DIFFUSION_RATE);

                    // Dispersión a vecinos (muy pequeña con decay lineal)
                    float orthogonalShare = Constants.PHEROMONE_DIFFUSION_RATE * Constants.PHEROMONE_DIFFUSION_ORTHOGONAL_WEIGHT / 4f;
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

            // Evaporation: decay lineal (resta fija, como evaporación real del agua)
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    temp[x, y] = Math.Max(0f, temp[x, y] - Constants.PHEROMONE_EVAPORATION_FIXED);
                }
            }

            _coloniesData[colonyId] = temp;
        }
    }
}
