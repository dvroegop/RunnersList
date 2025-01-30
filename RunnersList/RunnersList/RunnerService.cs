using RunnersListLibrary;

namespace RunnersList;

public class RunnerService(IHelloSayer helloSayer) : IRunnerService
{
    #region

    public void Run()
    {
        Console.WriteLine("RunnerService is running...");
        helloSayer.SayHello();
    }

    #endregion
}