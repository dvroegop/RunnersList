using RunnersListLibrary.DTO;
using RunnersListLibrary.DTO.SpotifyDataObjects;

namespace RunnersListLibrary.ServiceProviders.Spotify;

public interface ISpotifyConnector
{
    Task<string> GetSpotifyTokenAsync();

    Task<string> GetSongsAsync(string token, string genre);

    Task<string> CreatePlaylistAsync(string token, string playlistName, string description, IEnumerable<SpotifySong> songs);
}