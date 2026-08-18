using System;

public sealed record AgentDefinition
{
    public float MovementTilesPerSecond { get; }
    public int InventoryCapacity { get; }
    public float DamageActionsPerSecond { get; }
    public float ConstructionActionsPerSecond { get; }
    public float TirednessPerDay { get; }
    public float SatiationConsumedPerDay { get; }

    public AgentDefinition(
        float movementTilesPerSecond,
        int inventoryCapacity,
        float damageActionsPerSecond,
        float constructionActionsPerSecond,
        float tirednessPerDay,
        float satiationConsumedPerDay)
    {
        if (!float.IsFinite(movementTilesPerSecond) || movementTilesPerSecond <= 0f)
            throw new ArgumentOutOfRangeException(nameof(movementTilesPerSecond));
        if (inventoryCapacity < 0)
            throw new ArgumentOutOfRangeException(nameof(inventoryCapacity));
        if (!float.IsFinite(damageActionsPerSecond) || damageActionsPerSecond < 0f)
            throw new ArgumentOutOfRangeException(nameof(damageActionsPerSecond));
        if (!float.IsFinite(constructionActionsPerSecond) || constructionActionsPerSecond < 0f)
            throw new ArgumentOutOfRangeException(nameof(constructionActionsPerSecond));
        if (!float.IsFinite(tirednessPerDay) || tirednessPerDay < 0f)
            throw new ArgumentOutOfRangeException(nameof(tirednessPerDay));
        if (!float.IsFinite(satiationConsumedPerDay) || satiationConsumedPerDay < 0f)
            throw new ArgumentOutOfRangeException(nameof(satiationConsumedPerDay));

        MovementTilesPerSecond = movementTilesPerSecond;
        InventoryCapacity = inventoryCapacity;
        DamageActionsPerSecond = damageActionsPerSecond;
        ConstructionActionsPerSecond = constructionActionsPerSecond;
        TirednessPerDay = tirednessPerDay;
        SatiationConsumedPerDay = satiationConsumedPerDay;
    }

    public static AgentDefinition Colonist { get; } = new(
        movementTilesPerSecond: 4.25f,
        inventoryCapacity: 16,
        damageActionsPerSecond: 1f / 0.65f,
        constructionActionsPerSecond: 1f / 0.55f,
        tirednessPerDay: 0.5f,
        satiationConsumedPerDay: 0.5f);
}