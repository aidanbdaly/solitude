public sealed class FeaturePlan
{
    public FeaturePlanId Id { get; internal set; }
    public FeatureType Type { get; }
    public Inventory Inventory { get; } = new();
    public int ProgressRemaining { get; private set; }

    public FeaturePlan(FeatureType type)
    {
        Type = type;
    }
}