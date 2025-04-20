using System.Text.Json;
using Azure;
using Azure.AI.Projects;
using Azure.Identity;

namespace RunnersListWithAgents;

internal class AgentWrapper
{
    private ToolFunctions toolFunctions= new();

    public async Task RunAgent()
    {
        var modelName = "gpt-4o";
        var connectionString =
            "swedencentral.api.azureml.ms;279d1d3a-3490-47dd-85e5-ee752ad3da9d;rg-dvroegop-6520_ai;testprojectforlearning";
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
                    content: "What is the weather in my favority city?",
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
            instructions: "You are a weather bot. Use the provided functions to help answer questions. "
                          + "Customize your responses to the user's preferences as much as possible and use friendly "
                          + "nicknames for cities whenever possible.",
            tools: new List<ToolDefinition>
            {
                toolFunctions._getUserFavoriteCityTool, 
                toolFunctions._getCityNickNameTool, 
                toolFunctions._getCurrentWeatherAtLocationTool
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

            var implementations = new Implementations();
            using var argumentsJson = JsonDocument.Parse(functionToolCall.Arguments);

            if (functionToolCall.Name == toolFunctions._getUserFavoriteCityTool.Name)
                return new ToolOutput(toolCall, await implementations.GetUserFavoriteCity());

            if (functionToolCall.Name == toolFunctions._getCityNickNameTool.Name)
            {
                var locationArgument = argumentsJson.RootElement.GetProperty("location").GetString();
                return new ToolOutput(toolCall, implementations.GetCityNickName(locationArgument));
            }

            if (functionToolCall.Name == toolFunctions._getCurrentWeatherAtLocationTool.Name)
            {
                var locationArgument = argumentsJson.RootElement.GetProperty("location").GetString();
                if (argumentsJson.RootElement.TryGetProperty("unit", out var unitElement))
                {
                    var unitArgument = unitElement.GetString();
                    return new ToolOutput(toolCall, implementations.GetWeather(locationArgument, unitArgument));
                }

                return new ToolOutput(toolCall, implementations.GetWeather(locationArgument));
            }
        }

        return null;
    }
}