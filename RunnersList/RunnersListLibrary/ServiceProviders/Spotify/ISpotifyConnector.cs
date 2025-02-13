using RunnersListLibrary.DTO.SpotifyDataObjects;

namespace RunnersListLibrary.ServiceProviders.Spotify;

public interface ISpotifyConnector
{
    Task<string> GetSpotifyTokenAsync();

    Task<GetTracksResult?> GetSongAsync(string token);
}