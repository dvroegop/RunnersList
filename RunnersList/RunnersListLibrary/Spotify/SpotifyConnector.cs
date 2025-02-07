using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace RunnersListLibrary.Spotify;

// ReSharper disable once ClassNeverInstantiated.Global
internal class SpotifyConnector : ISpotifyConnector
{
    public async Task<string> GetSpotifyToken()
    {
        
    }
}

public interface ISpotifyConnector
{
    Task<string> GetSpotifyToken();
}