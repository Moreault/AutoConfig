namespace AutoConfig.Sample;

public class AssemblyInitializer : IAssemblyInitializer
{
    public void Configure(IServiceCollection services, IConfiguration configuration)
    {
        //TODO Add this line to AssemblyInitializer
        ToolBX.AutoConfig.ServiceCollectionExtensions.AddAutoConfig(services, configuration);
    }

    public void Configure(IApplicationBuilder app, IHostEnvironment env)
    {

    }
}