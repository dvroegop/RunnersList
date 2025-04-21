using System.Text.Json.Serialization;

namespace RunnersListWithAgents.DTO;

internal class GenreResponse
{
    [JsonPropertyName("status")] public required string Status { get; init; }

    [JsonPropertyName("genre")] public required string Genre { get; init; }
}