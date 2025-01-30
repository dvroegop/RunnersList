using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using RunnersListLibrary.Secrets;

namespace RunnersListLibrary;

internal class HelloSayer(IOptions<OpenAiSecrets> secrets) : IHelloSayer
{
    public void SayHello()
    {
        Console.WriteLine("Hello from HelloSayer!");
        Console.WriteLine($"Secret: {secrets.Value.ApiKey}");
        var kernel = Kernel.CreateBuilder();
    }
}
