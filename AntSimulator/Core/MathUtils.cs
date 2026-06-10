namespace AntSimulator.Core;

/// <summary>
/// Utilidades matemáticas para operaciones comunes en la simulación.
/// </summary>
public static class MathUtils
{
    /// <summary>
    /// Normaliza un ángulo en radianes al rango [0, 2π).
    /// </summary>
    public static float NormalizeAngle(float angleRadians)
    {
        angleRadians %= 2 * MathF.PI;
        if (angleRadians < 0)
            angleRadians += 2 * MathF.PI;
        return angleRadians;
    }
}
