using System.Text.Json.Serialization;

namespace RunnersListLibrary.DTO.SpotifyDataObjects;

public class GetTracksResult
{
    [JsonPropertyName("tracks")]
    public Tracks Tracks { get; set; }
}