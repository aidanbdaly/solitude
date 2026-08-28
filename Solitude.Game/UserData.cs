using System;
using System.Text.Json;
using Godot;

using FileAccess = Godot.FileAccess;

namespace Solitude.Persistence;

public static class UserData
{
    public const string DefaultSlot = "default";

    private const string SaveDirectory = "user://saves";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static Game LoadGame(string slot)
    {
        var path = GetPath(slot);

        using var saveFile =
            FileAccess.Open(path, FileAccess.ModeFlags.Read)
            ?? throw UserDataException.ReadFailed(path, FileAccess.GetOpenError());

        GameSnapshot snapshot;

        try
        {
            snapshot =
                JsonSerializer.Deserialize<GameSnapshot>(
                    saveFile.GetAsText(),
                    JsonOptions)
                ?? throw UserDataException.Null(path);
        }
        catch (JsonException exception)
        {
            throw UserDataException.DeserialisationFailed(path, exception);
        }

        return snapshot.ToGame();
    }

    public static void SaveGame(string slot, Game game)
    {
        var directoryPath =
            ProjectSettings.GlobalizePath(SaveDirectory);

        var directoryError =
            DirAccess.MakeDirRecursiveAbsolute(directoryPath);

        if (directoryError != Error.Ok &&
            directoryError != Error.AlreadyExists)
        {
            throw UserDataException.CreateDirectoryFailed(
                directoryPath,
                directoryError);
        }

        var savePath = GetPath(slot);
        var tempPath = $"{savePath}.{Guid.NewGuid():N}.tmp";

        var json = JsonSerializer.Serialize(
            game.ToSnapshot(),
            JsonOptions);

        try
        {
            using (var saveFile =
                FileAccess.Open(
                    tempPath,
                    FileAccess.ModeFlags.Write)
                ?? throw UserDataException.WriteFailed(tempPath, FileAccess.GetOpenError()))
            {
                saveFile.StoreString(json);
                saveFile.Flush();

                var writeError = saveFile.GetError();

                if (writeError != Error.Ok)
                {
                    throw UserDataException.WriteFailed(
                        tempPath,
                        writeError);
                }
            }

            var replaceError =
                DirAccess.RenameAbsolute(
                    tempPath,
                    savePath);

            if (replaceError != Error.Ok)
            {
                throw UserDataException.ReplaceFailed(
                    tempPath,
                    savePath,
                    replaceError);
            }
        }
        finally
        {
            if (FileAccess.FileExists(tempPath))
            {
                DirAccess.RemoveAbsolute(tempPath);
            }
        }
    }

    private static string GetPath(string slot) => $"{SaveDirectory}/{slot}.json";
}
