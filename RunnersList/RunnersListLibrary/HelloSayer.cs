using System.Net;
using System.Net.Http.Headers;
using System.Security.AccessControl;
using System.Text;
using System.Text.Json;
using System.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using RunnersListLibrary.Secrets;
using RunnersListLibrary.Spotify;

namespace RunnersListLibrary;

internal class HelloSayer(
    IHttpClientFactory httpClientFactory,
    IOptions<OpenAiSecrets> azureOpenAiSecrets, 
    IOptions<SpotifySecrets> spotifySecrets) : IHelloSayer
{
    #region


    public async Task SayHello()
    {
        Console.WriteLine(spotifySecrets.Value.RedirectUri);

        var client = httpClientFactory.CreateClient("SpotifyClient");
        string scopes = "playlist-modify-public playlist-modify-private";

        string state = Guid.NewGuid().ToString("N");
        var clientId = spotifySecrets.Value.ClientId;
        var redirectUri = spotifySecrets.Value.RedirectUri;

        string authorizationUrl = $"https://accounts.spotify.com/authorize?client_id={clientId}" +
                                  $"&response_type=code" +
                                  $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                                  $"&scope={Uri.EscapeDataString(scopes)}" +
                                  $"&state={state}";

        Console.WriteLine(authorizationUrl);

        using (var http = new HttpListener())
        {
            http.Prefixes.Add(spotifySecrets.Value.RedirectUri);
            http.Start();

            var context = await http.GetContextAsync();
            var request = context.Request;
            var response = context.Response;

            var queryParams = HttpUtility.ParseQueryString(request.Url.Query);
            string receivedState = queryParams["state"];
            string code = queryParams["code"];
            string error = queryParams["error"];

            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine($"Error during authorization: {error}");
                RespondToBrowser(response, "Authorization failed. You can close this window.");
                return;
            }

            if (state != receivedState)
            {
                Console.WriteLine("State does not match. Possible security issue!");
                RespondToBrowser(response, "Invalid state. You can close this window.");
            }

            string accessToken = await GetAccessTokenAsync(code, clientId, spotifySecrets.Value.ClientSecret, redirectUri);
            if (string.IsNullOrEmpty(accessToken))
            {
                Console.WriteLine("Failed to get the access token.");
                return;
            }

            Console.WriteLine($"Successfully got the token: {accessToken}");
        }
        await Task.CompletedTask;
    }

    static async Task<string> GetAccessTokenAsync(string code, string clientId, string clientSecret, string redirectUri)
    {
        using (var client = new HttpClient())
        {
            string tokenUrl = "https://accounts.spotify.com/api/token";

            // Build the POST parameters
            var parameters = new Dictionary<string, string>
            {
                {"grant_type", "authorization_code"},
                {"code", code},
                {"redirect_uri", redirectUri}
            };

            var requestContent = new FormUrlEncodedContent(parameters);

            // Set the authorization header using client credentials
            var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

            var response = await client.PostAsync(tokenUrl, requestContent);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Error retrieving access token: " + content);
                return null;
            }

            using (var document = JsonDocument.Parse(content))
            {
                var root = document.RootElement;
                string accessToken = root.GetProperty("access_token").GetString();
                return accessToken;
            }
        }
    }
    static void RespondToBrowser(HttpListenerResponse response, string message)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }

    public async Task SayHello2()
    {
        var kernelBuilder = Kernel.CreateBuilder();

        kernelBuilder.AddAzureOpenAIChatCompletion(
            azureOpenAiSecrets.Value.DeploymentName,
            azureOpenAiSecrets.Value.EndPoint,
            azureOpenAiSecrets.Value.ApiKey);


        kernelBuilder.Services.AddLogging(configure => configure.AddConsole().SetMinimumLevel(LogLevel.Error));

        var kernel = kernelBuilder.Build();

        kernel.Plugins.AddFromType<SpotifyConnector>("SpotifyConnector");

        var openAiPromptExecutionSettings = new AzureOpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        var history = new ChatHistory();
        history.AddSystemMessage("You are helping me creating a Spotify Playlist for my running workout. " +
                                 "You use the method to get the Spotify Token, then you can use that in your calls to Spotify." +
                                 "Pass the token to each call to Spotify."+
                                 "After that, ask for the genre they want for their playlist, then get the top 10 songs."+
                                 "When showing the list to the user, call the Spotify connector to create the playlist there, passing in the IDs for the songs.");
        history.AddUserMessage("Please generate a running playlist for me.");

        try
        {
            var canContinue = true;
            while (canContinue)
            {
                var responseBuilder = new StringBuilder();


                await foreach (var response in chatCompletionService.GetStreamingChatMessageContentsAsync(
                                   history,
                                   openAiPromptExecutionSettings,
                                   kernel))
                {
                    var content = response.Content;
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        Console.Write(content);
                        responseBuilder.Append(content);
                    }
                }

                history.AddAssistantMessage(responseBuilder.ToString());

                Console.Write(" > ");
                var responseFromUser = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(responseFromUser) || responseFromUser.ToUpperInvariant().Trim() == "EXIT")
                    canContinue = false;
                else
                    history.AddUserMessage(responseFromUser);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Assistant > " + ex.Message);
        }
    }

    #endregion
}