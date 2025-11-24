namespace EventBudgetPlanner.Cli.Infrastructure;

internal static class DataPathResolver
{
    public static string Resolve()
    {
        var workingDirectory = Environment.CurrentDirectory;
        var dataDirectory = Path.Combine(workingDirectory, "data");
        return Path.Combine(dataDirectory, "event-plans.json");
    }
}
