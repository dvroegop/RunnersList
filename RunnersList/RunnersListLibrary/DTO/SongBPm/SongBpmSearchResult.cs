using System.Text.Json.Serialization;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace RunnersListLibrary.DTO.SongBpm;

public class SongBpmSearchResult
{
    [JsonPropertyName("search")] public List<Search> Search { get; set; }
}