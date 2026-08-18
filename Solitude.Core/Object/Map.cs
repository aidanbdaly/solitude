using System.Collections.Generic;
using Godot;

public sealed class Map
{
    public required int Width { get; init; } // Constraint > 0;
    public required int Height { get; init; } // Constraint > 0;

    public required Dictionary<Vector2I, Tile> Tile { get; init; } // Check Constraint, Vector2I is within width x height

    public required Dictionary<ItemId, Item> Item { get; init; } // Unique Constraint ItemId, 
    public required Dictionary<ItemId, Vector2I> ItemLocation { get; init; } // ItemIdUnique, ItemLocationUnique. Foreign Key ItemId, Vector2I
    public Dictionary<Vector2I, ItemId> ItemAt { get; init; } = []; //  // ItemIdUnique, ItemLocationUnique ACROSS ITEMAT, FEATUREPLANAT, FEATUREAT, AGENT AT. Foreign Key ItemId, Vector2I INDEX

    public required Dictionary<FeatureId, Feature> Feature { get; init; } // Unique Constraint FeatureId,
    public required Dictionary<FeatureId, Vector2I> FeatureLocation { get; init; } // FeatureIdUnique, FeatureLocationUnique. Foreign Key FeatureId, Vector2I
    public Dictionary<Vector2I, FeatureId> FeatureAt { get; init; } = []; // FeatureIdUnique, FeatureLocationUnique ACROSS ITEMAT, FEATUREPLANAT, FEATUREAT, AGENT AT. Foreign Key FeatureId, Vector2I INDEX

    public required Dictionary<FeaturePlanId, FeaturePlan> FeaturePlan { get; init; } // Unique Constraint FeaturePlanId
    public required Dictionary<FeaturePlanId, Vector2I> FeaturePlanLocation { get; init; } // FeaturePlanId Unique, FeaturePlanLocationUnique. Foreign Key FeaturePlanId, Vector2I 
    public Dictionary<Vector2I, FeaturePlanId> FeaturePlanAt { get; init; } = []; // FeaturePlanId Unique, FeaturePlanLocationUnique ACROSS ITEMAT, FEATUREPLANAT, FEATUREAT, AGENT AT. Foreign Key FeaturePlanId, Vector2I INDEX


    public required Dictionary<AgentId, Agent> Agent { get; init; } // Unique Constraint AgentId
    public required Dictionary<AgentId, AgentLocation> AgentLocation { get; init; } // AgentLocationId Unique, AgentLocationId Unique, Foreign Key AgentId, Vector2I
    public Dictionary<Vector2I, AgentId> AgentAt { get; init; } = []; // AgentLocationId Unique, AgentLocationId Unique ACROSS ITEMAT, FEATUREPLANAT, FEATUREAT, AGENT AT, Foreign Key AgentId, Vector2I

    public required int NextItemId { get; set; }
    public required int NextFeatureId { get; set; }
    public required int NextFeaturePlanId { get; set; }
    public required int NextAgentId { get; set; }
}