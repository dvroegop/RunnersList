using Microsoft.Extensions.DependencyInjection;

namespace RunnersListLibrary
{
    public class RegistrationServices
    {
        public void RegisterServices(IServiceCollection services)
        {
            // Register your services here
            services.AddTransient<IHelloSayer, HelloSayer>();
        }
    }
}
