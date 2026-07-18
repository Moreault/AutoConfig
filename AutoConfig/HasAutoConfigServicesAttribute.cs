namespace ToolBX.AutoConfig;

/// <summary>
/// Emitted by the AutoConfig source generator onto any assembly that contains at least one
/// <see cref="AutoConfigAttribute"/> (or <see cref="AutoConfigAttribute{T}"/>) binding. Runtime registration
/// is performed via a module initializer that registers into <see cref="AutoConfigRegistry"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class HasAutoConfigServicesAttribute : Attribute;
