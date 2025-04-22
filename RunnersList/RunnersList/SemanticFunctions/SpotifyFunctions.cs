using System.ComponentModel;
using Microsoft.SemanticKernel;
using RunnersListLibrary.DTO;
using RunnersListLibrary.DTO.SpotifyDataObjects;
using RunnersListLibrary.ServiceProviders.Spotify;

namespace RunnersList.SemanticFunctions;

public class SpotifyFunctions(ISpotifyConnector spotifyConnector)
{
    #region Private members
    private string? _token;

    private DateTime _tokenAcquired = DateTime.MinValue;
    #endregion


    #region
    #region
    // Use snake_case for kernel functions, since that is the standard for Python. 
    [KernelFunction("get_spotify_token")]
    [Description("Gets the Spotify token, using the specified credentials in secrets.")]
    #endregion
    public async Task<string?> GetSpotifyToken(SpotifyCredentials credentials)
    {
        // Tokens can live for one hour. So we can prevent calling into the API if it less than one hour old.
        if (string.IsNullOrEmpty(_token))
        {
            _token = await spotifyConnector.GetSpotifyTokenAsync();
            _tokenAcquired = DateTime.Now;
        }
        else if (DateTime.Now - _tokenAcquired > TimeSpan.FromHours(1))
        {
            _token = await spotifyConnector.GetSpotifyTokenAsync();
            _tokenAcquired = DateTime.Now;
        }

        return _token;
    }
    #endregion


    #region
    #region
    [KernelFunction("get_top10_songs_for_genre")]
    [Description("Returns the top 10 songs in a given genre")]
    #endregion
    public async Task<CondensedSpotifySong[]?> GetTop10Songs(
        [Description("The favorite genre for this run, ask the user what they want")]
        FavoriteGenres genre)
    {

        var songResult = await spotifyConnector.GetSongsAsync(_token!, genre.ToString());

        return songResult.ToArray();
    }
    #endregion
}