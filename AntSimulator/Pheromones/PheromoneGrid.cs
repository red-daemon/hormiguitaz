namespace AntSimulator.Pheromones;

public class PheromoneGrid
{
    private Dictionary<PheromoneType, PheromoneLayer> _layers;

    public PheromoneGrid(int width, int height)
    {
        _layers = new Dictionary<PheromoneType, PheromoneLayer>
        {
            { PheromoneType.Food, new PheromoneLayer(width, height) },
            { PheromoneType.Return, new PheromoneLayer(width, height) },
            { PheromoneType.Alert, new PheromoneLayer(width, height) }
        };
    }

    public float GetPheromone(int x, int y, int colonyId, PheromoneType type)
    {
        if (!_layers.ContainsKey(type))
            return 0f;

        return _layers[type].GetPheromone(x, y, colonyId);
    }

    public void Deposit(int x, int y, int colonyId, PheromoneType type, float amount)
    {
        if (!_layers.ContainsKey(type))
            return;

        _layers[type].Deposit(x, y, colonyId, amount);
    }

    public void Update(float deltaTime)
    {
        foreach (var layer in _layers.Values)
        {
            layer.Update(deltaTime);
        }
    }
}
