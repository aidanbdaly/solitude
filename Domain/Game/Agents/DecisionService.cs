using System;
using Solitude.Domain.Game.Agents.Activity;
using Solitude.Domain.Game.Orders;
using Solitude.Domain.Game;

namespace Solitude.Domain.Game.Agents;

public sealed class DecisionService
{
	private readonly World _world;
	private readonly ActivityService _activityService;

	public DecisionService(
		World world,
		ActivityService activityService)
	{
		_world = world;
		_activityService = activityService;
	}

	public void Step()
	{
		foreach (var agent in _world.Agents)
		{
			if (agent.Activity is not null) continue;
            if (_world.TryGetOrderAssignmentForAgent(agent.Id, out _))
				throw new InvalidOperationException(
					$"Idle agent {agent.Id.Value} retains an order assignment.");
			Deliberate(agent);
		}
	}

	private void Deliberate(Agent agent)
	{
        foreach (var order in _world.UnassignedOrders)
		{
			var activity = CreateActivityIfCapable(order, agent);
			if (activity is null) continue;
            _world.AssignOrder(order.Id, agent.Id);
			_activityService.Start(agent, activity);
			return;
		}
	}

	private AgentActivity? CreateActivityIfCapable(
		Order order,
		Agent agent)
	{
		switch (order)
		{
			case DamageOrder damage:
			{
				var target = _world.GetObject(damage.Target);
				var rate = agent.Definition.DamageActionsPerSecond;
				return rate > 0f
					? ActivityComposer.ComposeDamage(target, 1f / rate)
					: null;
			}
			case ConstructOrder construct:
			{
				var site = _world.GetConstructionSite(construct.Target);
				var definition = agent.Definition;
				if (definition.ConstructionActionsPerSecond <= 0f
					|| (!site.State.IsFullySupplied && definition.InventoryCapacity <= 0))
					return null;
				return ActivityComposer.ComposeConstruct(
					site,
					definition.InventoryCapacity,
					1f / definition.ConstructionActionsPerSecond);
			}
			default:
				throw new ArgumentOutOfRangeException(nameof(order), order, "Unknown order type.");
		}
	}
}
