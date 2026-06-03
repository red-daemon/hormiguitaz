using AntSimulator.Agents;

namespace AntSimulator.ECS.Components;

public struct AntComponent
{
    public AntState State;
    public int ColonyId;
    public float Energy;
    public int TicksInState;
}
