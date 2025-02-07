using System.Text.Json.Serialization;

namespace RunnersListLibrary.Spotify.DTO.SpotifyDataObjects;

public class ExternalUrls
{
    [JsonPropertyName("spotify")]
    public string Spotify { get; set; }
}