namespace AntSimulator.ECS.Systems;

public interface ISystem
{
    void Update(float deltaTime, World world);
}
