using RunnersListLibrary.Spotify.DTO.SpotifyDataObjects;

namespace RunnersListLibrary.Spotify;

public interface ISpotifyConnector
{
    Task<string> GetSpotifyTokenAsync();

    Task<GetTracksResult> GetSongAsync(string token);
}