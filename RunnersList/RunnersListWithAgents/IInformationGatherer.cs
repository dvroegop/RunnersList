namespace RunnersListWithAgents;

public interface IInformationGatherer
{
    Task<string> GetFavoriteMusicGenre();
}