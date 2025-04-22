using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RunnersList;
using RunnersListLibrary;
using RunnersListLibrary.Secrets;

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
        // Register your services here
        services.AddTransient<IRunnerService, RunnerService>();

        var registrationServices = new RegistrationServices();
        registrationServices.RegisterServices(services);

        // Add local services
        services.AddHttpClient();

        // Add secrets
        services.Configure<OpenAiSecrets>(context.Configuration.GetSection("OpenAiSecrets"));
        services.Configure<SpotifySecrets>(context.Configuration.GetSection("SpotifySecrets"));
        services.Configure<SongBpmSecrets>(context.Configuration.GetSection("SongBpmSecrets"));
    })
    .Build();

var runnerService = host.Services.GetRequiredService<IRunnerService>();
await runnerService.RunAsync();