using System;
using System.Collections.Generic;

public interface IReadOnlyGrid<T> : IEnumerable<(Coordinate coordinate, T value)>
{
    public event EventHandler<GridChangedEvent>? GridChanged;

    T Get(Coordinate coordinate);
}
