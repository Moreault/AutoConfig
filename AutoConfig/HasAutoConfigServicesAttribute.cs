namespace ToolBX.AutoConfig;

/// <summary>
/// Emitted by the AutoConfig source generator onto any assembly that contains at least one
/// <see cref="AutoConfigAttribute"/> (or <see cref="AutoConfigAttribute{T}"/>) binding. Used at runtime to
/// discover which loaded assemblies have generated registration code.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class HasAutoConfigServicesAttribute : Attribute;
