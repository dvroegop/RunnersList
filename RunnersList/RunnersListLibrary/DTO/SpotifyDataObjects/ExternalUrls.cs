using System.Text.Json.Serialization;

namespace RunnersListLibrary.DTO.SpotifyDataObjects;

public class ExternalUrls
{
    [JsonPropertyName("spotify")]
    public string Spotify { get; set; }
}