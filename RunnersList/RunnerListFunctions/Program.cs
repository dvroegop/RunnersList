using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RunnersListLibrary;
using RunnersListLibrary.ServiceProviders.SongBpm;
using RunnersListLibrary.ServiceProviders.Spotify;


var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        var registrationServices = new RegistrationServices();
        registrationServices.RegisterServices(services);
        
    })
    .Build();

await host.RunAsync();

//var builder = FunctionsApplication.CreateBuilder(args);

//builder.ConfigureFunctionsWebApplication();

//// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
//builder.Services
//    .AddApplicationInsightsTelemetryWorkerService()
//    .ConfigureFunctionsApplicationInsights();


//builder.Build().Run();
