using System.Numerics;

namespace AntSimulator.ECS.Components;

public struct Position
{
    public Vector2 Value;

    public Position(Vector2 value)
    {
        Value = value;
    }
}
