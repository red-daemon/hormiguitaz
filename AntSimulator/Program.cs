using System.Numerics;
using AntSimulator.Agents;
using AntSimulator.Core;
using AntSimulator.Lab;
using AntSimulator.Visualization;
using Raylib_CsLo;

if (args.Contains("--debug-explore"))
{
    // Modo laboratorio: mundo 3x3 con hormiga
    Vector2 antPosition = new Vector2(1, 1);
    float initialOrientation = 45f * MathF.PI / 180f;
    AntState antState = AntState.Exploring;

    // Cargar escenario si se especifica
    int scenarioIndex = Array.IndexOf(args, "--scenario");
    if (scenarioIndex >= 0 && scenarioIndex + 1 < args.Length)
    {
        var labScenario = LabConfigLoader.LoadScenario(args[scenarioIndex + 1]);
        if (labScenario != null)
        {
            antPosition = labScenario.Position;
            initialOrientation = labScenario.Orientation;
            antState = labScenario.State;
            Console.WriteLine($"Escenario: {labScenario.Description}");
        }
    }

    var world = LabWorldHelper.CreateExploringAntWorld(
        antPosition: antPosition,
        initialOrientation: initialOrientation,
        state: antState
    );

    var renderer = new RaylibRenderer(world, gridWidth: 3, gridHeight: 3, cellPixelSize: 200);

    var deltaTime = 0.016f;
    while (!Raylib.WindowShouldClose())
    {
        // R para reiniciar
        if (Raylib.IsKeyPressed(KeyboardKey.KEY_R))
        {
            world = LabWorldHelper.CreateExploringAntWorld(
                antPosition: antPosition,
                initialOrientation: initialOrientation,
                state: antState
            );
            (renderer as RaylibRenderer)?.SetWorld(world);
        }

        world.Update(deltaTime);
        renderer.Render();
    }

    Raylib.CloseWindow();
}
else
{
    // Modo normal: simulación 100x100
    var engine = new SimulationEngine(gridWidth: 100, gridHeight: 100, antCount: 1, cellPixelSize: 10);
    engine.Run();
}
