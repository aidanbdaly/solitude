using Godot;
using Solitude.Domain.Game.Objects;

namespace Solitude.Domain.Game;

public enum SimulationEventType
{
    ObjectDamaged,
    ObjectDestroyed,
    ConstructionCompleted
}

public readonly record struct SimulationEvent(
    SimulationEventType Type,
    Vector2I Cell,
    MapObjectType? ObjectType = null);
