using System.Collections.Generic;
using Godot;

public interface IReadOnlySparseGrid<T>
{
    T? Get(Vector2I coordinate);
    IEnumerator<(Vector2I coordinate, T value)> GetEnumerator();
}