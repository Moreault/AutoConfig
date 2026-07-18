namespace ToolBX.AutoConfig;

public sealed record AutoConfigOptions
{
    public bool ValidateDataAnnotations { get; init; }
    public bool ValidateOnStart { get; init; }
}
