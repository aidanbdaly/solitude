using Godot;
using System.Collections.Generic;
using Solitude.Domain.Game.Items;

namespace Solitude.Domain.Game.Objects;

public enum MapObjectType
{
	Wall,
	Rock,
	Flora
}

public abstract class MapObject
{
	public MapObjectId Id { get; internal set; }
	public Vector2I Cell { get; internal set; }
	public MapObjectType Type { get; }
	public int HitPoints { get; private set; }
	public int MaxHitPoints { get; }
	public bool IsWalkable { get; }
	public bool IsDestroyed => HitPoints <= 0;
	protected MapObject(MapObjectType type, int maxHitPoints, bool isWalkable)
	{
		if (maxHitPoints <= 0)
			throw new System.ArgumentOutOfRangeException(nameof(maxHitPoints));
		Type = type;
		HitPoints = MaxHitPoints = maxHitPoints;
		IsWalkable = isWalkable;
	}

	internal void Damage(int amount)
	{
		if (amount <= 0) throw new System.ArgumentOutOfRangeException(nameof(amount));
		if (IsDestroyed) throw new System.InvalidOperationException("A destroyed object cannot be damaged.");
		HitPoints = System.Math.Max(0, HitPoints - amount);
	}
	public abstract IReadOnlyList<ItemStack> GetDrops();
}
