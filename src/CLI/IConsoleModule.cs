namespace CoordiNet.CLI;

public interface IConsoleModule
{
    string Name { get; }
    string Description { get; }
    string[] Commands { get; }
    CommandExecutionResult Execute(IDictionary<string, string>? parameters = null);
    void Stop();
}
