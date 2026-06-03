using AntSimulator.Core;

var engine = new SimulationEngine(gridWidth: 100, gridHeight: 100, antCount: 100, cellPixelSize: 10);
engine.Run();
