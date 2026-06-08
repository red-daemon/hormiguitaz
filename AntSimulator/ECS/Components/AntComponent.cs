using AntSimulator.Agents;

namespace AntSimulator.ECS.Components;

public struct AntComponent
{
    public AntState State;
    public int ColonyId;
    public float Energy;
    public int TicksInState;
    public float Orientation;           // Ángulo actual en radianes (0-2π), -1 si no ha salido del nido
    public float TargetOrientation;     // Ángulo objetivo para suavizado (lerp)
    public int WaitTicksRemaining;      // Ticks que espera antes de salir del nido
    public bool HasFood;                // True si la hormiga está cargando comida hacia el nido
}
