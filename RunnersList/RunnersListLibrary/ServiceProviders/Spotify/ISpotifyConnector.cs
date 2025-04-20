using RunnersListLibrary.DTO;
using RunnersListLibrary.DTO.SpotifyDataObjects;

namespace RunnersListLibrary.ServiceProviders.Spotify;

public interface ISpotifyConnector
{
    Task<string> GetSpotifyTokenAsync();

    Task<GetTracksResult?> GetSongsAsync(string token);

    Task<string> CreatePlaylistAsync(string token, string playlistName, string description, IEnumerable<SpotifySong> songs);
}