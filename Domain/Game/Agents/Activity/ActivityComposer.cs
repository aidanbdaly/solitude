using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Solitude.Domain.Game.Construction;
using Solitude.Domain.Game.Items;
using Solitude.Domain.Game.Map;
using Solitude.Domain.Game.Objects;

namespace Solitude.Domain.Game.Agents.Activity;

public static class ActivityComposer
{
    public static AgentActivity ComposeMove(Vector2I target) => new(new[]
    {
        new ActivityStep(
            new MoveInstruction(new CellTarget(target), PathGoalMode.ExactCell),
            new Complete(),
            new Fail())
    });

    public static AgentActivity ComposeDamage(
        MapObject target,
        float interval) => ComposeDamage(target.Id, target.Cell, interval);

    public static AgentActivity ComposeConstruct(
        ConstructionSite site,
        int inventoryCapacity,
        float interval)
    {
        var requirementTypes = site.State.Requirements
            .Select(requirement => requirement.Type)
            .Distinct()
            .Where(type => !site.State.IsRequirementSatisfied(type))
            .ToArray();
        var steps = new List<ActivityStep>();

        foreach (var type in requirementTypes)
        {
            var loopStart = steps.Count;
            var moveToSite = loopStart + 4;
            var maximumCount = Math.Min(site.State.GetMissingCount(type), inventoryCapacity);

            steps.Add(new ActivityStep(
                new HasItemCondition(type),
                new GoTo(moveToSite),
                new Next()));
            steps.Add(new ActivityStep(
                new ReserveItemInstruction(type, maximumCount),
                new Next(),
                new Fail()));
            steps.Add(new ActivityStep(
                new MoveInstruction(new ReservedItemTarget(), PathGoalMode.ExactCell),
                new Next(),
                new Fail()));
            steps.Add(new ActivityStep(
                new CollectReservedItemInstruction(),
                new Next(),
                new Fail()));
            steps.Add(new ActivityStep(
                new MoveInstruction(new CellTarget(site.Cell), PathGoalMode.AdjacentToCell),
                new Next(),
                new Fail()));
            steps.Add(new ActivityStep(
                new SupplyConstructionInstruction(site.Id, type),
                new Next(),
                new Fail()));
            steps.Add(new ActivityStep(
                new ConstructionRequirementSatisfiedCondition(site.Id, type),
                new Next(),
                new GoTo(loopStart)));
        }

        var constructionStart = steps.Count;
        steps.Add(new ActivityStep(
            new MoveInstruction(new CellTarget(site.Cell), PathGoalMode.AdjacentToCell),
            new Next(),
            new Fail()));
        steps.Add(new ActivityStep(
            new AdvanceConstructionInstruction(site.Id, 1),
            new Next(),
            new Fail()));
        steps.Add(new ActivityStep(
            new ConstructionSiteExistsCondition(site.Id),
            new Next(),
            new Complete()));
        steps.Add(new ActivityStep(
            new WaitInstruction(interval),
            new GoTo(constructionStart + 1),
            new Fail()));

        return new AgentActivity(steps);
    }

    private static AgentActivity ComposeDamage(
        MapObjectId target,
        Vector2I cell,
        float interval) => new(new[]
    {
        new ActivityStep(
            new MoveInstruction(new CellTarget(cell), PathGoalMode.AdjacentToCell),
            new Next(),
            new Fail()),
        new ActivityStep(
            new DamageObjectInstruction(target, 1),
            new Next(),
            new Fail()),
        new ActivityStep(
            new ObjectExistsCondition(target),
            new Next(),
            new Complete()),
        new ActivityStep(
            new WaitInstruction(interval),
            new GoTo(1),
            new Fail())
    });

}
