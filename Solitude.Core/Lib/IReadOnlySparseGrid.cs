using System.Collections.Generic;

public interface IReadOnlySparseGrid<T>
{
    T? Get(Coordinate coordinate);
    IEnumerator<(Coordinate coordinate, T value)> GetEnumerator();
}
