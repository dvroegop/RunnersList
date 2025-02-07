using System.Text.Json.Serialization;

namespace RunnersListLibrary.Spotify.DTO.SpotifyDataObjects;

public class GetTracksResult
{
    [JsonPropertyName("tracks")]
    public Tracks Tracks { get; set; }
}