using System;
using System.IO;

namespace Solitude.Persistence;

public static class GameFactory
{
    public static GameSnapshot ToSnapshot(this Game game)
        => new(
            GameSnapshot.CurrentVersion,
            game.World.ToSnapshot(),
            game.ActiveWorldCoordinate);

    internal static Game ToGame(this GameSnapshot snapshot)
    {
        SnapshotValidation.Require(snapshot is not null, "Game snapshot is missing");
        SnapshotValidation.Require(
            snapshot.Version == GameSnapshot.CurrentVersion,
            $"Unsupported game snapshot version '{snapshot.Version}'");
        SnapshotValidation.Require(snapshot.World is not null, "World snapshot is missing");

        var world = snapshot.World.ToWorld();
        var activeWorldCoordinate = snapshot.ActiveWorldCoordinate;

        if (activeWorldCoordinate is not null)
        {
            try
            {
                world.GetMap(activeWorldCoordinate.Value);
            }
            catch (Exception exception) when (exception is InvalidOperationException or ArgumentOutOfRangeException)
            {
                throw new InvalidDataException("Active world coordinate does not reference a restored map", exception);
            }
        }

        return new(new(), world, activeWorldCoordinate);
    }
}
