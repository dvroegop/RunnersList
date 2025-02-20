using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RunnersListLibrary.Secrets;

namespace RunnerListFunctions
{
    public class Function1
    {
        private readonly IOptions<SpotifySecrets> _spotifySecrets;
        private readonly ILogger<Function1> _logger;

        public Function1(IOptions<SpotifySecrets> spotifySecrets, ILogger<Function1> logger)
        {
            _spotifySecrets = spotifySecrets;
            _logger = logger;
        }

        [Function("Function1")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {

            // Get something out of the Azure Key Vault
            

            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult("Welcome to Azure Functions!");
        }
    }
}
