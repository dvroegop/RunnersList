using System.ComponentModel;
using Microsoft.SemanticKernel;
using RunnersListLibrary.DTO;
using RunnersListLibrary.SongBpm;

namespace RunnersListLibrary.SemanticFunctions;

public class SongBpmFunctions(ISongBpmConnector songBpmConnector)
{
    [KernelFunction]
    [Description(
        "Looks for a song, given the name of the artist and the title of the song, then returns the beats per minute (BPM) for that song.")]
    public async Task<SpotifySong> GetSongBpm(SpotifySong spotifySong)
    {
        var bpm = await songBpmConnector.GetSongBpmAsync(spotifySong.Artist, spotifySong.Title);
        spotifySong.Bpm = bpm;

        return spotifySong;
    }
}