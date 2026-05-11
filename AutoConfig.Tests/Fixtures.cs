using System.Reflection;

namespace AutoConfig.Tests;

[AutoConfig("Simple")]
public sealed record SimpleOptions
{
    public string Name { get; init; } = string.Empty;
    public int Count { get; init; }
}

[AutoConfig("Outer:Inner")]
public sealed record NestedOptions
{
    public string Value { get; init; } = string.Empty;
}

[AutoConfig("Validated", ValidateDataAnnotations = true)]
public sealed record ValidatedOptions
{
    [Required]
    public string? RequiredField { get; init; }
}

[AutoConfig("DefaultsTarget")]
public sealed record OptionsHonoringDefaults
{
    [Required]
    public string? RequiredField { get; init; }
}

public sealed record ExternalOptions
{
    public string Description { get; init; } = string.Empty;
}

public interface IMarker;

[AutoConfig("MarkerA")]
public sealed record MarkerAOptions : IMarker
{
    public string Tag { get; init; } = string.Empty;
}

[AutoConfig("MarkerB")]
public sealed record MarkerBOptions : IMarker
{
    public string Tag { get; init; } = string.Empty;
}
