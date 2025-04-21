using System.Text.Json;
using Azure;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RunnersListLibrary.Secrets;
using RunnersListLibrary.ServiceProviders.Spotify;
using RunnersListWithAgents.ExposedFunctions;
using RunnersListWithAgents.ToolFunctions;

namespace RunnersListWithAgents;

internal class AgentWrapper(
    ILogger<AgentWrapper> logger, 
    IOptions<OpenAiSecrets> openAiSecrets,
    ISpotifyConnector spotifyConnector,
    IInformationGatherer informationGatherer
    ) : IAgentWrapper
{
    private readonly SpotifyToolFunctions _spotifyToolFunctions = new();
    private readonly InformationGathererFunctions _informationGathererFunctions = new();

    public async Task RunAgent()
    {
        var modelName = openAiSecrets.Value.DeploymentName;
        var connectionString = openAiSecrets.Value.AiFoundryConnectionString;
        
        var aiProjectClient = new AIProjectClient(connectionString, new DefaultAzureCredential());

        var client = aiProjectClient.GetAgentsClient();

        var agent = await CreateAgent(client, modelName);
        try
        {

            Response<AgentThread> threadResponse = await aiProjectClient.GetAgentsClient().CreateThreadAsync();
            var thread = threadResponse.Value;
            try
            {

                Response<ThreadMessage> messageResponse = await client.CreateMessageAsync(
                    thread.Id,
                    content: "Can you create me the ultimate running music playlist?",
                    role: MessageRole.User);

                var message = messageResponse.Value;

                Response<ThreadRun> runResponse = await client.CreateRunAsync(thread, agent);

                await HandleThread(runResponse, client, thread);


                Response<PageableList<ThreadMessage>>
                    afterRunMessageResponse = await client.GetMessagesAsync(thread.Id);

                var messages = afterRunMessageResponse.Value.Data;

                DisplayResults(messages);

            }
            finally
            {

                // Delete the agent
                await client.DeleteThreadAsync(thread.Id);
            }
        }
        finally
        {

            await client.DeleteAgentAsync(agent.Id);
        }

    }

    private async Task<Agent> CreateAgent(AgentsClient client, string modelName)
    {
        var agentResponse = await client.CreateAgentAsync(modelName,
            "TestAgentClient",
            instructions: @"
You are a knowledgeable assistant that helps the user create a Spotify playlist tailored for their running workout. 
Follow the steps below precisely. 
Always maintain state and pass tokens where needed.
First, we need to get the users favorite genre. Then, we need to get a token so we
can communicate with Spotify. 
Then, we need to get the top 50 songs in that genre. 
Then we display those songs to the user.
",
            tools: new List<ToolDefinition>
            {
                _spotifyToolFunctions.GetSpotifyTokenTool,
                _spotifyToolFunctions.GetTop50SongsForGenreTool,
                _informationGathererFunctions.GetUserFavoriteMusicGenreTool
            });

        var agent = agentResponse.Value;
        return agent;
    }

    private async Task HandleThread(Response<ThreadRun> runResponse, AgentsClient client, AgentThread thread)
    {
        do
        {
            await Task.Delay(TimeSpan.FromMilliseconds(500));

            runResponse = await client.GetRunAsync(thread.Id, runResponse.Value.Id);
            if(runResponse.Value.Status == RunStatus.Failed)
            {
                Console.WriteLine($"Run failed. {runResponse.Value.LastError.Message}");
                break;
            }
            if (runResponse.Value.Status == RunStatus.RequiresAction
                && runResponse.Value.RequiredAction is SubmitToolOutputsAction submitToolOutputsAction)
            {
                List<ToolOutput> toolOutputs = new();
                foreach (var toolCall in submitToolOutputsAction.ToolCalls)
                    toolOutputs.Add(await GetResolvedToolOutput(toolCall));
                runResponse = await client.SubmitToolOutputsToRunAsync(runResponse.Value, toolOutputs);
            }

            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Current Status: {runResponse.Value.Status}");
            Console.ForegroundColor = oldColor;


        } while (runResponse.Value.Status == RunStatus.Queued
                 || runResponse.Value.Status == RunStatus.InProgress);
    }

    private static void DisplayResults(IReadOnlyList<ThreadMessage> messages)
    {
        for (int i = messages.Count - 1; i >= 0; i--)
        {
            var threadMessage = messages[i];
            Console.Write($"{threadMessage.CreatedAt:yyyy-MM-dd HH:mm:ss} - {threadMessage.Role,10}: ");
            foreach (var contentItem in threadMessage.ContentItems)
            {
                if (contentItem is MessageTextContent textItem)
                    Console.Write(textItem.Text);
                else if (contentItem is MessageImageFileContent imageFileItem)
                    Console.Write($"<image from ID: {imageFileItem.FileId}");

                Console.WriteLine();
            }
        }
    }

    private async Task<ToolOutput> GetResolvedToolOutput(RequiredToolCall toolCall)
    {

        if (toolCall is RequiredFunctionToolCall functionToolCall)
        {
            using var argumentsJson = JsonDocument.Parse(functionToolCall.Arguments);

            if (functionToolCall.Name == _informationGathererFunctions.GetUserFavoriteMusicGenreTool.Name)
            {
                //return new ToolOutput(toolCall, "rock");
                return new ToolOutput(toolCall, await informationGatherer.GetFavoriteMusicGenre());
            }

            if (functionToolCall.Name == _spotifyToolFunctions.GetSpotifyTokenTool.Name)
            {
                return new ToolOutput(toolCall, await spotifyConnector.GetSpotifyTokenAsync());
            }

            if (functionToolCall.Name == _spotifyToolFunctions.GetTop50SongsForGenreTool.Name)
            {
                var tokenArgument = argumentsJson.RootElement.GetProperty("token").GetString();
                var genreArgument = argumentsJson.RootElement.GetProperty("genre").GetString();
                return new ToolOutput(toolCall, await spotifyConnector.GetSongsAsync(tokenArgument ?? throw new InvalidOperationException(), genreArgument ?? throw new InvalidOperationException()));

            }
        }

        return null;
    }
}