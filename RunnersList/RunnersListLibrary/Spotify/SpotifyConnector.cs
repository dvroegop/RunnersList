using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace RunnersListLibrary.Spotify;

internal class SpotifyConnector
{
    #region

    // Use snake_case for kernel functions, since that is the standard for Python. 
    [KernelFunction("get_spotify_token")]
    [Description("Gets the Spotify token, using the specified credentials in secrets.")]
    public async Task<string> GetSpotifyToken(SpotifyCredentials credentials)
    {
        return await Task.FromResult("HelloToken");
    }

    [KernelFunction("get_favorite_genres")]
    [Description("Returns the favority genres from this user")]
    public async Task<string[]> GetGenres(string token)
    {
        return await Task.FromResult(new[] { "Rock", "Pop" });
    }
    #endregion
}

public class SpotifyCredentials
{
    public string? UserName { get; set; }
    public string? Password { get; set; }
}
