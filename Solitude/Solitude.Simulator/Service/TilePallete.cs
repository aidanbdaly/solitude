using Solitude.Simulator.Core.Model;

public sealed record TilePallete
{
    private readonly Tile[] _band = new Tile[byte.MaxValue + 1];

    public Tile this[byte elevation]
    {
        get => _band[elevation];
        set
        {
            for (int i = elevation; i >= 0 && _band[i] != value; i--)
            {
                _band[i] = value;
            }
        }
    }
}