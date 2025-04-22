namespace RunnersListLibrary.GenericFunctions;

public interface IInformationGatherer
{
    Task<string> GetFavoriteMusicGenre();
}