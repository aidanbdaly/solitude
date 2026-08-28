using System;
using System.Collections.Generic;

public sealed class GridChangedEvent : EventArgs
{
    public required Coordinate Coordinate { get; init; }
};

public sealed class Grid<T>(uint width, uint height) : IReadOnlyGrid<T> where T : struct
{
    public event EventHandler<GridChangedEvent>? GridChanged;

    private readonly T[] _cells =
        new T[checked((int)((ulong)width * height))];

    public void Set(Coordinate coordinate, T value)
    {
        _cells[GetIndex(coordinate)] = value;
        
        GridChanged?.Invoke(this, new()
        {
            Coordinate = coordinate
        });
    }

    public T Get(Coordinate coordinate)
    {
        return _cells[GetIndex(coordinate)];
    }

    public IEnumerator<(Coordinate coordinate, T value)> GetEnumerator()
    {
        var i = 0;

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                yield return (new Coordinate(x, y), _cells[i++]);
            }
        }
    }

    private int GetIndex(Coordinate coordinate)
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
