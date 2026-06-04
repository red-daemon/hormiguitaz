using System.Numerics;
using AntSimulator.Colonies;
using AntSimulator.ECS.Components;
using AntSimulator.Environment;
using AntSimulator.Pheromones;

namespace AntSimulator.Agents.Roles;

/// <summary>
/// Estrategia de comportamiento para hormigas obreras.
/// Maneja tres fases de vida: espera en el nido, salida inicial, y exploración/retorno.
/// </summary>
public class WorkerRole : IRoleStrategy
{
    /// <summary>
    /// Calcula la siguiente acción para una hormiga obrera.
    ///
    /// Fases:
    /// 1. ESPERA: Permanecer en el nido hasta que termine el tiempo de espera
    /// 2. SALIDA: Elegir orientación aleatoria, encontrar celda válida de salida, teletransportar
    /// 3. MOVIMIENTO: Caminar en línea recta, cambiar de estado al encontrar comida, retornar al nido
    /// </summary>
    public RoleDecision DecideAction(
        int id,
        Vector2 position,
        AntComponent ant,
        GridSystem grid,
        PheromoneGrid pheromones,
        ColonyTraits traits,
        Vector2 nestPosition)
    {
        Vector2 velocity = Vector2.Zero;
        AntState? newState = null;
        float newOrientation = ant.Orientation;
        var currentCell = grid.GetCell((int)position.X, (int)position.Y);

        // FASE 1: Esperando en el nido
        // La hormiga no se mueve hasta que termine su tiempo de espera (WaitTicksRemaining)
        if (currentCell.Type == CellType.Nest && ant.WaitTicksRemaining > 0)
        {
            velocity = Vector2.Zero;
            return new RoleDecision { Action = new AntAction { Velocity = velocity }, NewState = newState, NewOrientation = null };
        }

        // FASE 2: Saliendo del nido (primera vez)
        // Se ejecuta cuando: hormiga está en nido, tiempo de espera terminó, y no tiene orientación asignada
        if (currentCell.Type == CellType.Nest && ant.WaitTicksRemaining == 0 && ant.Orientation < 0)
        {
            // Generar orientación aleatoria entre 0 y 2π radianes
            newOrientation = (float)(Random.Shared.NextDouble() * Math.PI * 2);

            // Convertir orientación a vector dirección usando funciones trigonométricas
            Vector2 direction = new Vector2(
                MathF.Cos(newOrientation),
                MathF.Sin(newOrientation)
            );

            // Buscar la primera celda NO-NIDO en la dirección calculada
            // Comenzamos desde el centro del nido y avanzamos paso a paso
            Vector2 destPos = nestPosition;
            for (int step = 1; step < Math.Max(grid.Width, grid.Height); step++)
            {
                Vector2 searchPos = nestPosition + direction * step;
                int checkX = (int)searchPos.X;
                int checkY = (int)searchPos.Y;

                // Si salimos del grid, detener búsqueda
                if (checkX < 0 || checkX >= grid.Width || checkY < 0 || checkY >= grid.Height)
                    break;

                var checkCell = grid.GetCell(checkX, checkY);
                // Si encontramos una celda no-nido, usarla como destino y salir
                if (checkCell.Type != CellType.Nest)
                {
                    destPos = new Vector2(checkX, checkY);
                    break;
                }
            }

            // Solo salir del nido si encontramos una celda válida de salida
            // Si no hay salida en esa dirección, la hormiga se queda e intentará en el siguiente tick
            if (destPos != nestPosition)
            {
                velocity = Vector2.Zero;
                return new RoleDecision
                {
                    Action = new AntAction { Velocity = velocity },
                    NewOrientation = newOrientation,
                    NewPosition = destPos,
                    NewState = AntState.Exploring
                };
            }
        }

        // FASE 3: Movimiento por orientación
        // La hormiga camina en la dirección de su orientación si ya tiene una asignada (>= 0)
        if (newOrientation >= 0)
        {
            // Calcular velocidad como vector unitario en la dirección de orientación, escalado por velocidad de colonia
            velocity = new Vector2(
                MathF.Cos(newOrientation),
                MathF.Sin(newOrientation)
            ) * traits.Speed;

            // Agregar pequeña variación aleatoria a la orientación para movimiento serpenteante (±1°)
            float deltaRotation = (float)((Random.Shared.NextDouble() - 0.5) * 2 * Math.PI / 180);
            newOrientation += deltaRotation;
        }

        // Cambiar a estado RETURNING si la hormiga encuentra comida mientras explora
        if (ant.State == AntState.Exploring && currentCell.Type == CellType.Food)
        {
            newState = AntState.Returning;
        }

        // Cambiar a estado EXPLORING si la hormiga está cerca del nido mientras retorna
        // Distancia umbral: 3 unidades desde el centro del nido
        if (ant.State == AntState.Returning && Vector2.Distance(position, nestPosition) < 3f)
        {
            newState = AntState.Exploring;
        }

        // Muestreo de feromonas mientras explora (fuera del nido)
        // Busca en un radio 3x3 alrededor de la hormiga la concentración máxima de feromonas de comida
        if (ant.State == AntState.Exploring && currentCell.Type != CellType.Nest)
        {
            Vector2 pheromoneDirection = Vector2.Zero;
            float maxPheromone = 0f;

            // Iterar sobre todas las celdas en el radio 3x3
            for (int dx = -3; dx <= 3; dx++)
            {
                for (int dy = -3; dy <= 3; dy++)
                {
                    int nx = (int)position.X + dx;
                    int ny = (int)position.Y + dy;

                    // Ignorar celdas fuera del grid
                    if (nx < 0 || nx >= grid.Width || ny < 0 || ny >= grid.Height)
                        continue;

                    // Obtener concentración de feromona de comida en esta celda
                    float pheromone = pheromones.GetPheromone(nx, ny, ant.ColonyId, PheromoneType.Food);

                    // Rastrear celda con máxima concentración de feromona
                    if (pheromone > maxPheromone)
                    {
                        maxPheromone = pheromone;
                        Vector2 dir = new Vector2(nx - position.X, ny - position.Y);
                        // Validar que el vector dirección sea significativo (evitar vectores muy cortos)
                        if (dir.LengthSquared() > 0.1f)
                        {
                            pheromoneDirection = Vector2.Normalize(dir);
                        }
                    }
                }
            }

            // Si encontramos un rastro de feromona fuerte, seguirlo
            // NOTA: Temporalmente deshabilitado (& false) para debug. Habilitar cuando sea necesario
            if (maxPheromone > 0.05f && pheromoneDirection.LengthSquared() > 0.1f && false)
            {
                newOrientation = MathF.Atan2(pheromoneDirection.Y, pheromoneDirection.X);
                velocity = pheromoneDirection * traits.Speed;
            }
        }
        // Comportamiento de retorno al nido
        // Cuando la hormiga está en estado RETURNING, calcula el vector hacia el nido y se mueve en esa dirección
        else if (ant.State == AntState.Returning)
        {
            Vector2 direction = nestPosition - position;
            // Si está lo suficientemente lejos del nido, moverse hacia él
            if (direction.LengthSquared() > 1f)
            {
                velocity = Vector2.Normalize(direction) * traits.Speed;
                newOrientation = MathF.Atan2(direction.Y, direction.X);
            }
            else
            {
                // Si está muy cerca, detener el movimiento
                velocity = Vector2.Zero;
            }
        }

        // Retornar la decisión con velocidad calculada, cambios de estado y orientación
        // newOrientation solo será asignado por BehaviorSystem si es diferente de null
        return new RoleDecision { Action = new AntAction { Velocity = velocity }, NewState = newState, NewOrientation = newOrientation };
    }
}
