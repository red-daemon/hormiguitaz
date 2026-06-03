namespace AntSimulator.Environment;

public class GridSystem
{
    private Cell[,] _grid;
    public int Width { get; }
    public int Height { get; }

    public GridSystem(int width, int height)
    {
        Width = width;
        Height = height;
        _grid = new Cell[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                _grid[x, y] = new Cell { Type = CellType.Empty };
            }
        }
    }

    public Cell GetCell(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return new Cell { Type = CellType.Wall };

        return _grid[x, y];
    }

    public void SetCell(int x, int y, Cell cell)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return;

        _grid[x, y] = cell;
    }

    public bool IsWalkable(int x, int y)
    {
        var cell = GetCell(x, y);
        return cell.Type is CellType.Empty or CellType.Food or CellType.Nest;
    }
}
