using CoordiNet.CLI;
using Xunit;

namespace CoordiNet.Tests;

public class CommandSystemTests
{
    [Fact]
    public void ModuleRegistry_ShouldRegisterAndExecuteModule()
    {
        var registry = new CommandModuleRegistry();
        registry.Register(new TestModule());

        var module = registry.GetModule("test");

        Assert.NotNull(module);
        Assert.Equal("test", module.Name);

        var state = registry.ExecuteCommand("test", new Dictionary<string, string>
        {
            ["mode"] = "demo"
        });

        Assert.True(state.IsSuccess);
        Assert.Equal("demo", state.Output.Trim());
    }

    private sealed class TestModule : IConsoleModule
    {
        public string Name => "test";
        public string Description => "Test module";
        public string[] Commands => ["test"];

        public CommandExecutionResult Execute(IDictionary<string, string>? parameters = null)
        {
            return CommandExecutionResult.CreateSuccess(parameters?["mode"] ?? "default");
        }

        public void Stop()
        {
        }
    }
}
