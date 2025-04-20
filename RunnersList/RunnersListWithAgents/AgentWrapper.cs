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

        var agentResponse = await client.CreateAgentAsync(modelName,
            "TestAgentClient",
            instructions: "You are a weather bot. Use the provided functions to help answer questions. "
                          + "Customize your responses to the user's preferences as much as possible and use friendly "
                          + "nicknames for cities whenever possible.",
            tools: new List<ToolDefinition>
            {
                toolFunctions.getUserFavoriteCityTool, 
                toolFunctions.getCityNickNameTool, 
                toolFunctions.getCurrentWeatherAtLocationTool
            });

        var agent = agentResponse.Value;


        Response<AgentThread> threadResponse = await aiProjectClient.GetAgentsClient().CreateThreadAsync();
        var thread = threadResponse.Value;

        Response<ThreadMessage> messageResponse = await client.CreateMessageAsync(
            thread.Id,
            content: "What is the weather in my favority city?",
            role: MessageRole.User);

        var message = messageResponse.Value;

        Response<ThreadRun> runResponse = await client.CreateRunAsync(thread, agent);

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
                    toolOutputs.Add(GetResolvedToolOutput(toolCall));
                runResponse = await client.SubmitToolOutputsToRunAsync(runResponse.Value, toolOutputs);
            }


        } while (runResponse.Value.Status == RunStatus.Queued
                 || runResponse.Value.Status == RunStatus.InProgress);


        Response<PageableList<ThreadMessage>> afterRunMessageResponse = await client.GetMessagesAsync(thread.Id);

        var messages = afterRunMessageResponse.Value.Data;

        foreach (var threadMessage in messages)
        {
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

        // Delete the agent
        await client.DeleteThreadAsync(thread.Id);
        await client.DeleteAgentAsync(agent.Id);

        Console.WriteLine("At the end");
    }

    private ToolOutput GetResolvedToolOutput(RequiredToolCall toolCall)
    {
        if (toolCall is RequiredFunctionToolCall functionToolCall)
        {
            if (functionToolCall.Name == toolFunctions.getUserFavoriteCityTool.Name)
                return new ToolOutput(toolCall, toolFunctions.GetUserFavoriteCity());
            using var argumentsJson = JsonDocument.Parse(functionToolCall.Arguments);
            if (functionToolCall.Name == toolFunctions.getCityNickNameTool.Name)
            {
                var locationArgument = argumentsJson.RootElement.GetProperty("location").GetString();
                return new ToolOutput(toolCall, toolFunctions.GetCityNickName(locationArgument));
            }

            if (functionToolCall.Name == toolFunctions.getCurrentWeatherAtLocationTool.Name)
            {
                var locationArgument = argumentsJson.RootElement.GetProperty("location").GetString();
                if (argumentsJson.RootElement.TryGetProperty("unit", out var unitElement))
                {
                    var unitArgument = unitElement.GetString();
                    return new ToolOutput(toolCall, toolFunctions.GetWeather(locationArgument));
                }

                return new ToolOutput(toolCall, toolFunctions.GetWeather(locationArgument));
            }
        }

        return null;
    }
}