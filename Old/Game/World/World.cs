using System;
using Solitude.Domain.Game.Map;

namespace Solitude.Domain.Game;

public sealed partial class World
{
    public Grid Grid { get; }

    public World(Grid grid)
    {
        Grid = grid ?? throw new ArgumentNullException(nameof(grid));
    }
}
