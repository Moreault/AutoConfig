namespace AutoConfig.Sample;

[AutoConfig("Group:SampleA")]
public sealed record SampleAOptions
{
    public string Text { get; init; } = string.Empty;
}
