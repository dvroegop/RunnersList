using Microsoft.Extensions.DependencyInjection;
using RunnersListLibrary.ServiceProviders.SongBpm;
using RunnersListLibrary.ServiceProviders.Spotify;

namespace RunnersListLibrary
{
    public class RegistrationServices
    {
        public void RegisterServices(IServiceCollection services)
        {
            // Register your services here
            services.AddTransient<IHelloSayer, HelloSayer>();
            services.AddSingleton<ISpotifyConnector, SpotifyConnector>();
            services.AddSingleton<ISongBpmConnector, SongBpmConnector>();   
        }
    }
}
