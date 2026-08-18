using System.Collections.Generic;
using Solitude.Domain.Game.Items;

namespace Solitude.Domain.Game.Objects;

public sealed class FloraObject : MapObject
{
    public FloraObject() : base(MapObjectType.Flora, 5, false) { }
    public override IReadOnlyList<ItemStack> GetDrops() => new[] { new ItemStack(ItemType.Wood, 16) };
}
