using System.ComponentModel.DataAnnotations;

namespace AutoConfig.Sample;

//Will look for the "Sample" section at the root of the appsettings.json file
[AutoConfig("Sample", ValidateDataAnnotations = true, ValidateOnStart = true)]
public record SampleOptions
{
    [Required]
    //This "Denied!" in red will be displayed if the project is unproperly configured
    public string Text { get; init; } = "<color red=255>Denied!</color>";
}