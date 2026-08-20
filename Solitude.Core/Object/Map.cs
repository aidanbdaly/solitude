using System;
using System.Collections.Generic;
using Godot;

public sealed class Map(uint width, uint height)
{
    public int Width { get; private init; } = (int)width;
    public int Height { get; private init; } = (int)height;

    public Dictionary<Vector2I, Tile> Tile { get; private init; } = []; // Check Constraint, Vector2I is within width x height

    public Dictionary<ItemId, Item> Item { get; private init; } = []; // Unique Constraint ItemId, 
    public Dictionary<ItemId, Vector2I> ItemLocation { get; private init; } = []; // ItemIdUnique, ItemLocationUnique. Foreign Key ItemId, Vector2I
    public Dictionary<Vector2I, ItemId> ItemAt { get; private init; } = []; //  // ItemIdUnique, ItemLocationUnique ACROSS ITEMAT, FEATUREPLANAT, FEATUREAT, AGENT AT. Foreign Key ItemId, Vector2I INDEX

    public Dictionary<FeatureId, Feature> Feature { get; init; } = []; // Unique Constraint FeatureId,
    public Dictionary<FeatureId, Vector2I> FeatureLocation { get; init; } = []; // FeatureIdUnique, FeatureLocationUnique. Foreign Key FeatureId, Vector2I
    public Dictionary<Vector2I, FeatureId> FeatureAt { get; init; } = []; // FeatureIdUnique, FeatureLocationUnique ACROSS ITEMAT, FEATUREPLANAT, FEATUREAT, AGENT AT. Foreign Key FeatureId, Vector2I INDEX

    public Dictionary<FeaturePlanId, FeaturePlan> FeaturePlan { get; init; } = []; // Unique Constraint FeaturePlanId
    public Dictionary<FeaturePlanId, Vector2I> FeaturePlanLocation { get; init; } = []; // FeaturePlanId Unique, FeaturePlanLocationUnique. Foreign Key FeaturePlanId, Vector2I 
    public Dictionary<Vector2I, FeaturePlanId> FeaturePlanAt { get; init; } = []; // FeaturePlanId Unique, FeaturePlanLocationUnique ACROSS ITEMAT, FEATUREPLANAT, FEATUREAT, AGENT AT. Foreign Key FeaturePlanId, Vector2I INDEX

    public Dictionary<AgentId, Agent> Agent { get; init; } = []; // Unique Constraint AgentId
    public Dictionary<AgentId, AgentLocation> AgentLocation { get; init; } = []; // AgentLocationId Unique, AgentLocationId Unique, Foreign Key AgentId, Vector2I
    public Dictionary<Vector2I, AgentId> AgentAt { get; init; } = []; // AgentLocationId Unique, AgentLocationId Unique ACROSS ITEMAT, FEATUREPLANAT, FEATUREAT, AGENT AT, Foreign Key AgentId, Vector2I

    public int NextItemId { get; set; } = -1;
    public int NextFeatureId { get; set; } = -1;
    public int NextFeaturePlanId { get; set; } = -1;
    public int NextAgentId { get; set; } = -1;
}