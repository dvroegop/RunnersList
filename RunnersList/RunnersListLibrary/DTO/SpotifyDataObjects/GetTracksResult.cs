using System.Text.Json.Serialization;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace RunnersListLibrary.DTO.SpotifyDataObjects;

public class GetTracksResult
{
    [JsonPropertyName("tracks")]
    public Tracks Tracks { get; set; }
}