using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web;
using Microsoft.Extensions.Options;
using RunnersListLibrary.Secrets;

namespace RunnersListLibrary.Spotify;

// ReSharper disable once ClassNeverInstantiated.Global
internal class SpotifyConnector(IHttpClientFactory httpClientFactory, IOptions<SpotifySecrets> spotifySecrets)
    : ISpotifyConnector
{
    public async Task<string> GetSpotifyToken()
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

        using var http = new HttpListener();
        http.Prefixes.Add(spotifySecrets.Value.RedirectUri);
        http.Start();

        var context = await http.GetContextAsync();
        var request = context.Request;
        var response = context.Response;

        var queryParams = HttpUtility.ParseQueryString(request.Url.Query);
        var receivedState = queryParams["state"];
        var code = queryParams["code"];
        var error = queryParams["error"];

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

    private static async Task<string> GetAccessTokenAsync(string code, string clientId, string clientSecret,
        string redirectUri)
    {
        using (var client = new HttpClient())
        {
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

            using (var document = JsonDocument.Parse(content))
            {
                var root = document.RootElement;
                var accessToken = root.GetProperty("access_token").GetString();
                return accessToken;
            }
        }
    }

    private static void RespondToBrowser(HttpListenerResponse response, string message)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }
}