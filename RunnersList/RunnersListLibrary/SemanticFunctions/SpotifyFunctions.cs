using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using RunnersListLibrary.DTO;
using RunnersListLibrary.ServiceProviders.Spotify;

namespace RunnersListLibrary.SemanticFunctions;

public class SpotifyFunctions(ISpotifyConnector spotifyConnector)
{
    private string? _token;

    private DateTime _tokenAcquired = DateTime.MinValue;

    // Use snake_case for kernel functions, since that is the standard for Python. 
    [KernelFunction("get_spotify_token")]
    [Description("Gets the Spotify token, using the specified credentials in secrets.")]
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

    [KernelFunction("get_top50_songs_for_genre")]
    [Description("Returns the top 50 songs in a given genre")]
    public async Task<SpotifySong[]?> GetTop50Songs(
        [Description("The favorite genre for this run, ask the user what they want")]
        FavoriteGenres genre)
    {

        var songResult = await spotifyConnector.GetSongsAsync(_token!);
        var result = new List<SpotifySong>();
        if (songResult == null)
            return default;

        foreach (var item in songResult.Tracks.Items)
        {
            var artist = item.Artists[0].Name;
            var songName = item.Name;
            var song = new SpotifySong(songName, artist, item.Id);
            result.Add(song);
        }

        return result.ToArray();
    }

    [KernelFunction("create_playlist_in_spotify")]
    [Description("Creates a playlist in Spotify with the given songs")]
    public async Task CreatePlaylistInSpotify(SpotifySong[] songs, string token)
    {
        Console.WriteLine($"Creating playlist with {songs.Length} songs");
        await Task.CompletedTask;
    }


    [KernelFunction("get_favorite_genres")]
    [Description("Returns the favorite genres for the user")]
    public async Task<FavoriteGenres[]> GetFavoriteGenres()
    {
        return await Task.FromResult(new[] { FavoriteGenres.Eighties, FavoriteGenres.Pop, FavoriteGenres.Rock });
    }
}