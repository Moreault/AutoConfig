![AutoConfig](https://github.com/Moreault/AutoConfig/blob/master/autoconfig.png)

# AutoConfig
A .NET library to make it easier to use appsettings sections using [AutoConfig] attributes directly on classes.

## Prerequisites

- .NET 10 or later

## How does it work?

You write a configuration type, as you normally would except that you add a `[AutoConfig]` attribute with a `string` on top of it.

```cs
[AutoConfig("MyConfig")]
public sealed record Configuration
{
    public string Name { get; init; }
    public bool IsAwesome { get; init; }
}
```

The `string` is the section name inside your `appsettings.json` file.

```json
{
    "MyConfig": {
        "name": "Roger",
        "isAwesome": true
    }
}
```

### Nested sections

Section paths use the standard .NET `:` separator:

```cs
[AutoConfig("Outer:Inner")]
public sealed record NestedOptions { /* ... */ }
```

## Getting started

In order for your configuration to be injected as an `IOptions<T>`, you need to call the following method where you configure your application services :

```cs
services.AddAutoConfig(configuration);
```

Alternatively, you can also specify which assembly to use :

```cs
services.AddAutoConfig(Assembly.GetExecutingAssembly(), configuration);
```

The parameterless overload walks every loaded assembly and invokes the registration code generated for each one that contains `[AutoConfig]` bindings. The `Assembly` overload skips that walk and registers a single assembly, so prefer it if you want to be explicit or shave off the assembly enumeration.

## How it works

As of version 4.0.0, AutoConfig uses a **Roslyn source generator** to produce the binding code at compile time instead of discovering attributed types through runtime reflection. For every `[AutoConfig]`-attributed class (or type bound via `AutoConfig<T>`) the generator emits a strongly-typed `services.AddOptions<T>().Bind(...)` call into your assembly. This means:

- No runtime assembly/type scanning to find attributed types
- No `MakeGenericMethod` / `MakeGenericType` calls (which are not compatible with Native AOT)
- Faster startup

`AddAutoConfig` then simply invokes the generated registration code for the relevant assemblies.

### Project setup

When you consume AutoConfig as a NuGet package, the source generator is included automatically — there's nothing to configure. When you reference the projects directly (e.g. inside this repository), reference the generator project alongside the main one:

```xml
<ProjectReference Include="..\AutoConfig\AutoConfig.csproj" />
<ProjectReference Include="..\AutoConfig.Generators\AutoConfig.Generators.csproj"
                  OutputItemType="Analyzer"
                  ReferenceOutputAssembly="false" />
```

### Native AOT and trimming

The `ToolBX.AutoConfig` assembly is marked `IsAotCompatible` and contains no AOT-hostile reflection. The actual reading of values out of `IConfiguration` is still performed by the standard `Microsoft.Extensions.Configuration` binder, which uses reflection; the generated registration method is therefore annotated with `[RequiresUnreferencedCode]` / `[RequiresDynamicCode]`. Because AutoConfig invokes that method across a reflection boundary, this requirement stays contained and does not bubble up to your `AddAutoConfig` call sites — but, as with any reflection-based configuration binding, keep your options types simple (or preserve them via trimming roots) when publishing trimmed or Native AOT.

## Binding types you don't own

When the configuration type lives in an assembly you can't modify (a third-party POCO, for example), use the generic form at the class level on a marker class, or directly on the assembly:

```cs
// In any file in your project:
[assembly: AutoConfig<SomeExternalOptions>("ThirdParty")]
```

The generic attribute supports `AllowMultiple = true`, so you can bind as many external types as you need.

## Validation

AutoConfig supports optional validation via `System.ComponentModel.DataAnnotations`. Enable it on a single class by setting `ValidateDataAnnotations` and/or `ValidateOnStart` on the attribute:

```cs
using System.ComponentModel.DataAnnotations;

[AutoConfig("MyConfig", ValidateDataAnnotations = true, ValidateOnStart = true)]
public sealed record Configuration
{
    [Required]
    public string Name { get; init; }

    [Range(1, 100)]
    public int MaxRetries { get; init; }
}
```

- `ValidateDataAnnotations` — validates properties using data annotation attributes such as `[Required]`, `[Range]`, `[StringLength]`, etc.
- `ValidateOnStart` — triggers validation when the application starts rather than on first access, causing the app to fail fast if configuration is invalid.

Both default to `false`, so existing usage is unaffected.

### Project-wide defaults

To opt every `[AutoConfig]` class in at once, pass an `AutoConfigOptions` to `AddAutoConfig`:

```cs
services.AddAutoConfig(configuration, new AutoConfigOptions
{
    ValidateDataAnnotations = true,
    ValidateOnStart = true,
});
```

Per-attribute flags are OR'd with the defaults — turning a flag on globally cannot be cancelled at the attribute level.

## Retrieving every bound option

Use `GetAutoConfigOptions<T>` to grab every registered options instance that is assignable to a given type (an interface or base type). Useful for cross-cutting features like diagnostics or plugin discovery:

```cs
public interface IFeatureOptions { bool Enabled { get; } }

[AutoConfig("Feature.A")]
public sealed record FeatureAOptions : IFeatureOptions { public bool Enabled { get; init; } }

[AutoConfig("Feature.B")]
public sealed record FeatureBOptions : IFeatureOptions { public bool Enabled { get; init; } }

// Later, given an IServiceProvider:
var enabled = serviceProvider.GetAutoConfigOptions<IFeatureOptions>()
    .Where(x => x.Enabled);
```
