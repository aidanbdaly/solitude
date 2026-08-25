public sealed record AgentParameters
{
    public required string Name;
    public required AgentDrive Drive;
    public required AgentStature Stature;

    public static AgentParameters Colonist { get; } = new()
    {
        Name = "Colonist",
        Drive = AgentDrive.Acceptable,
        Stature = AgentStature.HugeHugo
    };
}