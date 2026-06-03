namespace AntSimulator.ECS.Components;

public struct PhysicsComponent
{
    public float Mass;
    public float Friction;
    public float MaxSpeed;

    public PhysicsComponent()
    {
        Mass = 1f;
        Friction = 0.9f;
        MaxSpeed = 50f;
    }
}
