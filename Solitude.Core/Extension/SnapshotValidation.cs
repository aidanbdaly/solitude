using System.Diagnostics.CodeAnalysis;

namespace Solitude.Persistence;

internal static class SnapshotValidation
{
    internal static void Require([DoesNotReturnIf(false)] bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidDataException(message);
        }
    }

    internal static IReadOnlyList<T> RequireList<T>(IReadOnlyList<T>? values, string name)
        => values ?? throw new InvalidDataException($"Snapshot collection '{name}' is missing");

    internal static bool Contains(Coordinate coordinate, int width, int height)
        => (uint)coordinate.X < (uint)width && (uint)coordinate.Y < (uint)height;
}
