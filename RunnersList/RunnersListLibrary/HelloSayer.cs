namespace RunnersListLibrary;

internal class HelloSayer : IHelloSayer
{
    #region

    public void SayHello()
    {
        Console.WriteLine("Hello from HelloSayer!");
    }

    #endregion
}