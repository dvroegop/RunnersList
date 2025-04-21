using Azure.AI.Projects;

namespace RunnersListWithAgents.ToolFunctions;

internal class InformationGathererFunctions
{
    public FunctionToolDefinition GetUserFavoriteMusicGenreTool= new("getUserFavoriteMusicGenre",
        "Gets the users favorite music genre");

}