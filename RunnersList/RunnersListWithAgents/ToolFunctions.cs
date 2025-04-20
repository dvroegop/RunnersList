using System.Text.Json;
using Azure.AI.Projects;

namespace RunnersListWithAgents;

internal class ToolFunctions
{
   
    #region FunctionToolDefinitions

    public FunctionToolDefinition _getUserFavoriteCityTool = new("getUserFavoriteCity",
        "Gets the users favorite city");

    public FunctionToolDefinition _getCityNickNameTool = new("getCityNickName",
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

    public FunctionToolDefinition _getCurrentWeatherAtLocationTool = new(
        "getCurrentWeatherAtLocation",
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