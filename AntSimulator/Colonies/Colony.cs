using System.Numerics;

namespace AntSimulator.Colonies;

public class Colony
{
    public int Id { get; }
    public Vector2 NestPosition { get; set; }
    public ColonyTraits Traits { get; set; }
    public int PopulationCount { get; private set; }

    public Colony(int id, Vector2 nestPosition, ColonyTraits traits)
    {
        Id = id;
        NestPosition = nestPosition;
        Traits = traits ?? new ColonyTraits();
        PopulationCount = 0;
    }

    public void IncrementPopulation() => PopulationCount++;
    public void DecrementPopulation() => PopulationCount = Math.Max(0, PopulationCount - 1);
}
