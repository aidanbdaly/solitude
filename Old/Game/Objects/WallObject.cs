using System.Collections.Generic;
using Solitude.Domain.Game.Items;

namespace Solitude.Domain.Game.Objects;

public sealed class WallObject : MapObject
{
    public WallObject() : base(MapObjectType.Wall, 5, false) { }
    public override IReadOnlyList<ItemStack> GetDrops() =>
        new[] { new ItemStack(ItemType.Wood, 16) };
}
