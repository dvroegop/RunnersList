using System.Diagnostics;
using Microsoft.Extensions.Options;
using RunnersListLibrary.DTO.SongBpm;
using RunnersListLibrary.Secrets;

namespace RunnersListLibrary.ServiceProviders.SongBpm;

internal class SongBpmConnector(
    IHttpClientFactory httpClientFactory,
    IOptions<SongBpmSecrets> songBpmSecrets) : ISongBpmConnector
{
    /// <summary>
    /// Retrieves the beats per minute (BPM) of a song based on the provided artist and title.
    /// </summary>
    /// <param name="artist">The name of the artist of the song.</param>
    /// <param name="title">The title of the song.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the BPM of the song as an integer.
    /// Returns -1 if the BPM cannot be determined.
    /// </returns>
    /// <exception cref="HttpRequestException">
    /// Thrown when the HTTP request to the SongBPM API fails.
    /// </exception>
    /// <exception cref="System.Text.Json.JsonException">
    /// Thrown when the response from the SongBPM API cannot be parsed.
    /// </exception>
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

    /// <summary>
    /// Retrieves a list of songs that match the specified beats per minute (BPM).
    /// </summary>
    /// <param name="bpm">The beats per minute (BPM) to filter songs by.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a JSON string
    /// with the list of songs matching the specified BPM.
    /// </returns>
    /// <exception cref="HttpRequestException">
    /// Thrown when the HTTP request to the external API fails or returns a non-success status code.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the API key required for the request is null or empty.
    /// </exception>
    public async Task<string> GetSongsWithGivenBpmAsync(int bpm)
    {
        var apiKey = songBpmSecrets.Value.ApiKey;


        var client = httpClientFactory.CreateClient();
        var baseUrl = $"https://api.getsong.co/tempo/?api_key={apiKey}&bpm={bpm}&limit=100";
        var response = await client.GetAsync(baseUrl);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return content;

    }

}