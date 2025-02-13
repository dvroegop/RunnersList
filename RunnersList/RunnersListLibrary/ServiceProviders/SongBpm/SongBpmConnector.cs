using System.Diagnostics;
using Microsoft.Extensions.Options;
using RunnersListLibrary.DTO.SongBpm;
using RunnersListLibrary.Secrets;

namespace RunnersListLibrary.ServiceProviders.SongBpm;

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

        try
        {
            var parsedContent = System.Text.Json.JsonSerializer.Deserialize<SongBpmSearchResult>(content);

            if (parsedContent == null || parsedContent.Search.Count == 0)
                return -1;

            // Always get the first result
            var bpm = int.Parse(parsedContent.Search[0].Tempo);
            return bpm;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in parsing JSON: {ex.Message}");
            return -1;
        }
        
    }

}

public interface ISongBpmConnector
{   
    Task<int> GetSongBpmAsync(string artist, string title);
}