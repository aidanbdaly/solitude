using Godot;
using Solitude.Domain.Game.Items;
using Solitude.Domain.Game.Agents.Activity;

namespace Solitude.Domain.Game.Agents;

public sealed class Agent
{
	public required AgentId Id { get; init; }
	public required string Name { get; init; }
	public required Vector2I Cell { get; set; }
	public required Vector2 Position { get; set; }
	public required AgentDefinition Definition { get; init; }
	public AgentNavigation Navigation { get; } = new();
	public required Inventory Inventory { get; init; }
	public AgentActivity? Activity { get; private set; }

	internal void BeginActivity(AgentActivity activity) => Activity = activity;
	internal void ClearActivity() => Activity = null;
}
