public sealed record AgentDefinition(
    string Name,
    AgentType Type,
    AgentDrive Drive,
    AgentStature Stature)
{
    public static AgentDefinition Colonist => new("Colonist",
        AgentType.Human,
        AgentDrive.Acceptable,
        AgentStature.HugeHugo);

}