namespace ToolBX.AutoConfig;

/// <summary>
/// Collects the source-generated registration callbacks produced for each assembly that declares
/// <c>[AutoConfig]</c> bindings. The generated <c>AutoConfigRegistrar</c> self-registers here from a
/// <see cref="System.Runtime.CompilerServices.ModuleInitializerAttribute"/>, which keeps discovery
/// reflection-free (no <c>Assembly.GetType</c>/<c>MethodInfo.Invoke</c>). Configuration binding itself still
/// relies on reflection, so bound types are not trimming/Native AOT safe, but discovery no longer is.
/// </summary>
public static class AutoConfigRegistry
{
    private static readonly List<Registration> Registrations = new();

    /// <summary>
    /// Called by generated code to register an assembly's <c>[AutoConfig]</c> bindings. Not intended to be
    /// called directly.
    /// </summary>
    public static void Register(Assembly assembly, Action<IServiceCollection, IConfiguration, AutoConfigOptions> register, Action<IServiceProvider, List<object>> collectOptions)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(register);
        ArgumentNullException.ThrowIfNull(collectOptions);
        Registrations.Add(new Registration(assembly, register, collectOptions));
    }

    internal static IEnumerable<Registration> For(Assembly assembly)
    {
        foreach (var registration in Registrations)
            if (Equals(registration.Assembly, assembly))
                yield return registration;
    }

    internal static IEnumerable<Registration> All()
    {
        foreach (var registration in Registrations)
            yield return registration;
    }

    internal sealed record Registration(Assembly Assembly, Action<IServiceCollection, IConfiguration, AutoConfigOptions> Register, Action<IServiceProvider, List<object>> CollectOptions);
}
