using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using RunnersListLibrary.Secrets;
using RunnersListLibrary.Spotify;

namespace RunnersListLibrary;

internal class HelloSayer(IOptions<OpenAiSecrets> secrets) : IHelloSayer
{
    public void SayHello()
    {
        var kernel = Kernel.CreateBuilder();
        kernel.Plugins.Services.AddAzureOpenAIChatCompletion(secrets.Value.DeploymentName, secrets.Value.EndPoint,
            secrets.Value.ApiKey);

        kernel.Plugins.AddFromType<SpotifyConnector>("SpotifyConnect");

        var x = kernel.Services.AsEnumerable();
        foreach (var y in x)
        {
            Console.WriteLine(y.ToString());
        }
    }
}
