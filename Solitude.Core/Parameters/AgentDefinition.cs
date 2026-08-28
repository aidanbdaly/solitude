public sealed record AgentDefinition
{
    public required string Name;
    public required AgentType Type;
    public required AgentDrive Drive;
    public required AgentStature Stature;

    public static AgentDefinition Colonist { get; } = new()
    {
        Name = "Colonist",
        Type = AgentType.Human,
        Drive = AgentDrive.Acceptable,
        Stature = AgentStature.HugeHugo
    };
}