using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using RunnersListLibrary.Secrets;
using RunnersListWithAgents.DTO;
using RunnersListWithAgents.ExposedFunctions;

namespace RunnersListLibrary.GenericFunctions;

internal class InformationGatherer(
    ILogger<InformationGatherer> logger,
    IOptions<OpenAiSecrets> openAiSecrets) : IInformationGatherer
{
    public async Task<string> GetFavoriteMusicGenre()
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

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

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

        var finalAnswer = string.Empty;
        var canContinue = true;

        while (canContinue)
        {
            var responseBuilder = new StringBuilder();

            await foreach (var response in chatCompletionService.GetStreamingChatMessageContentsAsync(
                               history,
                               new PromptExecutionSettings(),
                               kernel))
            {
                var content = response.Content;
                if (!string.IsNullOrWhiteSpace(content))
                {
                    Console.Write(content);
                    responseBuilder.Append(content);

                    if (content.Contains("\n"))
                        Console.WriteLine(Environment.NewLine);
                }
            }

            var responseFromAssistant = responseBuilder.ToString();
            history.AddAssistantMessage(responseFromAssistant);

            if (responseFromAssistant.Contains("completed", StringComparison.InvariantCultureIgnoreCase))
            {
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

                canContinue = false;
            }
            else
            {
                Console.Write(" > ");
                var responseFromUser = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(responseFromUser))
                    history.AddUserMessage(responseFromUser);
                else
                    Console.WriteLine("Please provide a valid input.");
            }
        }

        return finalAnswer;
    }
}
