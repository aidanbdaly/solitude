public sealed record GameSnapshot(
    int Version,
    WorldSnapshot World,
    Coordinate? ActiveWorldCoordinate)
{
    public const int CurrentVersion = 1;
}
