using System;
using System.Collections.Generic;
using Godot;

public sealed class SparseGrid<T>(uint width, uint height) : IReadOnlySparseGrid<T> where T : class
{
    private readonly T?[] _cells =
        new T?[checked((int)((ulong)width * height))];

    public void Set(Vector2I coordinate, T value)
    {
        var cellId = GetIndex(coordinate);

        if (_cells[cellId] is not null)
        {
            throw new InvalidOperationException("Cannot set entity coordinate, cell is already occupied");
        }

        _cells[cellId] = value;
    }

    public IEnumerator<(Vector2I coordinate, T value)> GetEnumerator()
    {
        var i = 0;

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var value = _cells[i++];

                if (value is not null)
                {
                    yield return (new Vector2I(x, y), value);
                }
            }
        }
    }

    public T? Get(Vector2I coordinate)
    {
        return _cells[GetIndex(coordinate)];
    }

    public bool Remove(Vector2I coordinate)
    {
        var cellId = GetIndex(coordinate);

        var entity = _cells[cellId];

        if (entity is null)
        {
            return false;
        }

        _cells[cellId] = null;

        return true;
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
