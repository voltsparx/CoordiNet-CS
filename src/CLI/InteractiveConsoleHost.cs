namespace CoordiNet.CLI;

public sealed class InteractiveConsoleHost
{
    private readonly CommandModuleRegistry _registry = new();
    private readonly Dictionary<string, string> _configuration = new(StringComparer.OrdinalIgnoreCase);
    private IConsoleModule? _activeModule;
    private bool _keepRunning = true;

    public InteractiveConsoleHost()
    {
        RegisterBuiltInModules();
    }

    public void RegisterDefaultModules()
    {
        RegisterBuiltInModules();
    }

    public void RegisterBuiltInModules()
    {
        _registry.Register(new DemoModule());
        _registry.Register(new TemplateProvisionModule());
        _registry.Register(new MirrorLocalModule());
        _registry.Register(new BundleZipModule());
    }

    public async Task RunAsync(string[]? args = null)
    {
        if (args is { Length: > 0 })
        {
            await RunCommandLineAsync(args);
            return;
        }

        ConsoleUI.ShowBanner();
        ConsoleUI.ShowMenu();

        while (_keepRunning)
        {
            var input = ConsoleUI.Ask("coordinet> ");
            if (string.IsNullOrWhiteSpace(input))
            {
                continue;
            }

            var normalizedInput = input.Trim();
            if (int.TryParse(normalizedInput, out var menuSelection))
            {
                var mappedCommand = MapMenuSelection(menuSelection);
                if (!string.IsNullOrWhiteSpace(mappedCommand))
                {
                    normalizedInput = mappedCommand;
                }
            }

            var result = ProcessCommand(normalizedInput);

            if (!string.IsNullOrWhiteSpace(result.Output))
            {
                Console.WriteLine(result.Output);
            }

            if (!string.IsNullOrWhiteSpace(result.Error))
            {
                ConsoleUI.Error(result.Error);
            }

            if (!_keepRunning)
            {
                break;
            }
        }
    }

    public async Task RunCommandLineAsync(string[] args)
    {
        if (args.Any(a => a.StartsWith("-", StringComparison.Ordinal)))
        {
            var parsed = CommandLine.Parse(args);
            if (parsed.ShowHelp)
            {
                CommandLine.PrintHelp();
                return;
            }

            if (!string.IsNullOrWhiteSpace(parsed.TemplatePath))
            {
                _configuration["template"] = parsed.TemplatePath;
            }

            _configuration["port"] = parsed.Port.ToString();
            _configuration["tunnel"] = parsed.TunnelProvider;

            var commandResult = _registry.ExecuteCommand("demo", _configuration);
            if (!string.IsNullOrWhiteSpace(commandResult.Output))
            {
                Console.WriteLine(commandResult.Output);
            }

            if (!string.IsNullOrWhiteSpace(commandResult.Error))
            {
                ConsoleUI.Error(commandResult.Error);
            }

            return;
        }

        var commandText = string.Join(" ", args);
        var result = ProcessCommand(commandText);

        if (!string.IsNullOrWhiteSpace(result.Output))
        {
            Console.WriteLine(result.Output);
        }

        if (!string.IsNullOrWhiteSpace(result.Error))
        {
            ConsoleUI.Error(result.Error);
        }

        await Task.CompletedTask;
    }

    public CommandExecutionResult ProcessCommand(string input)
    {
        var tokens = input.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (tokens.Length == 0)
        {
            return CommandExecutionResult.CreateSuccess(string.Empty);
        }

        var verb = tokens[0].Trim();

        return verb.ToLowerInvariant() switch
        {
            "help" => ShowHelp(),
            "list" => ListModules(),
            "load" => LoadModule(tokens),
            "configure" => Configure(tokens),
            "run" => RunModule(tokens),
            "stop" => StopActiveModule(),
            "provisions-default" or "provision-default" or "template-provision" => ExecuteDirect(tokens),
            "mirror-local" or "mirror" or "clone-local" => ExecuteDirect(tokens),
            "bundle-zip" or "bundle" or "zip-archive" => ExecuteDirect(tokens),
            "exit" or "quit" => Exit(),
            _ => ExecuteDirect(tokens)
        };
    }

