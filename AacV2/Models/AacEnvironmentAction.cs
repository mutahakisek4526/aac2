namespace AacV2.Models;

public enum EnvironmentActionType
{
    PcCommand,
    OpenUrl
}

public sealed class AacEnvironmentAction
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = "一般";
    public EnvironmentActionType ActionType { get; set; }
    public string Payload { get; set; } = string.Empty;
}
