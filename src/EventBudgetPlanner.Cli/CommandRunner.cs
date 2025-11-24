using System.CommandLine;
using System.CommandLine.Invocation;

namespace EventBudgetPlanner.Cli;

internal static class CommandRunner
{
    public static async Task RunAsync(InvocationContext context, Func<CancellationToken, Task> action)
    {
        try
        {
            await action(context.GetCancellationToken()).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            context.Console.Error.Write($"Error: {ex.Message}{Environment.NewLine}");
            context.ExitCode = 1;
        }
    }
}
