using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RunnersListLibrary;
using RunnersListLibrary.Secrets;
using RunnersListWithAgents;

Console.WriteLine("Starting the agent.");

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", true, true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

var configurationRoot = builder.Build();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) => { config.AddConfiguration(configurationRoot); })
    .ConfigureServices((context, services) =>
    {
        services.AddTransient<IAgentWrapper, AgentWrapper>();
        services.AddTransient<IInformationGatherer, InformationGatherer>();

        // Register additional services
        services.AddHttpClient();
        var runnersListLibraryRegistrationServices = new RegistrationServices();
        runnersListLibraryRegistrationServices.RegisterServices(services);

        services.Configure<OpenAiSecrets>(context.Configuration.GetSection("OpenAiSecrets"));
        services.Configure<SpotifySecrets>(context.Configuration.GetSection("SpotifySecrets"));
        services.Configure<SongBpmSecrets>(context.Configuration.GetSection("SongBpmSecrets"));
    })
    .Build();

var agentWrapper = host.Services.GetRequiredService<IAgentWrapper>();

await agentWrapper.RunAgent();