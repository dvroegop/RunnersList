using System.ComponentModel;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using RunnersListLibrary.Secrets;

namespace RunnersListLibrary.Spotify
{
    internal class SpotifyConnector(IOptions<OpenAiSecrets> secrets) : ISpotifyConnector
    {
        private readonly IOptions<OpenAiSecrets> _secrets = secrets;

        #region

        [KernelFunction("SpotifyConnect")]
        [Description("Connects to the spotify service.")]
        public void Connect()
        {
            Console.WriteLine("Connecting to Spotify...");
        }
        #endregion
    }
}
