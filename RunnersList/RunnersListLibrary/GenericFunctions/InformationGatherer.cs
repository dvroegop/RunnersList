﻿using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using RunnersListLibrary.Secrets;
using RunnersListWithAgents.DTO;

namespace RunnersListLibrary.GenericFunctions;

internal class InformationGatherer(
    ILogger<InformationGatherer> logger,
    IOptions<OpenAiSecrets> openAiSecrets) : IInformationGatherer
{
    /// <summary>
    /// Asynchronously determines the user's favorite music genre by interacting with a chat-based AI assistant.
    /// The assistant helps the user refine their choice until a valid genre is selected.
    /// </summary>
    /// <returns>
    /// A string representing the user's favorite music genre. The result is one of the predefined genres:
    /// "rock", "pop", "eighties", or "electronic".
    /// </returns>
    public async Task<string> GetFavoriteMusicGenre()
    {
        // 1. Create the kernel
        var kernel = SetupKernel(out var chatCompletionService);

        // 2. Set up the initial prompt
        var history = SetupStartPrompts();

        // 3. Run through the conversation
        var finalAnswer = string.Empty;
        var canContinue = true;

        #region The conversation loop
        while (canContinue)
        {
            var responseBuilder = new StringBuilder();

            // Stream responses from the chat completion service
            await foreach (var response in chatCompletionService.GetStreamingChatMessageContentsAsync(
                               history,
                               new PromptExecutionSettings(),
                               kernel))
            {
                var content = response.Content;
                if (string.IsNullOrWhiteSpace(content)) continue;

                Console.Write(content);
                responseBuilder.Append(content);
            }

            var responseFromAssistant = responseBuilder.ToString();
            history.AddAssistantMessage(responseFromAssistant);

            // Check if the assistant has completed the process
            if (responseFromAssistant.Contains("completed", StringComparison.InvariantCultureIgnoreCase))
            {
                finalAnswer = ExtractGenre(responseFromAssistant);
                canContinue = false;
            }
            else
            {
                // Prompt the user for additional input if the process is not complete
                Console.Write(" > ");
                var responseFromUser = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(responseFromUser))
                    history.AddUserMessage(responseFromUser);
                else
                    Console.WriteLine("Please provide a valid input.");
            }
        }
        #endregion


        // 4. Return the final answer
        return finalAnswer;
    }

    /// <summary>
    /// Extracts the music genre from the given response string.
    /// </summary>
    /// <param name="responseFromAssistant">
    /// The response string received from the assistant, which may contain the genre information.
    /// </param>
    /// <returns>
    /// A string representing the extracted music genre. Returns an empty string if the genre cannot be determined.
    /// </returns>
    /// <remarks>
    /// This method attempts to deserialize the response as a JSON object to extract the genre. 
    /// If deserialization fails, it falls back to using a regular expression to locate the genre in the response.
    /// </remarks>
    private static string ExtractGenre(string responseFromAssistant)
    {
        string finalAnswer = string.Empty;
        var result = responseFromAssistant;
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };

            var genreResponse = JsonSerializer.Deserialize<GenreResponse>(result, options);
            finalAnswer = genreResponse?.Genre ?? string.Empty;
        }
        catch (JsonException)
        {
            // Attempt to extract genre name using regex as a fallback
            var regex = new Regex(@"['""]genre['""]\s*:\s*['""]([^'""]+)['""]");
            var match = regex.Match(result);
            if (match.Success)
                finalAnswer = match.Groups[1].Value;
            else
                Console.WriteLine("Could not find the genre name in the response. Please try again.");
        }

        return finalAnswer;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="chatCompletionService"></param>
    /// <returns></returns>
    private Kernel SetupKernel(out IChatCompletionService chatCompletionService)
    {
        var deploymentName = openAiSecrets.Value.DeploymentName;
        var apiKey = openAiSecrets.Value.ApiKey;
        var endPoint = openAiSecrets.Value.EndPoint;

        var kernelBuilder = Kernel.CreateBuilder();

        kernelBuilder.AddAzureOpenAIChatCompletion(
            deploymentName,
            endPoint,
            apiKey);

        kernelBuilder.Services.AddLogging(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Error));
        var kernel = kernelBuilder.Build();

        chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        return kernel;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private static ChatHistory SetupStartPrompts()
    {
        var systemPrompt = @"
You are a music genre expert. Please ask the user about their favorite genre. 
If they are not clear, please ask and help them refine until they got it.
The resulting genre must be one of the following:
- rock
- pop
- eighties
- electronic

When you both agree on the genre, respond in the form of a JSON object with that genre
and a status field that says 'completed'. Only return the JSON in the final result!
The JSON looks like this:
{
    'status': 'completed',
    'genre': 'Rock'
}
";
        var history = new ChatHistory();
        history.AddSystemMessage(systemPrompt);
        history.AddUserMessage("Help me find my favorite music genre");
        return history;
    }
}
