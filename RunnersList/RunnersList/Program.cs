using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RunnersList.ApplicationCore;
using RunnersListLibrary;
using RunnersListLibrary.Secrets;


// 1. Create the Configuration Builder to hold configuration (duh...)
var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", true, true)
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();

var configurationRoot = builder.Build();


// 2. Create the Host 
var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) => { config.AddConfiguration(configurationRoot); })
    .ConfigureServices((context, services) =>
    {
        
        // 3. Add the services to the DI
        // (this is the mail service)
        services.AddTransient<IRunnerService, RunnerService>();

        // 4. Register the library services
        var registrationServices = new RegistrationServices();
        registrationServices.RegisterServices(services);

        // 5. Add local services
        services.AddHttpClient();

        // 6. Add secrets
        services.Configure<OpenAiSecrets>(context.Configuration.GetSection("OpenAiSecrets"));
        services.Configure<SpotifySecrets>(context.Configuration.GetSection("SpotifySecrets"));
        services.Configure<SongBpmSecrets>(context.Configuration.GetSection("SongBpmSecrets"));
    })
    .Build();


// 7. Run the service
var runnerService = host.Services.GetRequiredService<IRunnerService>();
await runnerService.RunAsync();

// 8. Enjoy the coffee / tea / whatever you drink