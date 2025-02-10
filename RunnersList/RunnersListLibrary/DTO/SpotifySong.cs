namespace RunnersListLibrary.DTO;

public class SpotifySong(string title, string artist, string id)
{
    public string Title { get; } = title;
    public string Artist { get; } = artist;
    public string Id { get; } = id;
}