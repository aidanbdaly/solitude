using System;
using System.Collections.Generic;
using Godot;

public interface IReadOnlyGrid<T>
{
    public event EventHandler<GridChangedEvent>? GridChanged;

    T Get(Vector2I coordinate);
    IEnumerator<(Vector2I coordinate, T value)> GetEnumerator();
}
