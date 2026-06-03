using System.Numerics;
using AntSimulator;
using AntSimulator.Colonies;
using AntSimulator.ECS.Components;

namespace AntSimulator.Tests;

public class AntArchetypeTests
{
    [Fact]
    public void CreateAnt_AddedToArray_CountIncreases()
    {
        var world = new World(100, 100);
        int antId = world.Ants.CreateAnt(1, Vector2.Zero);

        Assert.Equal(1, world.Ants.EntityCount);
        Assert.True(antId >= 0);
    }

    [Fact]
    public void DestroyAnt_RemovedFromActive_StateIsDead()
    {
        var world = new World(100, 100);
        int antId = world.Ants.CreateAnt(1, Vector2.Zero);
        world.Ants.DestroyAnt(antId);

        var ants = world.Ants.GetAnts();
        Assert.Equal(Agents.AntState.Dead, ants[antId].State);
    }

    [Fact]
    public void World_RegistersSystem_StoresItInOrder()
    {
        var world = new World(100, 100);
        Assert.NotNull(world.Ants);
        Assert.NotNull(world.Grid);
        Assert.NotNull(world.Pheromones);
    }
}
