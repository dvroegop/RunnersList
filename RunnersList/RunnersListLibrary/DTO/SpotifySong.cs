namespace RunnersListLibrary.DTO;

public class SpotifySong2(string title, string artist, string id)
{
    public string Title { get; } = title;
    public string Artist { get; } = artist;
    public int Bpm { get; set; }
    public string Id { get; } = id;
}