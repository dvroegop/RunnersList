using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using RunnersListLibrary.Secrets;
using RunnersListLibrary.SemanticFunctions;
using RunnersListLibrary.ServiceProviders.SongBpm;
using RunnersListLibrary.ServiceProviders.Spotify;

namespace RunnersListLibrary;

internal class HelloSayer(
    IOptions<OpenAiSecrets> azureOpenAiSecrets,
    ISongBpmConnector songBpmConnector,
    ISpotifyConnector spotifyConnector) : IHelloSayer
{
    #region

    public async Task SayHello2()
    {
        var artist = "Bruce Springsteen";
        var title = "Fire";

        var songBPm = await songBpmConnector.GetSongBpmAsync(artist, title);
    }

    public async Task SayHello()
    {
        var kernelBuilder = Kernel.CreateBuilder();

        kernelBuilder.AddAzureOpenAIChatCompletion(
            azureOpenAiSecrets.Value.DeploymentName,
            azureOpenAiSecrets.Value.EndPoint,
            azureOpenAiSecrets.Value.ApiKey);

        
        kernelBuilder.Services.AddSingleton<ISpotifyConnector>(sp => spotifyConnector);
        kernelBuilder.Services.AddSingleton<ISongBpmConnector>(sp => songBpmConnector);

        kernelBuilder.Services.AddLogging(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Trace));
        kernelBuilder.Plugins.AddFromType<SpotifyFunctions>();
        kernelBuilder.Plugins.AddFromType<SongBpmFunctions>();
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
                                 "After that, ask for the genre they want for their playlist, then get the top 10 songs." +
                                 "For each song, look up the beats per minute for that song using the correct API. The API will add that information to the song."+
                                 "When showing the list to the user, call the Spotify connector to create the playlist there, passing in the IDs for the songs.");
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
                        content.ToAscii();

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

public static class StringExtensions
{
    public static void ToAscii(this string source)
    {
        // Go through every character in the string, and display the ascii value
        foreach (var c in source) Debug.Write($"{(int)c} ");

        Debug.WriteLine($"  : {source}");
    }
}