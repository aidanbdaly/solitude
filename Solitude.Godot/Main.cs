using Godot;
using System;
using System.Linq;

namespace Solitude;

public partial class Main : Node
{
    private App app = null!;

    public override void _Ready()
    {
        GetWindow().Title = "Solitude";

        app = GetNode<App>("App");

        Run(OS.GetCmdlineUserArgs());
    }

    private void Run(string[] args)
    {
        if (args.Contains("--skip-menu"))
            app.NewGame(NewGameRequest.Default);
        else
            app.EnterMenu();
    }
}
