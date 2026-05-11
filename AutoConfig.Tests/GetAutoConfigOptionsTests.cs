using System.Reflection;

namespace AutoConfig.Tests;

[TestClass]
public class GetAutoConfigOptionsTests
{
    [TestMethod]
    public void WhenServiceProviderIsNull_Throw()
    {
        IServiceProvider serviceProvider = null!;
        var action = () => serviceProvider.GetAutoConfigOptions<IMarker>();
        action.Should().Throw<ArgumentNullException>().WithParameterName(nameof(serviceProvider));
    }

    [TestMethod]
    public void WhenInvoked_ReturnAllAssignableBoundOptions()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Simple:Name"] = "Roger",
                ["Simple:Count"] = "1",
                ["Outer:Inner:Value"] = "v",
                ["Validated:RequiredField"] = "ok",
                ["DefaultsTarget:RequiredField"] = "ok",
                ["External:Description"] = "ext",
                ["MarkerA:Tag"] = "a",
                ["MarkerB:Tag"] = "b",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddAutoConfig(Assembly.GetExecutingAssembly(), configuration);
        var provider = services.BuildServiceProvider();

        var result = provider.GetAutoConfigOptions<IMarker>();

        result.Should().HaveCount(2);
        result.OfType<MarkerAOptions>().Should().ContainSingle().Which.Tag.Should().Be("a");
        result.OfType<MarkerBOptions>().Should().ContainSingle().Which.Tag.Should().Be("b");
    }
}
