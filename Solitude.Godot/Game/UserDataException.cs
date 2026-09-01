using System;
using System.Text.Json;
using Godot;


public static class UserDataException
{
    public static Exception ReadFailed(string path, Error error)
    {
        return new System.IO.FileLoadException(
            $"Could not read file '{path}': {error}");
    }

    public static Exception WriteFailed(string path, Error error)
    {
        return new System.IO.IOException(
            $"Could not write file '{path}': {error}");
    }

    public static Exception CreateDirectoryFailed(string path, Error error)
    {
        return new System.IO.IOException(
            $"Could not create directory at '{path}': {error}"
        );
    }

    public static Exception ReplaceFailed(
        string tempPath,
        string savePath,
        Error error)
    {
        return new System.IO.IOException(
            $"Could not replace file '{savePath}' with '{tempPath}': {error}");
    }

    public static Exception Null(string path)
    {
        return new System.IO.InvalidDataException(
            $"Could not load file '{path}': The file is null");
    }

    public static Exception DeserialisationFailed(string path, JsonException innerException)
    {
        return new System.IO.InvalidDataException(
            $"Could not deserialize file '{path}'.",
            innerException);
    }
}
