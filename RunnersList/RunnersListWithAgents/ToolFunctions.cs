using System.Text.Json;
using Azure.AI.Projects;

namespace RunnersListWithAgents;

internal class ToolFunctions
{
    public FunctionToolDefinition getCityNickNameTool = new("getCityNickName",
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

    public FunctionToolDefinition getUserFavoriteCityTool = new("getUserFavoriteCity", "Gets the users favorite city");

    public string GetCurrentWeatherAtLocation(string location, string temperatureUnit = "f") => location switch
    {
        "Seattle, WA" => temperatureUnit == "f" ? "70f" : "21c",
        "Amsterdam" => temperatureUnit == "f" ? "60f" : "15c",
        _ => throw new NotImplementedException()
    };
    public FunctionToolDefinition getCurrentWeatherAtLocationTool = new(
        name: "getCurrentWeatherAtLocation",
        description: "Gets the current weather at a provided location.",
        parameters: BinaryData.FromObjectAsJson(
            new
            {
                Type = "object",
                Properties = new
                {
                    Location = new
                    {
                        Type = "string",
                        Description = "The city and state, e.g. San Francisco, CA",
                    },
                    Unit = new
                    {
                        Type = "string",
                        Enum = new[] { "c", "f" },
                    },
                },
                Required = new[] { "location" },
            },
            new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));

    public string GetUserFavoriteCity()
    {
        return "Seattle";
    }

    public string GetCityNickName(string location)
    {
        return location switch
        {
            "Amsterdam" => "The Venice of the North",
            "Paris" => "The City of Light",
            "London" => "The Big Smoke",
            "Seattle" => "The Emerald City",
            _ => "Unknown"
        };
    }


    public string GetWeather(string city)
    {
        return city switch
        {
            "Amsterdam" => "20",
            "Paris" => "25",
            "London" => "15",
            "Seattle" => "18",
            _ => "0"
        };
    }
};

