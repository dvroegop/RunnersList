namespace RunnersListLibrary.Spotify;

public interface ISpotifyConnector
{
    Task<string> GetSpotifyToken();
}