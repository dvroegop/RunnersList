using System.Text.Json.Serialization;

namespace RunnersListLibrary.DTO.SongBpm;

public class Search
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("uri")]
    public string Uri { get; set; }

    [JsonPropertyName("tempo")]
    public string Tempo { get; set; }

    [JsonPropertyName("time_sig")]
    public string TimeSig { get; set; }

    [JsonPropertyName("key_of")]
    public string KeyOf { get; set; }

    [JsonPropertyName("open_key")]
    public string OpenKey { get; set; }

    [JsonPropertyName("danceability")]
    public int? Danceability { get; set; }

    [JsonPropertyName("acousticness")]
    public int? Acousticness { get; set; }

    [JsonPropertyName("artist")]
    public Artist Artist { get; set; }

    [JsonPropertyName("album")]
    public Album Album { get; set; }
}