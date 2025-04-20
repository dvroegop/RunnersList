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

internal class Kickstarter(
    IOptions<OpenAiSecrets> azureOpenAiSecrets,
    ISongBpmConnector songBpmConnector,
    ISpotifyConnector spotifyConnector) : IKickstarter
{
    #region

    public async Task TestApi()
    {
        var token = await spotifyConnector.GetSpotifyTokenAsync();
        var allSongs = await spotifyConnector.GetSongsAsync(token);
        foreach (var song in allSongs.Tracks.Items)
        {
            var songTitle = song.Name;
            var songArtist = song.Artists[0].Name;
            var bpm = await songBpmConnector.GetSongBpmAsync(songArtist, songTitle);
        }
        Console.Write("done");
        //var bpm = 140;
        //var matchingSongs = await songBpmConnector.GetSongsWithGivenBpmAsync(bpm);
        //Console.WriteLine(matchingSongs);
    }

    public async Task StartAsync()
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
        string systemPrompt = @"
You are an assistant helping the user create a Spotify playlist tailored for their running workout. Follow the steps below precisely. Always maintain state and pass tokens where needed.

## Goal:
Build a Spotify playlist with songs matching the user’s preferred genre and desired running tempo (in BPM).

## Instructions:

1. **Authenticate with Spotify**
   - Use the available method to retrieve a valid Spotify access token.
   - Store and reuse this token for all subsequent Spotify API calls.

2. **Ask the User**
   - Ask the user to provide:
     - Their preferred **music genre** (e.g. rock, electronic, pop).
     - Their **target running BPM** (optional — if not provided, use a default of 160 BPM).

3. **Retrieve Songs**
   - Call Spotify to retrieve the **top 50 tracks** for the given genre.
   - Include artist and track ID information in the results.

4. **Enrich with BPM Data**
   - For each track, call the **SongBeatBPM API** to retrieve its beats per minute (BPM).
   - Store the BPM value alongside each track.

5. **Filter/Sort by Tempo**
   - Compare each song’s BPM with the user’s desired tempo.
   - Prioritize or filter songs that match or are within ±5 BPM of the target.
   - Aim for a final playlist of around 30–50 tracks.

6. **Create Spotify Playlist**
   - Use the Spotify connector to create a new playlist in the user’s account.
   - Add the selected track IDs to the playlist.

7. **Present the Result**
   - Summarize the playlist:
     - Genre
     - Target BPM
     - Number of songs
     - A fun or motivational message for the user’s run
   - Optionally provide a link to open the playlist in Spotify.

## Notes:
- Always pass the Spotify token in each API request.
- Be clear and concise in user prompts.
- If the user does not provide a genre or BPM, ask again — don't guess.
- Keep the interaction friendly and helpful — you’re their musical running buddy.
";

        var history = new ChatHistory();
        history.AddSystemMessage(systemPrompt);
        //history.AddSystemMessage("You are helping me creating a Spotify Playlist for my running workout. " +
        //                         "You use the method to get the Spotify Token, then you can use that in your calls to Spotify." +
        //                         "Pass the token to each call to Spotify." +
        //                         "After that, ask for the genre they want for their playlist, then get the top 50 songs." +
        //                         "For each song, look up the beats per minute for that song using the correct API. " +
        //                         "The API will add that information to the song."+
        //                         "When showing the list to the user, call the Spotify connector to create the playlist there, " +
        //                         "passing in the IDs for the songs.");
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
                       // content.ToAscii();

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