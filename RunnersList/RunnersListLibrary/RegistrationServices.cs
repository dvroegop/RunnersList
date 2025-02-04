using Microsoft.Extensions.DependencyInjection;
using RunnersListLibrary.Spotify;

namespace RunnersListLibrary
{
    public class RegistrationServices
    {
        public void RegisterServices(IServiceCollection services)
        {
            // Register your services here
            services.AddTransient<IHelloSayer, HelloSayer>();
            //services.AddTransient<ISpotifyConnector, SpotifyConnector>();
        }
    }
}
