public sealed class Work(int required)
{
    public int Required { get; } = required;

    public int Current { get; } = 0;

    public decimal GetFactor()
    {
        return decimal.Round(Current / Required, 2);
    }
}