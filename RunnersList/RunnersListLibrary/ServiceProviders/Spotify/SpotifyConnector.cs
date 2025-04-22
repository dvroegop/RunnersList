using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web;
using Microsoft.Extensions.Options;
using RunnersListLibrary.DTO.SpotifyDataObjects;
using RunnersListLibrary.Secrets;

namespace RunnersListLibrary.ServiceProviders.Spotify;

// ReSharper disable once ClassNeverInstantiated.Global
internal class SpotifyConnector(
    IHttpClientFactory httpClientFactory,
    IOptions<SpotifySecrets> spotifySecrets)
    : ISpotifyConnector
{
    
    /// <summary>
    /// Initiates the Spotify authorization process to retrieve an access token.
    /// </summary>
    /// <remarks>
    /// This method launches a browser to authenticate the user with Spotify and listens for the redirect 
    /// containing the authorization code. It then exchanges the authorization code for an access token.
    /// </remarks>
    /// <returns>
    /// A <see cref="Task{TResult}"/> representing the asynchronous operation, with a result of the Spotify access token as a <see cref="string"/>.
    /// </returns>
    /// <exception cref="Exception">
    /// Thrown when a valid request cannot be retrieved or other unexpected errors occur during the process.
    /// </exception>
    /// <exception cref="SpotifyException">
    /// Thrown when there is an error during Spotify authorization, such as missing or invalid state, 
    /// missing authorization code, or failure to retrieve the access token.
    /// </exception>
    public async Task<string> GetSpotifyTokenAsync()
    {
        var client = httpClientFactory.CreateClient("SpotifyClient");
        var scopes = "playlist-modify-public playlist-modify-private";

        var state = Guid.NewGuid().ToString("N");
        var clientId = spotifySecrets.Value.ClientId;
        var redirectUri = spotifySecrets.Value.RedirectUri;

        var authorizationUrl = $"https://accounts.spotify.com/authorize?client_id={clientId}" +
                               $"&response_type=code" +
                               $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                               $"&scope={Uri.EscapeDataString(scopes)}" +
                               $"&state={state}";

        Console.WriteLine(authorizationUrl);
        LaunchBrowser(authorizationUrl);

        using var http = new HttpListener();
        http.Prefixes.Add(spotifySecrets.Value.RedirectUri);
        http.Start();

        var context = await http.GetContextAsync();
        var request = context.Request;
        var response = context.Response;

        if (request == null || request.Url == null)
            throw new Exception("Cannot retrieve a valid request.");

        var queryParams = HttpUtility.ParseQueryString(request.Url.Query);
        var receivedState = queryParams["state"];
        var code = queryParams["code"];
        var error = queryParams["error"];

        if (string.IsNullOrWhiteSpace(code))
            throw new SpotifyException("No code received from Spotify.");

        if (!string.IsNullOrEmpty(error))
        {
            var errorMessage = $"Error during authorization: {error}";
            RespondToBrowser(response, "Authorization failed. You can close this window.");
            throw new SpotifyException(errorMessage);
        }

        if (state != receivedState)
        {
            var errorMessage = "State does not match. Possible security issue!";
            RespondToBrowser(response, "Invalid state. You can close this window.");
            throw new SpotifyException(errorMessage);
        }

        var accessToken = await GetAccessTokenAsync(code, clientId, spotifySecrets.Value.ClientSecret, redirectUri);
        if (string.IsNullOrEmpty(accessToken))
        {
            var errorMessage = "Failed to get the access token.";
            throw new SpotifyException(errorMessage);
        }

        Debug.WriteLine($"Successfully got the token: {accessToken}");

        return accessToken;
    }

    
    /// <summary>
    /// Retrieves a collection of songs from Spotify based on the specified genre.
    /// </summary>
    /// <param name="token">
    /// The Spotify access token used for authentication.
    /// </param>
    /// <param name="genre">
    /// The genre of songs to search for.
    /// </param>
    /// <returns>
    /// A collection of <see cref="CondensedSpotifySong"/> objects representing the retrieved songs.
    /// </returns>
    /// <exception cref="SpotifyException">
    /// Thrown when no songs are found or an error occurs during the request.
    /// </exception>
    public async Task<IEnumerable<CondensedSpotifySong>> GetSongsAsync(string token, string genre)
    {
        var httpClient = httpClientFactory.CreateClient("SpotifyClient");
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var results = new List<CondensedSpotifySong>();

        var rndOffset = new Random().Next(1, 10);
        var response =
            await httpClient.GetAsync(
                $"https://api.spotify.com/v1/search?q=genre%3A{genre}&type=track&limit=10&offset={rndOffset}");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<GetTracksResult>(content);
        if (data == null)
            throw new SpotifyException("Cannot find any song.");

        foreach (var song in data.Tracks.Items)
        {
            var condensedSong = ConvertSongToCondensed(song);
            results.Add(condensedSong);
        }

        return results;
    }


    #region Private helper methods

    private async Task<string?> GetAccessTokenAsync(string code, string clientId, string clientSecret,
        string redirectUri)
    {
        using var client = new HttpClient();

        var tokenUrl = "https://accounts.spotify.com/api/token";

        // Build the POST parameters
        var parameters = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "code", code },
            { "redirect_uri", redirectUri }
        };

        var requestContent = new FormUrlEncodedContent(parameters);

        // Set the authorization header using client credentials
        var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

        var response = await client.PostAsync(tokenUrl, requestContent);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = $"Error retrieving access token: {content}";
            throw new SpotifyException(errorMessage);
        }

        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;
        var accessToken = root.GetProperty("access_token").GetString();
        return accessToken;
    }

    private void RespondToBrowser(HttpListenerResponse response, string message)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }

    private void LaunchBrowser(string url)
    {
        var edgePath = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe";

        Process.Start(new ProcessStartInfo
        {
            FileName = edgePath,
            Arguments = url,
            UseShellExecute = true
        });
    }

    private CondensedSpotifySong ConvertSongToCondensed(Item spotifySong)
    {
        var result = new CondensedSpotifySong
        {
            Id = spotifySong.Id,
            Title = spotifySong.Name,
            Artist = spotifySong.Artists[0].Name
        };

        return result;
    }

    #endregion
}