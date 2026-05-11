using System.Reflection;

namespace AutoConfig.Tests;

[TestClass]
public class AddAutoConfigTests
{
    private static IConfiguration BuildConfiguration() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Simple:Name"] = "Roger",
                ["Simple:Count"] = "42",
                ["Outer:Inner:Value"] = "nested!",
                ["Validated:RequiredField"] = "ok",
                ["DefaultsTarget:RequiredField"] = "ok",
                ["External:Description"] = "from-assembly",
                ["MarkerA:Tag"] = "a",
                ["MarkerB:Tag"] = "b",
            })
            .Build();

    private static ServiceProvider BuildProvider(IConfiguration? configuration = null, AutoConfigOptions? options = null)
    {
        var services = new ServiceCollection();
        services.AddAutoConfig(Assembly.GetExecutingAssembly(), configuration ?? BuildConfiguration(), options);
        return services.BuildServiceProvider();
    }

    [TestMethod]
    public void WhenServicesIsNull_Throw()
    {
        IServiceCollection services = null!;
        var action = () => services.AddAutoConfig(Assembly.GetExecutingAssembly(), BuildConfiguration());
        action.Should().Throw<ArgumentNullException>().WithParameterName(nameof(services));
    }

    [TestMethod]
    public void WhenAssemblyIsNull_Throw()
    {
        var services = new ServiceCollection();
        Assembly assembly = null!;
        var action = () => services.AddAutoConfig(assembly, BuildConfiguration());
        action.Should().Throw<ArgumentNullException>().WithParameterName(nameof(assembly));
    }

    [TestMethod]
    public void WhenConfigurationIsNull_Throw()
    {
        var services = new ServiceCollection();
        IConfiguration configuration = null!;
        var action = () => services.AddAutoConfig(Assembly.GetExecutingAssembly(), configuration);
        action.Should().Throw<ArgumentNullException>().WithParameterName(nameof(configuration));
    }

    [TestMethod]
    public void WhenAnnotatedTypeExists_BindSection()
    {
        var provider = BuildProvider();
        var options = provider.GetRequiredService<IOptions<SimpleOptions>>().Value;
        options.Name.Should().Be("Roger");
        options.Count.Should().Be(42);
    }

    [TestMethod]
    public void WhenSectionPathUsesColon_BindNestedSection()
    {
        var provider = BuildProvider();
        var options = provider.GetRequiredService<IOptions<NestedOptions>>().Value;
        options.Value.Should().Be("nested!");
    }

    [TestMethod]
    public void WhenAttributeRequestsValidation_ThrowOnInvalidValue()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Simple:Name"] = "Roger",
                ["Simple:Count"] = "42",
                ["Outer:Inner:Value"] = "nested!",
                ["Validated:RequiredField"] = null,
                ["DefaultsTarget:RequiredField"] = "ok",
                ["External:Description"] = "x",
                ["MarkerA:Tag"] = "a",
                ["MarkerB:Tag"] = "b",
            })
            .Build();

        var provider = BuildProvider(configuration);
        var action = () => provider.GetRequiredService<IOptions<ValidatedOptions>>().Value;
        action.Should().Throw<OptionsValidationException>();
    }

    [TestMethod]
    public void WhenOptionsDefaultsEnableValidation_ApplyToAttributesWithoutFlag()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Simple:Name"] = "Roger",
                ["Simple:Count"] = "42",
                ["Outer:Inner:Value"] = "nested!",
                ["Validated:RequiredField"] = "ok",
                ["DefaultsTarget:RequiredField"] = null,
                ["External:Description"] = "x",
                ["MarkerA:Tag"] = "a",
                ["MarkerB:Tag"] = "b",
            })
            .Build();

        var provider = BuildProvider(configuration, new AutoConfigOptions { ValidateDataAnnotations = true });
        var action = () => provider.GetRequiredService<IOptions<OptionsHonoringDefaults>>().Value;
        action.Should().Throw<OptionsValidationException>();
    }

    [TestMethod]
    public void WhenAssemblyHasGenericAttribute_BindExternalType()
    {
        var provider = BuildProvider();
        var external = provider.GetRequiredService<IOptions<ExternalOptions>>().Value;
        external.Description.Should().Be("from-assembly");
    }
}
