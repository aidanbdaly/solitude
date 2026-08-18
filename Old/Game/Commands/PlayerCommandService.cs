using Godot;
using Solitude.Domain.Game.Construction;
using Solitude.Domain.Game.Agents;
using Solitude.Domain.Game.Agents.Plans;
using Solitude.Domain.Game.Map;
using Solitude.Domain.Game;

namespace Solitude.Domain.Game.Commands;

public sealed class PlayerCommandService
{
    private readonly World _world;

    public PlayerCommandService(World world)
    {
        _world = world;
    }

    public void Move(AgentId agentId, Vector2I cell)
    {
        if (!_world.TryGetAgent(agentId, out var agent)
            || !_world.TryFindPath(agent.Cell, cell, PathGoalMode.ExactCell, out _)) return;

        _world.ReplacePlan(agentId, new Plan(new PlanAction[]
        {
            new MoveToAction(cell)
        }));
    }

    public bool CanConstruct(Vector2I cell) => _world.CanPlaceConstruction(cell);

    public void Construct(Vector2I cell)
    {
        if (!CanConstruct(cell)) return;
        var siteId = _world.CreateConstructionSite(cell, BuildingType.Wall);
        _world.GetOrAddConstructOrder(siteId);
    }

    public bool CanDamage(Vector2I cell)
    {
        return _world.ObjectAt(cell) is { IsDestroyed: false };
    }

    public void Damage(Vector2I cell)
    {
        if (!CanDamage(cell) || _world.ObjectAt(cell) is not { } target) return;
        _world.GetOrAddDamageOrder(target.Id);
    }
}
