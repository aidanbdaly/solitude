using Godot;
using Solitude.Domain.Game.Items;
using Solitude.Domain.Game.Agents.Plans;

namespace Solitude.Domain.Game.Agents;

public sealed class Agent
{
	public required AgentId Id { get; init; }
	public required string Name { get; init; }
	public required Vector2I Cell { get; set; }
	public required Vector2 Position { get; set; }
	public required AgentDefinition Definition { get; init; }
	public required AgentNeeds Needs { get; init; }
	public required AgentNavigation Navigation { get; init; } = new();
	public required Inventory Inventory { get; init; }
	public Plan? Plan { get; private set; }

	internal void BeginPlan(Plan plan) => Plan = plan;
	internal void ClearPlan() => Plan = null;
}
