 using Godot;
using Solitude.Persistence;

public partial class App : Node
{
    [Export] public PackedScene MenuScene { get; set; } = null!;
    [Export] public PackedScene GameScene { get; set; } = null!;

    public void NewGame(NewGameRequest request)
    {
        var game = new Game(request.WorldWidth, request.WorldHeight);
        game.CreatePopulatedMap(request.CreatePopulatedMapRequest);
        game.SetActiveMap(request.CreatePopulatedMapRequest.Coordinate);

        UserData.SaveGame(request.Name, game);

        EnterGame(game);
    }

    public void LoadGame(string slot) => EnterGame(UserData.LoadGame(slot));

    public void QuitGame()
    {
        GetChild(0)?.QueueFree();
        GetTree().Quit();
    }

    public void EnterGame(Game state)
    {
        var game = GameScene.Instantiate<GameScene>();

        SwapRoot(game);

        game.Bind(state);
    }

    public void EnterMenu()
    {
        var menu = MenuScene.Instantiate<MenuScene>();

        menu.NewGameRequested += NewGame;
        menu.LoadGameRequested += LoadGame;
        menu.QuitGameRequested += QuitGame;

        SwapRoot(menu);
    }

    private void SwapRoot(Node next)
    {
        GetChild(0)?.QueueFree();
        AddChild(next);
    }
}
