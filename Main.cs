using Godot;
using System;

using System.Linq;
using System.Text.Json;

namespace Solitude;

public partial class Main : Node
{
    [Export] public PackedScene MenuScene { get; set; } = null!;
    [Export] public PackedScene GameScene { get; set; } = null!;

    private Node? _currentScreen;

    public override void _Ready()
    {
        GetWindow().Title = "Solitude";
        if (OS.GetCmdlineUserArgs().Contains("--skip-menu")) NewGame();
        else ShowMenu();
    }

    private void ShowMenu()
    {
        var menu = MenuScene.Instantiate<MenuScene>();

        menu.NewGameRequested += NewGame;
        menu.LoadGameRequested += LoadGame;

        ReplaceScreen(menu);
    }

    private void LoadGame(string savePath)
    {
        if (!FileAccess.FileExists(savePath))
        {
            throw new Exception("The save file could not be found");
        }

        // revert tree

        using var saveFile = FileAccess.Open(savePath, FileAccess.ModeFlags.Read)
            ?? throw new Exception($"Error opening save file: {FileAccess.GetOpenError()}");

        GameState save = JsonSerializer.Deserialize<GameState>(saveFile.GetAsText())
            ?? throw new Exception("The save file is corrupted");

        var game = GameScene.Instantiate<GameScene>();

        game.Set("State", save);

        ReplaceScreen(game);
    }

    private void NewGame(string savePath, GameDefinition definition)
    {
        if (FileAccess.FileExists(savePath))
        {
            throw new Exception("The save file already exists");
        }

        GameState save = definition.ToState();

        using var saveFile = FileAccess.Open(savePath, FileAccess.ModeFlags.Write);

        saveFile.StoreString(JsonSerializer.Serialize(save));

        var game = GameScene.Instantiate<GameScene>();

        game.Set("State", save);

        ReplaceScreen(game);
    }

    private void ReplaceScreen(Node next)
    {
        _currentScreen?.QueueFree();
        _currentScreen = next;
        AddChild(next);
    }
}
