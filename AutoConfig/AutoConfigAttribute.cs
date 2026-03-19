namespace ToolBX.AutoConfig;

[AttributeUsage(AttributeTargets.Class)]
public class AutoConfigAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}