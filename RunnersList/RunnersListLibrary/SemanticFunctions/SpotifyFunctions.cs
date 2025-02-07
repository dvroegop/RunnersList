using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.SemanticKernel;
using RunnersListLibrary.Spotify;

namespace RunnersListLibrary.SemanticFunctions;

public class SpotifyFunctions(ISpotifyConnector spotifyConnector)
{
    private readonly ISpotifyConnector _spotifyConnector = spotifyConnector;

    private string? _token;
    private DateTime _tokenAcquired = DateTime.MinValue;
    // Use snake_case for kernel functions, since that is the standard for Python. 
    [KernelFunction("get_spotify_token")]
    [Description("Gets the Spotify token, using the specified credentials in secrets.")]
    public async Task<string?> GetSpotifyToken(SpotifyCredentials credentials)
    {
        // Tokens can live for one hour. So we can prevent calling into the API if it less than one hour old.
        if (!string.IsNullOrEmpty(_token))
        {
            _token = await _spotifyConnector.GetSpotifyToken();
            _tokenAcquired = DateTime.Now;
        }
        else if (DateTime.Now - _tokenAcquired > TimeSpan.FromHours(1))
        {
            _token = await _spotifyConnector.GetSpotifyToken();
            _tokenAcquired = DateTime.Now;
        }

        return _token;
    }

    [KernelFunction("get_top10_songs_for_genre")]
    [Description("Returns the top 10 songs in a given genre")]
    public async Task<SpotifySong[]> GetTop10Songs(
        [Description("The favorite genre for this run, ask the user what they want")]
        FavoriteGenres genre)
    {
        var result = new List<SpotifySong>();
        switch (genre)
        {
            case FavoriteGenres.Eighties:
                result.Add(new SpotifySong("Take on me", "A-Ha", Guid.NewGuid()));
                result.Add(new SpotifySong("Sanctify yourself", "Simple Minds", Guid.NewGuid()));
                break;

            case FavoriteGenres.Rock:
                result.Add(new SpotifySong("Bohemian Rhapsody", "Queen", Guid.NewGuid()));
                result.Add(new SpotifySong("Born to run", "Bruce Springsteen", Guid.NewGuid()));
                break;

            case FavoriteGenres.Pop:
                result.Add(new SpotifySong("Shallow", "Lady Gaga", Guid.NewGuid()));
                result.Add(new SpotifySong("This love", "Maroon 5", Guid.NewGuid()));

                break;

            default:
                result.Add(new SpotifySong("Leef", "Andre Hazes", Guid.NewGuid()));
                break;
        }

        return await Task.FromResult(result.ToArray());
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