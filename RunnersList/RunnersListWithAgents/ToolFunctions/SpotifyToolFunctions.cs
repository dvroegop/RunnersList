using System.Text.Json;
using Azure.AI.Projects;

namespace RunnersListWithAgents.ToolFunctions;

internal class SpotifyToolFunctions
{
    public FunctionToolDefinition GetSpotifyTokenTool = new("getSpotifyTokenTool",
        "Gets the Spotify token",
        BinaryData.FromObjectAsJson(
            new
            {
                Type= "object",
                Properties = new
                {
                    ClientId = new
                    {
                        Type = "string",
                        Description = "The Spotify client id"
                    },
                    ClientSecret = new
                    {
                        Type = "string",
                        Description = "The Spotify client secret"
                    },
                    RedirectUrl = new
                    {
                        Type = "string",
                        Description = "The Spotify redirect url"
                    }
                },
                Required = new[]
                {
                    "ClientId",
                    "ClientSecret",
                    "RedirectUrl"
                }
            },
            new JsonSerializerOptions{PropertyNamingPolicy = JsonNamingPolicy.CamelCase}
        ));

    public FunctionToolDefinition GetTop50SongsForGenreTool = new("getTop50SongsForGenreTool",
        description: "Gets the top 50 songs for a given genre from Spotify, using the specified token to log in.",
        BinaryData.FromObjectAsJson(
            new
            {
                Type = "object",
                Properties = new
                {
                    Genre = new
                    {
                        Type = "string",
                        Description = "The genre to get the top 50 songs for"
                    },
                    Token = new
                    {
                        Type = "string",
                        Description = "The Spotify token"
                    }
                },
                Required = new[]
                {
                    "Genre",
                    "Token"
                }
            },
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
}