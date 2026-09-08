using Solitude.Simulator.Core.Model;
using Solitude.Simulator.Mutation;
using Solitude.Simulator.Request;

namespace Solitude.Simulator.Service;


//return new(160, 160, 1, 0.08f, 0.115f, pallete);

public class ScenarioService(
    MapMutation map,
    EntityMutation entity
)
{
    private readonly MapMutation _map = map;
    private readonly EntityMutation _entity = entity;

    public void GenerateScenario(GenerateScenario request)
    {
        var map = new Map()
        {
            X = request.MapX,
            Y = request.MapY,
            Width = request.Width,
            Height = request.Height
        };

        var random = new Random(request.Seed);

        for (var y = 0; y < map.Height; y++)
        {
            for (var x = 0; x < map.Width; x++)
            {
                biome.EntityAt(x, y);
            }
        }
    }
}