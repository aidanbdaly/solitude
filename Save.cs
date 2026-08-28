
using System;
using System.Text.Json;
using Godot;

public static class Save
{
    public static Game Read(string savePath)
    {
        if (!FileAccess.FileExists(savePath))
        {
            throw new Exception("The save file could not be found");
        }

        using var saveFile = FileAccess.Open(savePath, FileAccess.ModeFlags.Read)
            ?? throw new Exception($"Error opening save file: {FileAccess.GetOpenError()}");

        Game save = JsonSerializer.Deserialize<Game>(saveFile.GetAsText())
            ?? throw new Exception("The save file is corrupted");

        return save;
    }

    public static void Write(string savePath, Game save)
    {
        using var saveFile = FileAccess.Open(savePath, FileAccess.ModeFlags.Write);

        saveFile.StoreString(JsonSerializer.Serialize(save));
    }

    public static bool Exists(string savePath)
    {
        return FileAccess.FileExists(savePath);
    }

}
