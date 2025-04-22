using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using RunnersListLibrary;
using RunnersListLibrary.Secrets;
using RunnersListLibrary.ServiceProviders.SongBpm;
using RunnersListLibrary.ServiceProviders.Spotify;
using RunnersListWithAgents.ExposedFunctions;
using System.Text;

namespace RunnersList;

public class RunnerService(
    ILogger<RunnerService> logger, 
    IOptions<OpenAiSecrets> azureOpenAiSecrets, 
    ISpotifyConnector spotifyConnector, 
    ISongBpmConnector songBpmConnector,
    IInformationGatherer informationGatherer) : IRunnerService
{
    #region

    public async Task RunAsync()
    {
        var kernelBuilder = Kernel.CreateBuilder();

        kernelBuilder.AddAzureOpenAIChatCompletion(
            azureOpenAiSecrets.Value.DeploymentName,
            azureOpenAiSecrets.Value.EndPoint,
            azureOpenAiSecrets.Value.ApiKey);


        kernelBuilder.Services.AddSingleton<ISpotifyConnector>(sp => spotifyConnector);
        kernelBuilder.Services.AddSingleton<ISongBpmConnector>(sp => songBpmConnector);
        kernelBuilder.Services.AddSingleton<IInformationGatherer>(sp => informationGatherer);

        kernelBuilder.Services.AddLogging(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Trace));
        kernelBuilder.Plugins.AddFromType<RunnersList.SemanticFunctions.SpotifyFunctions>();
        kernelBuilder.Plugins.AddFromType<RunnersList.SemanticFunctions.SongBpmFunctions>();
        kernelBuilder.Plugins.AddFromType<RunnersList.SemanticFunctions.MiscFunctions>();
        
        var kernel = kernelBuilder.Build();


        var openAiPromptExecutionSettings = new AzureOpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        var history = new ChatHistory();
        history.AddSystemMessage("You are helping me creating a Spotify Playlist for my running workout. " +
                                 "You use the method to get the Spotify Token, then you can use that in your calls to Spotify." +
                                 "Pass the token to each call to Spotify." +
                                 "After that, ask for the genre they want for their playlist, then get the top songs." +
                                 "For each song, look up the beats per minute for that song using the correct API. The API will add that information to the song." +
                                 "Then you show that list to the user, including all songs that match a BPM between 130 and 170");
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
                        //content.ToAscii();

                        if (content.Contains("\n"))
                            Console.WriteLine(Environment.NewLine);
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