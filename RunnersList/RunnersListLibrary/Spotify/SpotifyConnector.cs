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

    [KernelFunction("get_top10_songs_for_genre")]
    [Description("Returns the top 10 songs in a given genre")]
    public async Task<string[]> GetTop10Songs([Description("The favorite genre for this run, ask the user what they want")]FavoriteGenres genre)
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

public enum FavoriteGenres
{
    Rock,
    Pop,
    Eighties
}
