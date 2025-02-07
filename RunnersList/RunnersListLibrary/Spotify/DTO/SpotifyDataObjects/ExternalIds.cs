using System.Text.Json.Serialization;

namespace RunnersListLibrary.Spotify.DTO.SpotifyDataObjects;

public class ExternalIds
{
    [JsonPropertyName("isrc")]
    public string Isrc { get; set; }
}