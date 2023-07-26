namespace ToolBX.AutoConfig;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAutoConfig(this IServiceCollection services, IConfiguration configuration)
    {
        var types = Types.Where(x => !x.IsInterface && !x.IsAbstract && !x.IsGenericTypeDefinition && !x.IsGenericType && x.HasAttribute<AutoConfigAttribute>()).ToList();

        foreach (var type in types)
        {
            var attribute = (AutoConfigAttribute)Attribute.GetCustomAttribute(type, typeof(AutoConfigAttribute), true)!;
            typeof(ServiceCollectionExtensions).GetMethod(nameof(Configure), BindingFlags.Static | BindingFlags.NonPublic)!.MakeGenericMethod(type)
                .Invoke(null, BindingFlags.Static | BindingFlags.NonPublic, null, new object[] { services, configuration, attribute.Name }, null);
        }

        return services;
    }

    private static IServiceCollection Configure<T>(IServiceCollection services, IConfiguration configuration, string name) where T : class => services.Configure<T>(x => configuration.GetSection(name).Bind(x));
}