namespace CoordiNet.CLI;

public sealed class CommandExecutionResult
{
    public bool IsSuccess { get; }
    public string Output { get; }
    public string? Error { get; }
    public IDictionary<string, string>? Metadata { get; }

    private CommandExecutionResult(bool success, string output, string? error = null, IDictionary<string, string>? metadata = null)
    {
        IsSuccess = success;
        Output = output;
        Error = error;
        Metadata = metadata;
    }

    public static CommandExecutionResult CreateSuccess(string output, IDictionary<string, string>? metadata = null)
    {
        return new CommandExecutionResult(true, output, null, metadata);
    }

    public static CommandExecutionResult CreateFailure(string error, string? output = null, IDictionary<string, string>? metadata = null)
    {
        return new CommandExecutionResult(false, output ?? string.Empty, error, metadata);
    }
}
