using System.Numerics;

namespace AntSimulator.ECS.Components;

public struct Velocity
{
    public Vector2 Value;

    public Velocity(Vector2 value)
    {
        Value = value;
    }
}
