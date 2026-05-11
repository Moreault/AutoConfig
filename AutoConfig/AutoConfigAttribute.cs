namespace ToolBX.AutoConfig;

[AttributeUsage(AttributeTargets.Class)]
public sealed class AutoConfigAttribute(string name) : AutoConfigAttributeBase(name)
{
    public override Type? TargetType => null;
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class AutoConfigAttribute<T>(string name) : AutoConfigAttributeBase(name) where T : class
{
    public override Type TargetType => typeof(T);
}

public abstract class AutoConfigAttributeBase(string name) : Attribute
{
    public string Name { get; } = name;
    public bool ValidateDataAnnotations { get; set; }
    public bool ValidateOnStart { get; set; }
    public abstract Type? TargetType { get; }
}
