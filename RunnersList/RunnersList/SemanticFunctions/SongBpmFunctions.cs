using System.ComponentModel;
using Microsoft.SemanticKernel;
using RunnersListLibrary.DTO.SpotifyDataObjects;
using RunnersListLibrary.ServiceProviders.SongBpm;

namespace RunnersList.SemanticFunctions;

public class SongBpmFunctions(ISongBpmConnector songBpmConnector)
{
    [KernelFunction]
    [Description(
        "Looks for a song, given the name of the artist and the title of the song, then returns the beats per minute (BPM) for that song.")]
    public async Task<CondensedSpotifySong> GetSongBpm(CondensedSpotifySong spotifySong)
    {
        var bpm = await songBpmConnector.GetSongBpmAsync(spotifySong.Artist, spotifySong.Title);
        spotifySong.Bpm = bpm;

        return spotifySong;
    }
}