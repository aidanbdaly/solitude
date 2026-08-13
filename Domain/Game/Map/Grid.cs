using Godot;
using System;
using System.Collections.Generic;

namespace Solitude.Domain.Game.Map;

public sealed class Grid
{
    private readonly GridCell[] _cells;

    public int Width { get; }
    public int Height { get; }

    public Grid(int width, int height)
    {
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        Width = width;
        Height = height;
        _cells = new GridCell[width * height];
    }

    public GridCell this[Vector2I position] => _cells[ToIndex(position)];

    public IEnumerable<KeyValuePair<Vector2I, GridCell>> Cells
    {
        get
        {
            for (var y = 0; y < Height; y++)
            for (var x = 0; x < Width; x++)
            {
                var position = new Vector2I(x, y);
                yield return new KeyValuePair<Vector2I, GridCell>(position, this[position]);
            }
        }
    }

    public void SetCell(Vector2I position, TileType type, TileVariation variation) =>
        _cells[ToIndex(position)] = new GridCell(type, variation);

    public bool Contains(Vector2I position) =>
        position.X >= 0 && position.Y >= 0 && position.X < Width && position.Y < Height;

    public bool IsTerrainTraversable(Vector2I position) =>
        Contains(position) && this[position].TileType != TileType.Water;

    private int ToIndex(Vector2I position)
    {
        if (!Contains(position))
            throw new ArgumentOutOfRangeException(nameof(position), position, "Cell is outside the grid.");
        return position.Y * Width + position.X;
    }
}
