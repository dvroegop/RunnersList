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
        var result = new List<string>();
        switch (genre)
        {
            case FavoriteGenres.Eighties:
                result.Add("Take on me");
                result.Add("Sanctify yourself");
                break;

            case FavoriteGenres.Rock:
                result.Add("Bohemian Rhapsody");
                result.Add("Born to run");
                break;

            case FavoriteGenres.Pop:
                result.Add("Shallow");
                result.Add("Frozen");

                break;

            default:
                result.Add("Leef");
                break;
        }
        return await Task.FromResult(result.ToArray());
    }


    [KernelFunction("get_favorite_genres")]
    [Description("Returns the favorite genres for the user")]
    public async Task<FavoriteGenres[]> GetFavoriteGenres()
    {
        return await Task.FromResult(new [] {FavoriteGenres.Eighties, FavoriteGenres.Pop, FavoriteGenres.Rock});
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
