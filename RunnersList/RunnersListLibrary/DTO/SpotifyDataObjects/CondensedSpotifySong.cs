using System.Text.Json.Serialization;

namespace RunnersListLibrary.DTO.SpotifyDataObjects;

public class CondensedSpotifySong
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }
    [JsonPropertyName("title")]
    public required string Title { get; init; }
    [JsonPropertyName("artist")]
    public required string Artist { get; init; }

    public int Bpm { get; set; } = -1;
}