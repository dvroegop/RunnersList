using RunnersListLibrary;

namespace RunnersList;

public class RunnerService(IHelloSayer helloSayer) : IRunnerService
{
    #region

    public async Task Run()
    {
        Console.WriteLine("RunnerService is running...");
        await helloSayer.SayHello();
    }

    #endregion
}