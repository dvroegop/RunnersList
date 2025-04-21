using RunnersListLibrary;

namespace RunnersList;

public class RunnerService(IKickstarter kickStarter) : IRunnerService
{
    #region

    public async Task Run()
    {
        Console.WriteLine("RunnerService is running...");
        await kickStarter.TestApi();

       // await kickStarter.StartAsync();
    }

    #endregion
}