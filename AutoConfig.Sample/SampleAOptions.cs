namespace AutoConfig.Sample;

[AutoConfig("Group.SampleA")]
public sealed record SampleAOptions
{
    public string Text { get; init; }

}

[AutoConfig("foo")]
public sealed record Foo
{
    public Bar1 Bar1 { get; init; }
    public Bar2 Bar2 { get; init; }
}

public sealed record Bar1
{

}

public sealed record Bar2
{

}