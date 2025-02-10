using Microsoft.Extensions.Options;
using RunnersListLibrary.DTO.SongBpm;
using RunnersListLibrary.Secrets;

namespace RunnersListLibrary.SongBpm;

internal class SongBpmConnector(
    IHttpClientFactory httpClientFactory,
    IOptions<SongBpmSecrets> songBpmSecrets) : ISongBpmConnector
{
    public async Task<int> GetSongBpmAsync(string artist, string title)
    {
        var client = httpClientFactory.CreateClient();
        var songName = title.Replace(' ', '+');
        var artistName = artist.Replace(' ', '+');
        var apiKey = songBpmSecrets.Value.ApiKey;

        var baseUrl =
            $"https://api.getsong.co/search/?api_key={apiKey}&type=both&lookup=song:{songName}+artist:{artistName}";
        var response = await client.GetAsync(baseUrl);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        var parsedContent = System.Text.Json.JsonSerializer.Deserialize<SongBpmSearchResult>(content);
        return 42;
    }
}

public interface ISongBpmConnector
{   
    Task<int> GetSongBpmAsync(string artist, string title);
}