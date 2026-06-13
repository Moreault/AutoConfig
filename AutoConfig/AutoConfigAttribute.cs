namespace ToolBX.AutoConfig;

/// <summary>
/// Binds the decorated class to the configuration section named <paramref name="name"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class AutoConfigAttribute(string name) : AutoConfigAttributeBase(name);

/// <summary>
/// Binds <typeparamref name="T"/> to the configuration section named <paramref name="name"/>. Use this form to bind a
/// type you don't own, either on a marker class or directly on the assembly.
/// </summary>
// ReSharper disable once UnusedTypeParameter : consumed by the source generator via the attribute's type argument
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class AutoConfigAttribute<T>(string name) : AutoConfigAttributeBase(name) where T : class;

public abstract class AutoConfigAttributeBase(string name) : Attribute
{
    public string Name { get; } = name;
    public bool ValidateDataAnnotations { get; set; }
    public bool ValidateOnStart { get; set; }
}
