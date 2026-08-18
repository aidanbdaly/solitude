public class Feature
{
    public FeatureId Id { get; internal set; }
    public FeatureType Type { get; }
    public int Health { get; private set; }

    protected Feature(FeatureId id, FeatureType type)
    {
        Id = id;
        Type = type;
    }

    internal void Damage(int amount)
    {
        if (amount <= 0) throw new System.ArgumentOutOfRangeException(nameof(amount));
        Health = System.Math.Max(0, Health - amount);
    }
}
