using System;
using System.Collections.Generic;
using System.Linq;
using Solitude.Domain.Game.Items;

namespace Solitude.Domain.Game.Construction;

public sealed class ConstructionState
{
    private readonly IReadOnlyList<ItemRequirement> _requirements;

    public IReadOnlyList<ItemRequirement> Requirements => _requirements;
    public Inventory DeliveredMaterials { get; } = new();
    public int ProgressRequired { get; }
    public int ProgressRemaining { get; private set; }
    public bool IsComplete => ProgressRemaining <= 0;
    public bool IsFullySupplied => _requirements.All(requirement =>
        IsRequirementSatisfied(requirement.Type));
    public float SupplyProgress
    {
        get
        {
            var requirements = _requirements
                .GroupBy(requirement => requirement.Type)
                .Select(group => new ItemRequirement(group.Key, group.Sum(requirement => requirement.Count)))
                .ToArray();
            var required = requirements.Sum(requirement => requirement.Count);
            if (required <= 0) return 1f;
            var supplied = requirements.Sum(requirement =>
                Math.Min(requirement.Count, DeliveredMaterials.GetCount(requirement.Type)));
            return (float)supplied / required;
        }
    }
    public float ConstructionProgress => ProgressRequired <= 0
        ? 1f
        : 1f - (float)ProgressRemaining / ProgressRequired;

    public ConstructionState(IReadOnlyList<ItemRequirement> requirements, int progressRequired)
    {
        ArgumentNullException.ThrowIfNull(requirements);
        if (progressRequired <= 0)
            throw new ArgumentOutOfRangeException(nameof(progressRequired));
        _requirements = requirements.ToArray();
        ProgressRequired = ProgressRemaining = progressRequired;
    }

    public int GetRequiredCount(ItemType type) => _requirements
        .Where(requirement => requirement.Type == type)
        .Sum(requirement => requirement.Count);

    public int GetMissingCount(ItemType type) =>
        Math.Max(0, GetRequiredCount(type) - DeliveredMaterials.GetCount(type));

    public bool IsRequirementSatisfied(ItemType type) => GetMissingCount(type) == 0;

    internal int SupplyFrom(
        Inventory source,
        ItemType type,
        int maximumCount)
    {
        if (maximumCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(maximumCount));
        var missing = GetMissingCount(type);
        return missing > 0
            ? source.TransferTo(DeliveredMaterials, type, Math.Min(missing, maximumCount))
            : 0;
    }

    internal void Advance(int amount)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        if (!IsFullySupplied) throw new InvalidOperationException("Construction is not fully supplied.");
        if (IsComplete) throw new InvalidOperationException("Construction is already complete.");
        ProgressRemaining = Math.Max(0, ProgressRemaining - amount);
    }

    public IReadOnlyList<ItemStack> GetDepositedMaterials() => DeliveredMaterials.GetStacks();
}
