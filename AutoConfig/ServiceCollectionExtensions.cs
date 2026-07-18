namespace ToolBX.AutoConfig;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds every class with the <see cref="AutoConfigAttribute"/> attribute (or type bound via <see cref="AutoConfigAttribute{T}"/>)
    /// from the specified assembly to the <see cref="IServiceCollection"/> as <see cref="IOptions{TOptions}"/>.
    /// </summary>
    public static IServiceCollection AddAutoConfig(this IServiceCollection services, Assembly assembly, IConfiguration configuration, AutoConfigOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(configuration);

        options ??= new AutoConfigOptions();
        foreach (var registration in AutoConfigRegistry.For(assembly))
            registration.Register(services, configuration, options);

        return services;
    }

    /// <summary>
    /// Adds every class with the <see cref="AutoConfigAttribute"/> attribute (or type bound via <see cref="AutoConfigAttribute{T}"/>)
    /// from all assemblies that declare <c>[AutoConfig]</c> bindings to the <see cref="IServiceCollection"/> as <see cref="IOptions{TOptions}"/>.
    /// </summary>
    public static IServiceCollection AddAutoConfig(this IServiceCollection services, IConfiguration configuration, AutoConfigOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        options ??= new AutoConfigOptions();
        foreach (var registration in AutoConfigRegistry.All())
            registration.Register(services, configuration, options);

        return services;
    }

    /// <summary>
    /// Returns the resolved <see cref="IOptions{T}.Value"/> for every type bound via <see cref="AutoConfigAttribute"/> (or
    /// <see cref="AutoConfigAttribute{T}"/>) that is assignable to <typeparamref name="T"/>.
    /// </summary>
    public static IReadOnlyList<T> GetAutoConfigOptions<T>(this IServiceProvider serviceProvider) where T : class
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        var collected = new List<object>();
        foreach (var registration in AutoConfigRegistry.All())
            registration.CollectOptions(serviceProvider, collected);

        return collected.OfType<T>().Distinct().ToList();
    }
}
