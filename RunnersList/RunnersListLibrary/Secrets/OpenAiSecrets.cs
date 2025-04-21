namespace RunnersListLibrary.Secrets;

public class OpenAiSecrets
{
    public required string DeploymentName { get; init; }
    public required string ApiKey { get; init; }
// Make sure to only use the actual URL part of the deployment, when copying 
    // from the Azure Portal.
    public required string EndPoint { get; init; }

    public required string AiFoundryConnectionString { get; init; }
}