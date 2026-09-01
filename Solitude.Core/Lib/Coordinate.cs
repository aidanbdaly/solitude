using System;

public readonly record struct Coordinate(int X, int Y)
{
    public static Coordinate Zero => new(0, 0);

    public static Coordinate One => new(1, 1);

    public static Coordinate Right => new(1, 0);

    public static Coordinate operator +(Coordinate left, Coordinate right) =>
        new(left.X + right.X, left.Y + right.Y);

    public static Coordinate operator -(Coordinate left, Coordinate right) =>
        new(left.X - right.X, left.Y - right.Y);

    public Coordinate Abs() => new(Math.Abs(X), Math.Abs(Y));
}
