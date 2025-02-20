using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RunnersListLibrary;
using RunnersListLibrary.Secrets;
using RunnersListLibrary.ServiceProviders.SongBpm;
using RunnersListLibrary.ServiceProviders.Spotify;

var builder = FunctionsApplication.CreateBuilder(args);

// Add Azure Key Vault to the configuration
var keyVaultEndpoint = new Uri(Environment.GetEnvironmentVariable("KEYVAULT_ENDPOINT") ?? throw new InvalidOperationException("Key Vault endpoint is not set"));
builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());

builder.ConfigureFunctionsWebApplication();

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .AddHttpClient();

// Now, add the IOptions<T> to the services collection
builder.Services.AddOptions<SpotifySecrets>()
    .Configure<IConfiguration>((settings, configuration) =>
    {
        configuration.GetSection("SpotifySettings").Bind(settings);
    });

var registrationServices = new RegistrationServices();
registrationServices.RegisterServices(builder.Services);

builder.Build().Run();
