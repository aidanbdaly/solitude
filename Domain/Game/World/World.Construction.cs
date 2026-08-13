using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Solitude.Domain.Game.Construction;
using Solitude.Domain.Game.Items;
using Solitude.Domain.Game.Map;
using Solitude.Domain.Game.Objects;

namespace Solitude.Domain.Game;

public readonly record struct ConstructionAdvanceResult(Vector2I Cell, bool Completed);

public sealed partial class World
{
    private int _nextConstructionSiteId = 1;
    private readonly Dictionary<ConstructionSiteId, ConstructionSite> _constructionSites = new();
    private readonly Dictionary<Vector2I, ConstructionSiteId> _constructionSiteByCell = new();

    public IReadOnlyCollection<ConstructionSite> ConstructionSites => _constructionSites.Values;

    public bool CanPlaceConstruction(Vector2I cell) =>
        Grid.Contains(cell)
        && Grid[cell].TileType != TileType.Water
        && ObjectAt(cell) is null
        && ConstructionSiteAt(cell) is null
        && Agents.All(agent => agent.Cell != cell);

    public ConstructionSiteId CreateConstructionSite(Vector2I cell, BuildingType buildingType)
    {
        if (!CanPlaceConstruction(cell))
            throw new ArgumentException(
                $"Construction cannot be placed at cell {cell}.",
                nameof(cell));

        var site = CreateConstructionSite(buildingType);
        var id = new ConstructionSiteId(_nextConstructionSiteId++);
        site.Id = id;
        site.Cell = cell;
        _constructionSites.Add(id, site);
        _constructionSiteByCell.Add(cell, id);
        return id;
    }

    public bool TryGetConstructionSite(ConstructionSiteId id, out ConstructionSite site) =>
        _constructionSites.TryGetValue(id, out site!);

    public ConstructionSite GetConstructionSite(ConstructionSiteId id)
    {
        if (!TryGetConstructionSite(id, out var site))
            throw new KeyNotFoundException($"Construction site {id.Value} does not exist.");
        return site;
    }

    public ConstructionSite? ConstructionSiteAt(Vector2I cell) =>
        _constructionSiteByCell.TryGetValue(cell, out var id)
        && TryGetConstructionSite(id, out var site)
            ? site
            : null;

    public void CancelConstructionSite(ConstructionSiteId id)
    {
        var site = GetConstructionSite(id);
        _constructionSites.Remove(id);
        _constructionSiteByCell.Remove(site.Cell);
        foreach (var material in site.State.GetDepositedMaterials())
            CreateItem(site.Cell, material.Type, material.Count);
    }

    public ConstructionAdvanceResult AdvanceConstruction(ConstructionSiteId id, int amount)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));

        var site = GetConstructionSite(id);
        if (site.State.IsComplete)
            throw new InvalidOperationException($"Construction site {id.Value} is already complete.");
        if (!site.State.IsFullySupplied)
            throw new InvalidOperationException($"Construction site {id.Value} is not fully supplied.");

        var completedBuilding = amount >= site.State.ProgressRemaining
            ? CreateCompletedBuilding(site.BuildingType)
            : null;
        site.State.Advance(amount);
        if (!site.State.IsComplete)
            return new ConstructionAdvanceResult(site.Cell, Completed: false);

        FinalizeConstruction(
            site,
            completedBuilding
                ?? throw new InvalidOperationException(
                    $"Construction site {id.Value} completed without a building."));
        return new ConstructionAdvanceResult(site.Cell, Completed: true);
    }

    private static ConstructionSite CreateConstructionSite(BuildingType buildingType) =>
        buildingType switch
        {
            BuildingType.Wall => new ConstructionSite(
                buildingType,
                new ConstructionState(
                    new[] { new ItemRequirement(ItemType.Wood, 16) },
                    progressRequired: 5)),
            _ => throw new ArgumentOutOfRangeException(nameof(buildingType), buildingType, null)
        };

    private static MapObject CreateCompletedBuilding(BuildingType buildingType) =>
        buildingType switch
        {
            BuildingType.Wall => new WallObject(),
            _ => throw new ArgumentOutOfRangeException(nameof(buildingType), buildingType, null)
        };

    private void FinalizeConstruction(ConstructionSite site, MapObject completedBuilding)
    {
        var cell = site.Cell;
        _constructionSites.Remove(site.Id);
        _constructionSiteByCell.Remove(cell);
        CreateObject(cell, completedBuilding);
    }
}
