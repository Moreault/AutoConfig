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

## Getting started

In order for your configuration to be injected as an `IOptions<T>`, you need to call the following method where you configure your application services :

```cs
services.AddAutoConfig(configuration);
```

Alternatively, you can also specify which assembly to use :

```cs
services.AddAutoConfig(Assembly.GetExecutingAssembly(), configuration);
```

The latter is more performant because the former will look through all your assemblies looking for anything with the `[AutoConfig]` attribute on it. It is more convenient but it comes at certain a cost. Use the `Assembly` overload if performance is a concern.

## Validation

AutoConfig supports optional validation via `System.ComponentModel.DataAnnotations`. Enable it by setting `ValidateDataAnnotations` and/or `ValidateOnStart` on the attribute:

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