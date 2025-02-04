using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using RunnersListLibrary.Secrets;
using RunnersListLibrary.Spotify;

namespace RunnersListLibrary;

internal class HelloSayer(IOptions<OpenAiSecrets> secrets) : IHelloSayer
{
    #region

    public async Task SayHello()
    {
        var kernelBuilder = Kernel.CreateBuilder();

        kernelBuilder.AddAzureOpenAIChatCompletion(
            secrets.Value.DeploymentName,
            secrets.Value.EndPoint,
            secrets.Value.ApiKey);


        kernelBuilder.Services.AddLogging(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Trace));

        var kernel = kernelBuilder.Build();

        kernel.Plugins.AddFromType<SpotifyConnector>("SpotifyConnector");

        var openAiPromptExecutionSettings = new AzureOpenAIPromptExecutionSettings()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        var history = new ChatHistory();
        history.AddSystemMessage("You are helping me creating a Spotify Playlist for my running workout. " +
                                 "You use the method to get the Spotify Token, then you can use that in your calls to Spotify." +
                                 "After that, you get my favorite genre from Spotify. This is what you show to me.");
        history.AddUserMessage("Please get the token for Spotify");
        try
        {
            var result = await chatCompletionService.GetChatMessageContentsAsync(
                history,
                executionSettings: openAiPromptExecutionSettings,
                kernel: kernel);
            var x = result[0].ToString();
            Console.WriteLine(x);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Assistant > " + ex.Message);
        }
    }

    #endregion
}