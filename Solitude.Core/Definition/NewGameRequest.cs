
public sealed class NewGameRequest
{
    public required string Name;

    public required uint WorldWidth;
    public required uint WorldHeight;

    public required CreatePopulatedMapRequest CreatePopulatedMapRequest;
 
    public static NewGameRequest Default => new()
    {
        Name = "default",
        WorldWidth = 10,
        WorldHeight = 10,
        CreatePopulatedMapRequest = CreatePopulatedMapRequest.Default
    };
}
