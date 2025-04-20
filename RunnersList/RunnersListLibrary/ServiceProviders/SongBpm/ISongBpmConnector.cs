namespace RunnersListLibrary.ServiceProviders.SongBpm;

public interface ISongBpmConnector
{   
    Task<int> GetSongBpmAsync(string artist, string title);
    Task<string> GetSongsWithGivenBpmAsync(int bpm);
}