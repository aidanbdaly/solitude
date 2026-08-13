using Godot;
using System;
using System.Linq;
using Solitude.Domain.Game;
using Solitude.Domain.Game.Agents;
using Solitude.Domain.Game.Generation;
using Solitude.Nodes.Game;
using Solitude.Nodes.Menu;

namespace Solitude;

public partial class Main : Node
{
    [Export] public PackedScene MenuScene { get; set; } = null!;
    [Export] public PackedScene GameScene { get; set; } = null!;

    private readonly WorldGenerator _worldGenerator = new();
    private Node? _currentScreen;

    public override void _Ready()
    {
        GetWindow().Title = "Solitude";
        if (OS.GetCmdlineUserArgs().Contains("--skip-menu")) StartNewGame();
        else ShowMenu();
    }

    private void ShowMenu()
    {
        var menu = MenuScene.Instantiate<MenuScreen>();
        menu.BeginRequested += StartNewGame;
        ReplaceScreen(menu);
    }

    private void StartNewGame()
    {
        var state = _worldGenerator.Generate(
            WorldGeneratorParameters.Default,
            Random.Shared.Next(),
            AgentDefinition.Colonist);
        ShowGame(state);
    }

    private void ShowGame(State state)
    {
        var game = GameScene.Instantiate<GameNode>();
        game.Initialize(state);
        ReplaceScreen(game);
    }

    private void ReplaceScreen(Node next)
    {
        _currentScreen?.QueueFree();
        _currentScreen = next;
        AddChild(next);
    }
}
