using AntSimulator.Core;

var engine = new SimulationEngine(gridWidth: 100, gridHeight: 100, antCount: 3, cellPixelSize: 10);
engine.Run();
