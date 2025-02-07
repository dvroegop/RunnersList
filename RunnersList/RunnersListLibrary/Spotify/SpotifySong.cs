namespace RunnersListLibrary.Spotify;

public class SpotifySong(string title, string artist, Guid id)
{
    public string Title { get; } = title;
    public string Artist { get; } = artist;
    public Guid Id { get; } = id;
}