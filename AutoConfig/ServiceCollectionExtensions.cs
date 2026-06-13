namespace ToolBX.AutoConfig;

public static class ServiceCollectionExtensions
{
    private const string RegistrarTypeName = "ToolBX.AutoConfig.Generated.AutoConfigRegistrar";

    /// <summary>
    /// Adds every class with the <see cref="AutoConfigAttribute"/> attribute (or type bound via <see cref="AutoConfigAttribute{T}"/>)
    /// from the specified assembly to the <see cref="IServiceCollection"/> as <see cref="IOptions{TOptions}"/>.
    /// </summary>
    public static IServiceCollection AddAutoConfig(this IServiceCollection services, Assembly assembly, IConfiguration configuration, AutoConfigOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(configuration);

        Register(assembly, services, configuration, options ?? new AutoConfigOptions());
        return services;
    }

    /// <summary>
    /// Adds every class with the <see cref="AutoConfigAttribute"/> attribute (or type bound via <see cref="AutoConfigAttribute{T}"/>)
    /// from all loaded assemblies to the <see cref="IServiceCollection"/> as <see cref="IOptions{TOptions}"/>.
    /// </summary>
    public static IServiceCollection AddAutoConfig(this IServiceCollection services, IConfiguration configuration, AutoConfigOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        options ??= new AutoConfigOptions();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.GetCustomAttribute<HasAutoConfigServicesAttribute>() is not null)
                Register(assembly, services, configuration, options);
        }

        return services;
    }

    private static void Register(Assembly assembly, IServiceCollection services, IConfiguration configuration, AutoConfigOptions options)
    {
        var registrar = assembly.GetType(RegistrarTypeName);
        var method = registrar?.GetMethod("Register", BindingFlags.Public | BindingFlags.Static);
        method?.Invoke(null, [services, configuration, options]);
    }

    /// <summary>
    /// Returns the resolved <see cref="IOptions{T}.Value"/> for every type bound via <see cref="AutoConfigAttribute"/> (or
    /// <see cref="AutoConfigAttribute{T}"/>) that is assignable to <typeparamref name="T"/>.
    /// </summary>
    public static IReadOnlyList<T> GetAutoConfigOptions<T>(this IServiceProvider serviceProvider) where T : class
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        var collected = new List<object>();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.GetCustomAttribute<HasAutoConfigServicesAttribute>() is null) continue;

            var registrar = assembly.GetType(RegistrarTypeName);
            var method = registrar?.GetMethod("CollectOptions", BindingFlags.Public | BindingFlags.Static);
            method?.Invoke(null, [serviceProvider, collected]);
        }

        return collected.OfType<T>().Distinct().ToList();
    }
}
