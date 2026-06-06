using System.Numerics;
using AntSimulator.Agents;
using AntSimulator.Colonies;
using AntSimulator.ECS.Systems;
using AntSimulator.Environment;

namespace AntSimulator.Lab;

/// <summary>
/// Generador de mundos de laboratorio (debug) para pruebas paso a paso de comportamientos específicos.
/// Crea mundos 3x3 controlados con una sola hormiga en estado predefinido.
/// </summary>
public static class LabWorldHelper
{
    /// <summary>
    /// Crea un mundo 3x3 minimal con una hormiga con parámetros predefinidos.
    /// </summary>
    /// <param name="antPosition">Posición inicial de la hormiga dentro del grid 3x3</param>
    /// <param name="initialOrientation">Orientación inicial en radianes (0 = derecha, π/2 = abajo, π = izquierda, 3π/2 = arriba)</param>
    /// <param name="state">Estado de la hormiga (Exploring, Idle, etc.)</param>
    /// <returns>World configurado y listo para ejecutar paso a paso</returns>
    public static World CreateExploringAntWorld(Vector2 antPosition, float initialOrientation, AntState state = AntState.Exploring)
    {
        const int gridSize = 3;
        var world = new World(gridSize, gridSize);

        // Crear colonia minimal (sin marcar nido en el grid)
        var traits = new ColonyTraits();
        var colony = new Colony(1, Vector2.Zero, traits);
        world.Colonies.Add(1, colony);

        // Crear una hormiga en posición predefinida
        int antId = world.Ants.CreateAnt(1, antPosition);

        // Configurar hormiga con parámetros específicos
        var ants = world.Ants.GetAntsMutable();
        ants[antId].State = state;
        ants[antId].Orientation = initialOrientation;
        ants[antId].Energy = Constants.DEFAULT_ENERGY;
        ants[antId].TicksInState = 0;

        colony.IncrementPopulation();

        // Registrar todos los sistemas para comportamiento realista
        world.RegisterSystem(new BehaviorSystem());
        world.RegisterSystem(new MovementSystem());
        world.RegisterSystem(new PheromoneSystem());
        world.RegisterSystem(new EnergySystem());

        return world;
    }
}
