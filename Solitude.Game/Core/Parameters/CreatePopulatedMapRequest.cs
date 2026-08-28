
using System.Collections.Generic;

public record CreatePopulatedMapRequest(
    Coordinate Coordinate,
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
