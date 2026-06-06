using System.Numerics;
using System.Text.Json;
using AntSimulator.Agents;

namespace AntSimulator.Lab;

/// <summary>
/// Representa un escenario de debug del lab.
/// </summary>
public class LabScenario
{
    public string Description { get; set; } = string.Empty;
    public Vector2 Position { get; set; }
    public float Orientation { get; set; }
    public AntState State { get; set; } = AntState.Exploring;
}

/// <summary>
/// Carga escenarios de debug desde lab-config.json
/// </summary>
public static class LabConfigLoader
{
    private static Dictionary<string, LabScenario>? _scenarios;

    public static LabScenario? LoadScenario(string scenarioName)
    {
        LoadAllScenarios();

        if (_scenarios!.TryGetValue(scenarioName, out var scenario))
            return scenario;

        Console.WriteLine($"Escenario '{scenarioName}' no encontrado.");
        Console.WriteLine("Disponibles:");
        foreach (var name in _scenarios.Keys)
            Console.WriteLine($"  - {name}");

        return null;
    }

    private static void LoadAllScenarios()
    {
        if (_scenarios != null) return;

        try
        {
            // Buscar lab-config.json en varios lugares
            string configPath = "lab-config.json";

            if (!File.Exists(configPath))
                configPath = Path.Combine(AppContext.BaseDirectory, "lab-config.json");

            if (!File.Exists(configPath))
                configPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "lab-config.json");

            if (!File.Exists(configPath))
            {
                Console.WriteLine($"No se encontró lab-config.json en: {Path.GetFullPath(".")}");
                _scenarios = new Dictionary<string, LabScenario>();
                return;
            }

            var json = File.ReadAllText(configPath);
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            _scenarios = new Dictionary<string, LabScenario>();

            if (root.TryGetProperty("scenarios", out var scenariosElement))
            {
                foreach (var prop in scenariosElement.EnumerateObject())
                {
                    var name = prop.Name;
                    var value = prop.Value;

                    float x = value.GetProperty("position").GetProperty("x").GetSingle();
                    float y = value.GetProperty("position").GetProperty("y").GetSingle();
                    float orient = value.GetProperty("orientation").GetSingle();
                    string desc = value.TryGetProperty("description", out var descProp)
                        ? descProp.GetString() ?? string.Empty
                        : string.Empty;

                    var state = AntState.Exploring;
                    if (value.TryGetProperty("state", out var stateProp) && stateProp.GetString() is string stateStr)
                    {
                        if (Enum.TryParse<AntState>(stateStr, ignoreCase: true, out var parsedState))
                            state = parsedState;
                    }

                    _scenarios[name] = new LabScenario
                    {
                        Description = desc,
                        Position = new Vector2(x, y),
                        Orientation = orient,
                        State = state
                    };
                }
            }

            Console.WriteLine($"Config cargado desde: {Path.GetFullPath(configPath)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando lab-config.json: {ex.Message}");
            _scenarios = new Dictionary<string, LabScenario>();
        }
    }
}
