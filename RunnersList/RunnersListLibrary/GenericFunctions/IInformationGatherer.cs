namespace RunnersListWithAgents.ExposedFunctions;

public interface IInformationGatherer
{
    Task<string> GetFavoriteMusicGenre();
}