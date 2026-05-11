namespace ToolBX.AutoConfig;

public static class ServiceCollectionExtensions
{
    private static readonly MethodInfo ConfigureMethod = typeof(ServiceCollectionExtensions)
        .GetMethod(nameof(Configure), BindingFlags.Static | BindingFlags.NonPublic)!;

    /// <summary>
    /// Adds all classes with the <see cref="AutoConfigAttribute"/> attribute from the specified assembly to the <see cref="IServiceCollection"/> as <see cref="IOptions{TOptions}"/>.
    /// </summary>
    public static IServiceCollection AddAutoConfig(this IServiceCollection services, Assembly assembly, IConfiguration configuration, AutoConfigOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(configuration);

        return services.AddAutoConfig(configuration, GetBindings(Types.From(assembly), [assembly]), options);
    }

    /// <summary>
    /// Adds all classes with the <see cref="AutoConfigAttribute"/> attribute from all assemblies to the <see cref="IServiceCollection"/> as <see cref="IOptions{TOptions}"/>.
    /// </summary>
    public static IServiceCollection AddAutoConfig(this IServiceCollection services, IConfiguration configuration, AutoConfigOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        return services.AddAutoConfig(configuration, GetBindings(Types.ToList(), AppDomain.CurrentDomain.GetAssemblies()), options);
    }

    private static IServiceCollection AddAutoConfig(this IServiceCollection services, IConfiguration configuration, IEnumerable<(Type Target, AutoConfigAttributeBase Attribute)> bindings, AutoConfigOptions? options)
    {
        options ??= new AutoConfigOptions();

        foreach (var (target, attribute) in bindings)
        {
            var validateDataAnnotations = attribute.ValidateDataAnnotations || options.ValidateDataAnnotations;
            var validateOnStart = attribute.ValidateOnStart || options.ValidateOnStart;

            ConfigureMethod.MakeGenericMethod(target)
                .Invoke(null, [services, configuration, attribute.Name, validateDataAnnotations, validateOnStart]);
        }

        return services;
    }

    private static IEnumerable<(Type Target, AutoConfigAttributeBase Attribute)> GetBindings(IEnumerable<Type> types, IEnumerable<Assembly> assemblies)
    {
        foreach (var type in types)
        {
            if (type.IsInterface || type.IsAbstract || type.IsGenericTypeDefinition || type.IsGenericType) continue;

            foreach (var attribute in type.GetCustomAttributes(typeof(AutoConfigAttributeBase), true).Cast<AutoConfigAttributeBase>())
                yield return (attribute.TargetType ?? type, attribute);
        }

        foreach (var assembly in assemblies)
        {
            foreach (var attribute in assembly.GetCustomAttributes().OfType<AutoConfigAttributeBase>())
            {
                if (attribute.TargetType is null) continue;
                yield return (attribute.TargetType, attribute);
            }
        }
    }

    private static IServiceCollection Configure<T>(IServiceCollection services, IConfiguration configuration, string name, bool validateDataAnnotations, bool validateOnStart) where T : class
    {
        var section = configuration.GetSection(name);

        if (!validateDataAnnotations && !validateOnStart)
            return services.Configure<T>(section.Bind);

        var builder = services.AddOptions<T>().Bind(section);

        if (validateDataAnnotations)
            builder.ValidateDataAnnotations();

        if (validateOnStart)
            builder.ValidateOnStart();

        return services;
    }

    /// <summary>
    /// Returns the resolved <see cref="IOptions{T}.Value"/> for every type bound via <see cref="AutoConfigAttribute"/> (or <see cref="AutoConfigAttribute{T}"/>) that is assignable to <typeparamref name="T"/>.
    /// </summary>
    public static IReadOnlyList<T> GetAutoConfigOptions<T>(this IServiceProvider serviceProvider) where T : class
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        var targets = GetBindings(Types.ToList(), AppDomain.CurrentDomain.GetAssemblies())
            .Select(x => x.Target)
            .Where(typeof(T).IsAssignableFrom)
            .Distinct();

        var result = new List<T>();
        foreach (var target in targets)
        {
            var optionsType = typeof(IOptions<>).MakeGenericType(target);
            var options = serviceProvider.GetService(optionsType);
            if (options is null) continue;
            var value = (T)optionsType.GetProperty(nameof(IOptions<object>.Value))!.GetValue(options)!;
            result.Add(value);
        }
        return result;
    }
}
