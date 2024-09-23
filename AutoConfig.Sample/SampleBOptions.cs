namespace AutoConfig.Sample;

[ToolBX.AutoConfig.AutoConfig("Group.SampleB")]
public sealed record SampleBOptions
{
    public string Text { get; init; }
}