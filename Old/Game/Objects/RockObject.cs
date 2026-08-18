using System.Collections.Generic;
using Solitude.Domain.Game.Items;

namespace Solitude.Domain.Game.Objects;

public sealed class RockObject : MapObject
{
    public RockObject() : base(MapObjectType.Rock, 5, false) { }
    public override IReadOnlyList<ItemStack> GetDrops() => new[] { new ItemStack(ItemType.Stone, 16) };
}
