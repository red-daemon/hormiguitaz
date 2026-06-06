using System.Numerics;
using Xunit;
using AntSimulator;
using AntSimulator.ECS.Systems;
using AntSimulator.Colonies;
using AntSimulator.Agents;

namespace AntSimulator.Tests;

/// <summary>
/// Tests para el MovementSystem, enfocados en la lógica de colisión con paredes.
/// Crea un mundo super controlado con una sola hormiga para verificar comportamientos específicos.
/// </summary>
public class MovementSystemTests
{
    private const float DELTA_TIME = 0.016f;
    private const int GRID_SIZE = 100;

    /// <summary>
    /// Crea un mundo minimal con una sola hormiga posicionada y orientada específicamente.
    /// </summary>
    private World CreateTestWorld(Vector2 startPosition, float orientation)
    {
        var world = new World(GRID_SIZE, GRID_SIZE);

        // Crear una colonia dummy
        var traits = new ColonyTraits { Speed = 10f };
        var colony = new Colony(1, new Vector2(50, 50), traits);
        world.Colonies[1] = colony;

        // Crear una hormiga con parámetros controlados
        int antId = world.Ants.CreateAnt(colonyId: 1, position: startPosition);

        // Configurar velocidad GRANDE para garantizar colisión en el primer frame
        var velocities = world.Ants.GetVelocitiesMutable();
        velocities[antId] = new Vector2(MathF.Cos(orientation), MathF.Sin(orientation)) * 200f;

        var ants = world.Ants.GetAntsMutable();
        ants[antId].Orientation = orientation;
        ants[antId].State = AntState.Idle;

        // Registrar solo el MovementSystem para tests limpios
        world.RegisterSystem(new MovementSystem());

        return world;
    }

    /// <summary>
    /// Extrae la orientación actual de la hormiga en el mundo.
    /// </summary>
    private float GetAntOrientation(World world, int antId = 0)
    {
        var ants = world.Ants.GetAnts();
        return ants[antId].Orientation;
    }

    /// <summary>
    /// Normaliza un ángulo al rango [0, 2π) para comparaciones.
    /// </summary>
    private static float NormalizeAngle(float angle)
    {
        angle = angle % (2 * MathF.PI);
        if (angle < 0)
            angle += 2 * MathF.PI;
        return angle;
    }

    [Fact]
    public void Ant_TouchesLeftWall_AdjustsOrientationToParallel()
    {
        // Pared izquierda (X < 0), hormiga apuntando hacia arriba-izquierda (270° - algo)
        float initialOrientation = 225f * MathF.PI / 180f; // ~225° (arriba-izquierda en sistema 2D)
        var world = CreateTestWorld(new Vector2(1, 50), initialOrientation);

        world.Update(DELTA_TIME);

        float finalOrientation = NormalizeAngle(GetAntOrientation(world));
        // Ángulos paralelos a pared vertical: 90° (abajo) y 270° (arriba)
        // 225° está más cerca de 270°, así que elige 270° + 3°
        float expectedParallel = NormalizeAngle(270f * MathF.PI / 180f + 3f * MathF.PI / 180f);

        float angleDiff = MathF.Abs(finalOrientation - expectedParallel);
        if (angleDiff > MathF.PI)
            angleDiff = 2 * MathF.PI - angleDiff;

        Assert.True(angleDiff < 0.1f, $"Expected {expectedParallel:F2} rad ({expectedParallel * 180 / MathF.PI:F1}°), got {finalOrientation:F2} rad ({finalOrientation * 180 / MathF.PI:F1}°)");
    }

