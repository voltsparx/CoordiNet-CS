namespace CoordiNet.CLI;

public static class Prompts
{
    public static string AskForInput(string message)
    {
        Console.Write($"{message}: ");
        return Console.ReadLine() ?? string.Empty;
    }
}