var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false);


IConfiguration config = builder.Build();

var services = new ServiceCollection();

ToolBX.AutoConfig.ServiceCollectionExtensions.AddAutoConfig(services, config);
services.AddAutoInjectServices();

var provider = services.BuildServiceProvider();
var app = new ConsoleAppBuilder
{
    ApplicationServices = provider
};

provider.GetRequiredService<ISample>().Start();