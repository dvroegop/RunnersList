using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
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


        kernelBuilder.Services.AddLogging(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Error));

        var kernel = kernelBuilder.Build();

        kernel.Plugins.AddFromType<SpotifyConnector>("SpotifyConnector");

        var openAiPromptExecutionSettings = new AzureOpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        var history = new ChatHistory();
        history.AddSystemMessage("You are helping me creating a Spotify Playlist for my running workout. " +
                                 "You use the method to get the Spotify Token, then you can use that in your calls to Spotify." +
                                 "After that, ask for the genre they want for their playlist, then get the top 10 songs.");
        history.AddUserMessage("Please generate a running playlist for me.");

        try
        {
            var canContinue = true;
            while (canContinue)
            {
                var responseBuilder = new StringBuilder();


                await foreach (var response in chatCompletionService.GetStreamingChatMessageContentsAsync(
                                   history,
                                   openAiPromptExecutionSettings,
                                   kernel))
                {
                    var content = response.Content;
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        Console.Write(content);
                        responseBuilder.Append(content);
                    }
                }

                history.AddAssistantMessage(responseBuilder.ToString());

                Console.Write(" > ");
                var responseFromUser = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(responseFromUser) || responseFromUser.ToUpperInvariant().Trim() == "EXIT")
                    canContinue = false;
                else
                    history.AddUserMessage(responseFromUser);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Assistant > " + ex.Message);
        }
    }

    #endregion
}