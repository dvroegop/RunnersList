using RunnersListLibrary.DTO;
using RunnersListLibrary.DTO.SpotifyDataObjects;

namespace RunnersListLibrary.ServiceProviders.Spotify;

public interface ISpotifyConnector
{
    Task<string> GetSpotifyTokenAsync();

    Task<IEnumerable<CondensedSpotifySong>> GetSongsAsync(string token, string genre);
}