using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Solitude.Domain.Game.Construction;
using Solitude.Domain.Game.Items;
using Solitude.Domain.Game.Map;
using Solitude.Domain.Game.Objects;

namespace Solitude.Domain.Game.Agents.Plans;

public static class PlanComposer
{
    public static Plan ComposeMove(Vector2I target) => new(new[]
    {
        new PlanStep(
            new MoveInstruction(new CellTarget(target), PathGoalMode.ExactCell),
            new Complete(),
            new Fail())
    });

    public static Plan ComposeDamage(
        MapObject target,
        float interval) => ComposeDamage(target.Id, target.Cell, interval);

    public static Plan ComposeConstruct(
        ConstructionSite site,
        int inventoryCapacity,
        float interval)
    {
        var requirementTypes = site.State.Requirements
            .Select(requirement => requirement.Type)
            .Distinct()
            .Where(type => !site.State.IsRequirementSatisfied(type))
            .ToArray();
        var steps = new List<PlanStep>();

        foreach (var type in requirementTypes)
        {
            var loopStart = steps.Count;
            var moveToSite = loopStart + 4;
            var maximumCount = Math.Min(site.State.GetMissingCount(type), inventoryCapacity);

            steps.Add(new PlanStep(
                new HasItemCondition(type),
                new GoTo(moveToSite),
                new Next()));
            steps.Add(new PlanStep(
                new ReserveItemInstruction(type, maximumCount),
                new Next(),
                new Fail()));
            steps.Add(new PlanStep(
                new MoveInstruction(new ReservedItemTarget(), PathGoalMode.ExactCell),
                new Next(),
                new Fail()));
            steps.Add(new PlanStep(
                new CollectReservedItemInstruction(),
                new Next(),
                new Fail()));
            steps.Add(new PlanStep(
                new MoveInstruction(new CellTarget(site.Cell), PathGoalMode.AdjacentToCell),
                new Next(),
                new Fail()));
            steps.Add(new PlanStep(
                new SupplyConstructionInstruction(site.Id, type),
                new Next(),
                new Fail()));
            steps.Add(new PlanStep(
                new ConstructionRequirementSatisfiedCondition(site.Id, type),
                new Next(),
                new GoTo(loopStart)));
        }

        var constructionStart = steps.Count;
        steps.Add(new PlanStep(
            new MoveInstruction(new CellTarget(site.Cell), PathGoalMode.AdjacentToCell),
            new Next(),
            new Fail()));
        steps.Add(new PlanStep(
            new AdvanceConstructionInstruction(site.Id, 1),
            new Next(),
            new Fail()));
        steps.Add(new PlanStep(
            new ConstructionSiteExistsCondition(site.Id),
            new Next(),
            new Complete()));
        steps.Add(new PlanStep(
            new WaitInstruction(interval),
            new GoTo(constructionStart + 1),
            new Fail()));

        return new Plan(steps);
    }

    private static Plan ComposeDamage(
        MapObjectId target,
        Vector2I cell,
        float interval) => new(new[]
    {
        new PlanStep(
            new MoveInstruction(new CellTarget(cell), PathGoalMode.AdjacentToCell),
            new Next(),
            new Fail()),
        new PlanStep(
            new DamageObjectInstruction(target, 1),
            new Next(),
            new Fail()),
        new PlanStep(
            new ObjectExistsCondition(target),
            new Next(),
            new Complete()),
        new PlanStep(
            new WaitInstruction(interval),
            new GoTo(1),
            new Fail())
    });

}
