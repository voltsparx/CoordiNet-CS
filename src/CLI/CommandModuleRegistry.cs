namespace CoordiNet.CLI;

public sealed class CommandModuleRegistry
{
    private readonly Dictionary<string, IConsoleModule> _modulesByName = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, IConsoleModule> _modulesByCommand = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<IConsoleModule> Modules => _modulesByName.Values;

    public void Register(IConsoleModule module)
    {
        if (module is null)
        {
            throw new ArgumentNullException(nameof(module));
        }

        _modulesByName[module.Name] = module;

        foreach (var command in module.Commands)
        {
            _modulesByCommand[command] = module;
        }
    }

    public IConsoleModule? GetModule(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        if (_modulesByName.TryGetValue(name, out var module))
        {
            return module;
        }

        _modulesByCommand.TryGetValue(name, out module);
        return module;
    }

    public CommandExecutionResult ExecuteCommand(string commandName, IDictionary<string, string>? parameters = null)
    {
        var module = GetModule(commandName);

        if (module is null)
        {
            return CommandExecutionResult.CreateFailure($"Unknown command or module '{commandName}'.");
        }

        return module.Execute(parameters);
    }
}
