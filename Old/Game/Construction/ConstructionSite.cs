using Godot;
using System;

namespace Solitude.Domain.Game.Construction;

public sealed class ConstructionSite
{
    public ConstructionSiteId Id { get; internal set; }
    public Vector2I Cell { get; internal set; }
    public BuildingType BuildingType { get; }
    public ConstructionState State { get; }

    public ConstructionSite(BuildingType buildingType, ConstructionState state)
    {
        if (!Enum.IsDefined(buildingType))
            throw new ArgumentOutOfRangeException(nameof(buildingType), buildingType, null);
        BuildingType = buildingType;
        State = state ?? throw new ArgumentNullException(nameof(state));
    }
}
