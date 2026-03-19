namespace ToolBX.AutoConfig;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds all classes with the <see cref="AutoConfigAttribute"/> attribute from the specified assembly to the <see cref="IServiceCollection"/> as <see cref="IOptions{TOptions}"/>.
    /// </summary>
    public static IServiceCollection AddAutoConfig(this IServiceCollection services, Assembly assembly, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(configuration);

        var types = Types.From(assembly).Where(x => !x.IsInterface && !x.IsAbstract && !x.IsGenericTypeDefinition && !x.IsGenericType && x.HasAttribute<AutoConfigAttribute>());
        return services.AddAutoConfig(configuration, types);
    }

    /// <summary>
    /// Adds all classes with the <see cref="AutoConfigAttribute"/> attribute from all assemblies to the <see cref="IServiceCollection"/> as <see cref="IOptions{TOptions}"/>.
    /// </summary>
    public static IServiceCollection AddAutoConfig(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var types = Types.Where(x => !x.IsInterface && !x.IsAbstract && !x.IsGenericTypeDefinition && !x.IsGenericType && x.HasAttribute<AutoConfigAttribute>());
        return services.AddAutoConfig(configuration, types);
    }

    private static IServiceCollection AddAutoConfig(this IServiceCollection services, IConfiguration configuration, IEnumerable<Type> types)
    {
        foreach (var type in types)
        {
            var attribute = (AutoConfigAttribute)Attribute.GetCustomAttribute(type, typeof(AutoConfigAttribute), true)!;
            typeof(ServiceCollectionExtensions).GetMethod(nameof(Configure), BindingFlags.Static | BindingFlags.NonPublic)!.MakeGenericMethod(type)
                .Invoke(null, BindingFlags.Static | BindingFlags.NonPublic, null, [services, configuration, attribute.Name, attribute.ValidateDataAnnotations, attribute.ValidateOnStart], null);
        }

        return services;
    }

    private static IServiceCollection Configure<T>(IServiceCollection services, IConfiguration configuration, string name, bool validateDataAnnotations, bool validateOnStart) where T : class
    {
        var section = GetSection(configuration, name);

        if (!validateDataAnnotations && !validateOnStart)
        {
            return services.Configure<T>(x => section.Bind(x));
        }

        var builder = services.AddOptions<T>().Bind(section);

        if (validateDataAnnotations)
            builder.ValidateDataAnnotations();

        if (validateOnStart)
            builder.ValidateOnStart();

        return services;
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