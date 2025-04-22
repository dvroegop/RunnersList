using System.ComponentModel;
using Microsoft.SemanticKernel;
using RunnersListWithAgents.ExposedFunctions;

namespace RunnersList.SemanticFunctions;

internal class MiscFunctions(IInformationGatherer informationGatherer)
{
    [KernelFunction("get_users_favorite_music_genre")]
    [Description("Asks the user for their favority music genre to build the runners list with.")]
    public async Task<string> GetFavoriteMusicGenre()
    {
        var genre = await informationGatherer.GetFavoriteMusicGenre();
        return genre;
    }
}