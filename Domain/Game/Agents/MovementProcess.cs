using Solitude.Domain.Game;
using System.Linq;

namespace Solitude.Domain.Game.Agents;

public sealed class MovementProcess
{
    private const float PositionEpsilonSquared = 0.0001f;
    private readonly World _world;

    public MovementProcess(World world)
    {
        _world = world;
    }

    public void Step(float delta)
    {
        foreach (var agent in _world.Agents)
        {
            var navigation = agent.Navigation;
            if (navigation.Status != AgentNavigationStatus.Moving)
            {
                agent.Position = agent.Cell;
                continue;
            }

            var remainingDistance = agent.Definition.MovementTilesPerSecond * delta;
            while (remainingDistance > 0f && navigation.Status == AgentNavigationStatus.Moving)
            {
                if (navigation.Path.Count == 0)
                {
                    navigation.Succeed();
                    break;
                }

                var next = navigation.Path.Peek();
                if (agent.Position.DistanceSquaredTo(agent.Cell) <= PositionEpsilonSquared
                    && !_world.CanStep(agent.Cell, next))
                {
                    if (!TryRepath(agent)) navigation.Fail();
                    if (navigation.Status != AgentNavigationStatus.Moving) break;
                    continue;
                }

                var target = (Godot.Vector2)next;
                var distance = agent.Position.DistanceTo(target);
                if (distance > remainingDistance)
                {
                    agent.Position = agent.Position.MoveToward(target, remainingDistance);
                    break;
                }

                agent.Position = target;
                agent.Cell = next;
                navigation.Path.Dequeue();
                remainingDistance -= distance;
                if (navigation.Path.Count == 0) navigation.Succeed();
            }
        }
    }

    private bool TryRepath(Agent agent)
    {
        var navigation = agent.Navigation;
        if (!_world.TryFindPath(agent.Cell, navigation.Target, navigation.GoalMode, out var path)) return false;
        navigation.ReplacePath(path.Skip(1));
        return true;
    }
}
