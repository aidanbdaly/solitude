using System;

namespace Solitude.Domain.Game.Map;

public static class GridMath
{
    public static readonly Coordinate[] EightWayDirections =
    {
        new(-1, -1), new(0, -1), new(1, -1), new(-1, 0),
        new(1, 0), new(-1, 1), new(0, 1), new(1, 1)
    };

    public static bool IsDiagonal(Coordinate direction) => direction.X != 0 && direction.Y != 0;
    public static float StepCost(Coordinate direction) => IsDiagonal(direction) ? 1.41421356f : 1f;

    public static bool IsAdjacent(Coordinate a, Coordinate b)
    {
        var delta = (a - b).Abs();
        return a != b && delta.X <= 1 && delta.Y <= 1;
    }

    public static float Octile(Coordinate a, Coordinate b)
    {
        var delta = (a - b).Abs();
        return Math.Max(delta.X, delta.Y) + 0.41421356f * Math.Min(delta.X, delta.Y);
    }

    public static int Manhattan(Coordinate a, Coordinate b)
    {
        var delta = (a - b).Abs();
        return delta.X + delta.Y;
    }
}
