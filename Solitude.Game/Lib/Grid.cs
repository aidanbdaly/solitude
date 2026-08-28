using System;
using System.Collections.Generic;
using Godot;

public sealed class GridChangedEvent : EventArgs
{
    public required Vector2I Coordinate { get; init; }
};

public sealed class Grid<T>(uint width, uint height) : IReadOnlyGrid<T> where T : struct
{
    public event EventHandler<GridChangedEvent>? GridChanged;

    private readonly T[] _cells =
        new T[checked((int)((ulong)width * height))];

    public void Set(Vector2I coordinate, T value)
    {
        _cells[GetIndex(coordinate)] = value;
        
        GridChanged?.Invoke(this, new()
        {
            Coordinate = coordinate
        });
    }

    public T Get(Vector2I coordinate)
    {
        return _cells[GetIndex(coordinate)];
    }

    public IEnumerator<(Vector2I coordinate, T value)> GetEnumerator()
    {
        var i = 0;

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                yield return (new Vector2I(x, y), _cells[i++]);
            }
        }
    }

    private int GetIndex(Vector2I coordinate)
    {
        if ((uint)coordinate.X >= width ||
            (uint)coordinate.Y >= height)
        {
            throw new ArgumentOutOfRangeException(nameof(coordinate));
        }

        return checked((int)((ulong)(uint)coordinate.Y * width +
                                 (uint)coordinate.X));
    }
}
