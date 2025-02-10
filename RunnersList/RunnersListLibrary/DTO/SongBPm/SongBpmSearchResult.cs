using System.Text.Json.Serialization;

namespace RunnersListLibrary.DTO.SongBpm;
// SongBpmSearchResult myDeserializedClass = JsonSerializer.Deserialize<SongBpmSearchResult>(myJsonResponse);

public class SongBpmSearchResult
{
    [JsonPropertyName("search")] public List<Search> Search { get; set; }
}