    private CommandExecutionResult ShowHelp()
    {
        var lines = new[]
        {
            "Available commands:",
            "  help                        Show this help message",
            "  list                        List installed modules",
            "  load <module>               Load a module into the current session",
            "  configure key=value         Set a module option",
            "  run [module]                Execute the active module or the named one",
            "  stop                        Stop the active module",
            "  provisions-default          Provision our built-in simulation routes",
            "  mirror-local                Mirror a local HTML workspace to /external/<name>",
            "  bundle-zip                 Bundle the current generated deployment into a ZIP",
            "  exit / quit                 Exit the console",
            "",
            "Example:",
            "  ./coordinet-cs --command \"provisions-default\"",
            "  ./coordinet-cs --command \"mirror-local\"",
            "  ./coordinet-cs --command \"bundle-zip\"",
            "  load demo",
            "  configure port=9000 tunnel=ngrok",
            "  run demo"
        };

        return CommandExecutionResult.CreateSuccess(string.Join(Environment.NewLine, lines));
    }

    private CommandExecutionResult ListModules()
    {
        var lines = new List<string>
        {
            "Loaded modules:"
        };

        foreach (var module in _registry.Modules)
        {
            lines.Add($"  - {module.Name}: {module.Description}");
        }

        return CommandExecutionResult.CreateSuccess(string.Join(Environment.NewLine, lines));
    }

    private CommandExecutionResult LoadModule(string[] tokens)
    {
        if (tokens.Length < 2)
        {
            return CommandExecutionResult.CreateFailure("Usage: load <module>");
        }

        var module = _registry.GetModule(tokens[1]);
        if (module is null)
        {
            return CommandExecutionResult.CreateFailure($"Module '{tokens[1]}' was not found.");
        }

        _activeModule = module;
        return CommandExecutionResult.CreateSuccess($"Module '{module.Name}' is loaded and ready.");
    }

    private CommandExecutionResult Configure(string[] tokens)
    {
        if (tokens.Length < 2)
        {
            return CommandExecutionResult.CreateFailure("Usage: configure key=value [key=value ...]");
        }

        foreach (var token in tokens.Skip(1))
        {
            var index = token.IndexOf('=');
            if (index <= 0)
            {
                return CommandExecutionResult.CreateFailure($"Invalid setting '{token}'. Use key=value syntax.");
            }

            var key = token.Substring(0, index).Trim();
            var value = token.Substring(index + 1).Trim();
            _configuration[key] = value;
        }

        return CommandExecutionResult.CreateSuccess($"Configuration updated with {tokens.Length - 1} setting(s).");
    }

    private CommandExecutionResult RunModule(string[] tokens)
    {
        var target = tokens.Length > 1 ? tokens[1] : _activeModule?.Name;
        if (string.IsNullOrWhiteSpace(target))
        {
            return CommandExecutionResult.CreateFailure("No module is loaded. Use 'load <module>' first.");
        }

        var result = _registry.ExecuteCommand(target, _configuration);
        if (result.IsSuccess)
        {
            _activeModule ??= _registry.GetModule(target);
        }

        return result;
    }

    private CommandExecutionResult StopActiveModule()
    {
        if (_activeModule is null)
        {
            return CommandExecutionResult.CreateFailure("No active module to stop.");
        }

        _activeModule.Stop();
        return CommandExecutionResult.CreateSuccess($"Stopped module '{_activeModule.Name}'.");
    }

    private CommandExecutionResult Exit()
    {
        _keepRunning = false;
        return CommandExecutionResult.CreateSuccess("Goodbye.");
    }

    private static string? MapMenuSelection(int menuSelection)
    {
        return menuSelection switch
        {
            1 => "demo",
            2 => "demo",
            3 => "demo",
            4 => "provisions-default",
            5 => "mirror-local",
            6 => "bundle-zip",
            7 => "help",
            8 => "exit",
            _ => null
        };
    }

    private CommandExecutionResult ExecuteDirect(string[] tokens)
    {
        var moduleName = tokens[0];
        var result = _registry.ExecuteCommand(moduleName, _configuration);

        if (result.IsSuccess)
        {
            _activeModule ??= _registry.GetModule(moduleName);
        }

        return result;
    }
}
