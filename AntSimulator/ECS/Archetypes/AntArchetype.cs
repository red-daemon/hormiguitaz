using System.Numerics;
using AntSimulator.ECS.Components;

namespace AntSimulator.ECS.Archetypes;

public class AntArchetype
{
    private Vector2[] _positions;
    private Vector2[] _velocities;
    private AntComponent[] _ants;
    private PhysicsComponent[] _physics;

    private int _count;
    private int _aliveCount;
    private Queue<int> _freeIndices;
    private int _capacity;

    public int EntityCount => _aliveCount;

    public AntArchetype(int initialCapacity = 1024)
    {
        _capacity = initialCapacity;
        _positions = new Vector2[_capacity];
        _velocities = new Vector2[_capacity];
        _ants = new AntComponent[_capacity];
        _physics = new PhysicsComponent[_capacity];
        _count = 0;
        _aliveCount = 0;
        _freeIndices = new Queue<int>();
    }

    public int CreateAnt(int colonyId, Vector2 position)
    {
        int id;

        if (_freeIndices.Count > 0)
        {
            id = _freeIndices.Dequeue();
        }
        else
        {
            if (_count >= _capacity)
            {
                Resize(_capacity * 2);
            }
            id = _count;
            _count++;
        }

        _positions[id] = position;
        _velocities[id] = Vector2.Zero;
        _ants[id] = new AntComponent
        {
            ColonyId = colonyId,
            State = Agents.AntState.Idle,
            Energy = Constants.DEFAULT_ENERGY,
            TicksInState = 0
        };
        _physics[id] = new PhysicsComponent();
        _aliveCount++;

        return id;
    }

    public void DestroyAnt(int id)
    {
        if (id >= _capacity || _ants[id].State == Agents.AntState.Dead)
            return;

        _ants[id].State = Agents.AntState.Dead;
        _freeIndices.Enqueue(id);
        _aliveCount--;
    }

    public ReadOnlySpan<Vector2> GetPositions() => new ReadOnlySpan<Vector2>(_positions, 0, _capacity);
    public Span<Vector2> GetPositionsMutable() => new Span<Vector2>(_positions, 0, _capacity);

    public ReadOnlySpan<Vector2> GetVelocities() => new ReadOnlySpan<Vector2>(_velocities, 0, _capacity);
    public Span<Vector2> GetVelocitiesMutable() => new Span<Vector2>(_velocities, 0, _capacity);

    public ReadOnlySpan<AntComponent> GetAnts() => new ReadOnlySpan<AntComponent>(_ants, 0, _capacity);
    public Span<AntComponent> GetAntsMutable() => new Span<AntComponent>(_ants, 0, _capacity);

    public ReadOnlySpan<PhysicsComponent> GetPhysics() => new ReadOnlySpan<PhysicsComponent>(_physics, 0, _capacity);
    public Span<PhysicsComponent> GetPhysicsMutable() => new Span<PhysicsComponent>(_physics, 0, _capacity);

    private void Resize(int newCapacity)
    {
        Array.Resize(ref _positions, newCapacity);
        Array.Resize(ref _velocities, newCapacity);
        Array.Resize(ref _ants, newCapacity);
        Array.Resize(ref _physics, newCapacity);
        _capacity = newCapacity;
    }
}
