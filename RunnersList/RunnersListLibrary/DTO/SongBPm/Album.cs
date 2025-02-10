using System.Text.Json.Serialization;

namespace RunnersListLibrary.DTO.SongBpm;

public class Album
{
    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonPropertyName("uri")]
    public string Uri { get; set; }

    [JsonPropertyName("year")]
    public string Year { get; set; }
}