    [Fact]
    public void Ant_TouchesRightWall_AdjustsOrientationToParallel()
    {
        // Pared derecha (X >= grid.Width), hormiga apuntando hacia abajo-derecha
        float initialOrientation = 315f * MathF.PI / 180f; // ~315° (abajo-derecha)
        var world = CreateTestWorld(new Vector2(GRID_SIZE - 2, 50), initialOrientation);

        world.Update(DELTA_TIME);

        float finalOrientation = NormalizeAngle(GetAntOrientation(world));
        // Ángulos paralelos: 90° (abajo) y 270° (arriba)
        // 315° está más cerca de 270°, así que elige 270° + 3°
        float expectedParallel = NormalizeAngle(270f * MathF.PI / 180f + 3f * MathF.PI / 180f);

        float angleDiff = MathF.Abs(finalOrientation - expectedParallel);
        if (angleDiff > MathF.PI)
            angleDiff = 2 * MathF.PI - angleDiff;

        Assert.True(angleDiff < 0.1f, $"Expected {expectedParallel:F2} rad ({expectedParallel * 180 / MathF.PI:F1}°), got {finalOrientation:F2} rad ({finalOrientation * 180 / MathF.PI:F1}°)");
    }

    [Fact]
    public void Ant_TouchesTopWall_AdjustsOrientationToParallel()
    {
        // Pared superior (Y < 0), hormiga apuntando hacia arriba-derecha
        // 315° = arriba-derecha
        float initialOrientation = 315f * MathF.PI / 180f; // 315°
        var world = CreateTestWorld(new Vector2(50, 1), initialOrientation);

        world.Update(DELTA_TIME);

        float finalOrientation = NormalizeAngle(GetAntOrientation(world));
        // Ángulos paralelos: 0° (derecha) y 180° (izquierda)
        // 315° está más cerca de 0°, así que elige 0° + 3°
        float expectedParallel = NormalizeAngle(0f + 3f * MathF.PI / 180f);

        float angleDiff = MathF.Abs(finalOrientation - expectedParallel);
        if (angleDiff > MathF.PI)
            angleDiff = 2 * MathF.PI - angleDiff;

        Assert.True(angleDiff < 0.1f, $"Expected {expectedParallel:F2} rad ({expectedParallel * 180 / MathF.PI:F1}°), got {finalOrientation:F2} rad ({finalOrientation * 180 / MathF.PI:F1}°)");
    }

    [Fact]
    public void Ant_TouchesBottomWall_AdjustsOrientationToParallel()
    {
        // Pared inferior (Y >= grid.Height), hormiga apuntando hacia abajo
        float initialOrientation = 90f * MathF.PI / 180f; // 90° (abajo)
        var world = CreateTestWorld(new Vector2(50, GRID_SIZE - 2), initialOrientation);

        world.Update(DELTA_TIME);

        float finalOrientation = NormalizeAngle(GetAntOrientation(world));
        // Ángulos paralelos a pared horizontal: 0° (derecha) y 180° (izquierda)
        // 90° está equidistante, pero tomaremos el primero que encuentre: 0° + 3°
        float expectedParallel = NormalizeAngle(0f + 3f * MathF.PI / 180f);

        float angleDiff = MathF.Abs(finalOrientation - expectedParallel);
        if (angleDiff > MathF.PI)
            angleDiff = 2 * MathF.PI - angleDiff;

        Assert.True(angleDiff < 0.1f, $"Expected {expectedParallel:F2} rad ({expectedParallel * 180 / MathF.PI:F1}°), got {finalOrientation:F2} rad ({finalOrientation * 180 / MathF.PI:F1}°)");
    }

    [Fact]
    public void Ant_Away_FromWalls_MaintainsOrientation()
    {
        // Hormiga en el centro, lejos de paredes
        float initialOrientation = 45f * MathF.PI / 180f;
        var world = CreateTestWorld(new Vector2(50, 50), initialOrientation);

        var ants = world.Ants.GetAntsMutable();
        ants[0].Orientation = initialOrientation;

        world.Update(DELTA_TIME);

        float finalOrientation = GetAntOrientation(world);
        Assert.Equal(NormalizeAngle(initialOrientation), NormalizeAngle(finalOrientation), precision: 4);
    }

}
