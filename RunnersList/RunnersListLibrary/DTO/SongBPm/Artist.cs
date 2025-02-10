using System.Text.Json.Serialization;

namespace RunnersListLibrary.DTO.SongBpm;

public class Artist
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("uri")]
    public string Uri { get; set; }

    [JsonPropertyName("genres")]
    public List<string> Genres { get; set; }

    [JsonPropertyName("from")]
    public string From { get; set; }

    [JsonPropertyName("mbid")]
    public string Mbid { get; set; }
}