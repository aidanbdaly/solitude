
using System.Collections.Generic;
using Godot;

public record CreatePopulatedMapRequest(
    Vector2I Coordinate,
    MapStyle MapStyle,
    List<AgentDefinition> Agents
)
{
    public static CreatePopulatedMapRequest Default => new(
        new(5, 5),
        MapStyle.Default,
        [
            AgentDefinition.Colonist,
            AgentDefinition.Colonist,
            AgentDefinition.Colonist
        ]);
}
