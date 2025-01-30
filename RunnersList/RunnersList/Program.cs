using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RunnersList;
using RunnersListLibrary;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Register your services here
        services.AddTransient<IRunnerService, RunnerService>();

        var registrationServices = new RegistrationServices();
        registrationServices.RegisterServices(services);
    })
    .Build();

var runnerService = host.Services.GetRequiredService<IRunnerService>();
runnerService.Run();