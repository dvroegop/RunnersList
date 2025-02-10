using Microsoft.Extensions.DependencyInjection;
using RunnersListLibrary.SongBpm;
using RunnersListLibrary.Spotify;

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
