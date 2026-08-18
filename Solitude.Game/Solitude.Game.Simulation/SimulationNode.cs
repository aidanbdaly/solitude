using Godot;

public partial class SimulationNode : Node2D
{
    private SimulationService _simulationService = null!;


    public override void _Ready()
    {
        var camera = GetNode<Camera>("Camera");

        camera.Set("_surface", _simulationService.GetMapSize() * SimulationConstant.TileResolutionPX);

        camera.Center();

    }
}