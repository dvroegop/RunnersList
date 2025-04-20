using System.Text.Json;
using Azure.AI.Projects;

namespace RunnersListWithAgents;

internal class ToolFunctions
{
   
    #region FunctionToolDefinitions

    public FunctionToolDefinition GetUserFavoriteCityTool = new("getUserFavoriteCityTool",
        "Gets the users favorite city");

    public FunctionToolDefinition GetCityNickNameTool = new("getCityNickNameTool",
        "Gets the nickname of a city, e.g. 'LA' for Los Angeles, CA.",
        BinaryData.FromObjectAsJson(new
            {
                Type = "object",
                Properties = new
                {
                    Location = new
                    {
                        Type = "string",
                        Description = "The city, e.g. 'Alkmaar'"
                    }
                },
                Required = new[]
                {
                    "Location"
                }
            },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));

    public FunctionToolDefinition GetCurrentWeatherAtLocationTool = new(
        "getCurrentWeatherAtLocationTool",
        "Gets the current weather at a provided location.",
        BinaryData.FromObjectAsJson(
            new
            {
                Type = "object",
                Properties = new
                {
                    Location = new
                    {
                        Type = "string",
                        Description = "The city and state, e.g. San Francisco, CA"
                    },
                    Unit = new
                    {
                        Type = "string",
                        Enum = new[] { "c", "f" }
                    }
                },
                Required = new[] { "location" }
            },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));

    #endregion
}