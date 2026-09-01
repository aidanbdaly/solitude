using System.Collections.Generic;

public interface IReadOnlySparseGrid<T> : IEnumerable<(Coordinate coordinate, T value)>
{
    T? Get(Coordinate coordinate);
}
