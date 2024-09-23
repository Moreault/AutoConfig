namespace ToolBX.AutoConfig;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all classes with the <see cref="AutoConfigAttribute"/> attribute from the specified assembly to the <see cref="IServiceCollection"/> as <see cref="IOptions{TOptions}"/>.
    /// </summary>
    public static IServiceCollection AddAutoConfig(this IServiceCollection services, Assembly assembly, IConfiguration configuration)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));
        if (assembly is null) throw new ArgumentNullException(nameof(assembly));
        if (configuration is null) throw new ArgumentNullException(nameof(configuration));

        var types = Types.From(assembly).Where(x => !x.IsInterface && !x.IsAbstract && !x.IsGenericTypeDefinition && !x.IsGenericType && x.HasAttribute<AutoConfigAttribute>());
        return services.AddAutoConfig(configuration, types);
    }

    /// <summary>
    /// Adds all classes with the <see cref="AutoConfigAttribute"/> attribute from all assemblies to the <see cref="IServiceCollection"/> as <see cref="IOptions{TOptions}"/>.
    /// </summary>
    public static IServiceCollection AddAutoConfig(this IServiceCollection services, IConfiguration configuration)
    {
        if (services is null) throw new ArgumentNullException(nameof(services));
        if (configuration is null) throw new ArgumentNullException(nameof(configuration));

        var types = Types.Where(x => !x.IsInterface && !x.IsAbstract && !x.IsGenericTypeDefinition && !x.IsGenericType && x.HasAttribute<AutoConfigAttribute>());
        return services.AddAutoConfig(configuration, types);
    }

    private static IServiceCollection AddAutoConfig(this IServiceCollection services, IConfiguration configuration, IEnumerable<Type> types)
    {
        foreach (var type in types)
        {
            var attribute = (AutoConfigAttribute)Attribute.GetCustomAttribute(type, typeof(AutoConfigAttribute), true)!;
            typeof(ServiceCollectionExtensions).GetMethod(nameof(Configure), BindingFlags.Static | BindingFlags.NonPublic)!.MakeGenericMethod(type)
                .Invoke(null, BindingFlags.Static | BindingFlags.NonPublic, null, new object[] { services, configuration, attribute.Name }, null);
        }

        return services;
    }

    private static IServiceCollection Configure<T>(IServiceCollection services, IConfiguration configuration, string name) where T : class
    {
        var section = GetSection(configuration, name);
        return services.Configure<T>(x => section.Bind(x));
    }

    private static IConfigurationSection GetSection(IConfiguration configuration, string path)
    {
        string[] parts = path.Split('.');
        IConfigurationSection section = configuration.GetSection(parts[0]);
        for (int i = 1; i < parts.Length; i++)
        {
            section = section.GetSection(parts[i]);
        }
        return section;
    }
}