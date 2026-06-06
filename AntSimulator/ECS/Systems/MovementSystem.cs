using System.Numerics;
using AntSimulator.Agents;
using AntSimulator.ECS.Components;

namespace AntSimulator.ECS.Systems;

/// <summary>
/// Sistema de movimiento de hormigas.
/// Actualiza posiciones según velocidad, detecta colisiones con paredes,
/// y ajusta la orientación para caminar paralelo a la pared.
/// </summary>
public class MovementSystem : ISystem
{
    /// <summary>Ajuste angular (3°) aplicado cuando la hormiga toca una pared para caminar paralela.</summary>
    private const float WALL_PARALLEL_ANGLE_ADJUSTMENT = 3f * MathF.PI / 180f;

    /// <summary>
    /// Calcula la diferencia angular mínima entre dos ángulos en radianes.
    /// Retorna un valor entre -π y π.
    /// </summary>
    private static float AngleDifference(float angle1, float angle2)
    {
        float diff = angle1 - angle2;
        while (diff > MathF.PI) diff -= 2 * MathF.PI;
        while (diff < -MathF.PI) diff += 2 * MathF.PI;
        return diff;
    }

    /// <summary>
    /// Normaliza la orientación al rango [0, 2π).
    /// </summary>
    private static void NormalizeOrientation(ref AntComponent ant)
    {
        ant.Orientation = ant.Orientation % (2 * MathF.PI);
        if (ant.Orientation < 0)
            ant.Orientation += 2 * MathF.PI;
    }

    /// <summary>
    /// Determina la orientación paralela a una pared.
    /// Calcula cuál de los dos ángulos paralelos es más cercano a la orientación actual,
    /// luego suma o resta 3° para que la hormiga se aleje de la pared.
    /// </summary>
    private static float GetParallelWallOrientation(float currentOrientation, bool isVerticalWall)
    {
        float angle1, angle2;

        if (isVerticalWall)
        {
            // Paredes verticales: ángulos paralelos son 90° (arriba) y 270° (abajo)
            angle1 = MathF.PI / 2f;
            angle2 = 3 * MathF.PI / 2f;
        }
        else
        {
            // Paredes horizontales: ángulos paralelos son 0° (derecha) y 180° (izquierda)
            angle1 = 0f;
            angle2 = MathF.PI;
        }

        // Elegir el ángulo paralelo más cercano
        float diff1 = MathF.Abs(AngleDifference(currentOrientation, angle1));
        float diff2 = MathF.Abs(AngleDifference(currentOrientation, angle2));

        float parallelAngle = diff1 <= diff2 ? angle1 : angle2;

        // Determinar si sumar o restar según la orientación actual relativa al paralelo
        float diff = AngleDifference(currentOrientation, parallelAngle);
        float scaledAdjustment = WALL_PARALLEL_ANGLE_ADJUSTMENT * (MathF.Abs(diff) / MathF.PI);
        float adjustment = diff > 0 ? -scaledAdjustment : scaledAdjustment;

        return parallelAngle + adjustment;
    }

    /// <summary>
    /// Actualiza la posición de todas las hormigas vivas y gestiona colisiones con paredes.
    /// </summary>
    public void Update(float deltaTime, World world)
    {
        var positions = world.Ants.GetPositionsMutable();
        var velocities = world.Ants.GetVelocities();
        var ants = world.Ants.GetAntsMutable();
        var grid = world.Grid;

        for (int i = 0; i < world.Ants.EntityCount; i++)
        {
            if (ants[i].State == AntState.Dead) continue;

            var newPos = positions[i] + velocities[i] * deltaTime;

            // Detectar colisión con pared izquierda/derecha (vertical)
            if (newPos.X < 0 || newPos.X >= grid.Width)
            {
                ants[i].Orientation = GetParallelWallOrientation(ants[i].Orientation, isVerticalWall: true);
                NormalizeOrientation(ref ants[i]);
            }

            // Detectar colisión con pared superior/inferior (horizontal)
            if (newPos.Y < 0 || newPos.Y >= grid.Height)
            {
                ants[i].Orientation = GetParallelWallOrientation(ants[i].Orientation, isVerticalWall: false);
                NormalizeOrientation(ref ants[i]);
            }

            // Clampear posición para mantenerla dentro del grid
            positions[i] = Vector2.Clamp(
                newPos,
                Vector2.Zero,
                new Vector2(grid.Width, grid.Height)
            );
        }
    }
}
