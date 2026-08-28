using System;

public sealed class Work(int required)
{
    private readonly int _required = required;

    private int _current = 0;

    public void Set(int amount)
    {
        _current = Math.Min(_required, amount);
    }

    public void Set(Func<int, int> setStateDelegate)
    {
        _current = Math.Min(_required, setStateDelegate(_current));
    }

    public int Get()
    {
        return _current;
    }

    public int GetRequired()
    {
        return _required;
    }

    public decimal GetFactor()
    {
        return decimal.Round(_current / _required, 2);
    }
}