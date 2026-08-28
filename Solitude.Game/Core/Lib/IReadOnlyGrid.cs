using System;
using System.Collections.Generic;

public interface IReadOnlyGrid<T>
{
    public event EventHandler<GridChangedEvent>? GridChanged;

    T Get(Coordinate coordinate);
    IEnumerator<(Coordinate coordinate, T value)> GetEnumerator();
}
