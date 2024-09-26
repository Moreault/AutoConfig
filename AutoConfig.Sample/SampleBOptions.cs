namespace AutoConfig.Sample;

[AutoConfig("Group.SampleB")]
public sealed record SampleBOptions
{
    public string Text { get; init; }
}