using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using EventBudgetPlanner.Cli.Commands;
using EventBudgetPlanner.Cli.Infrastructure;
using EventBudgetPlanner.Core.Abstractions;
using EventBudgetPlanner.Core.Persistence;
using EventBudgetPlanner.Core.Services;

var app = new PlannerApplication();
return await app.RunAsync(args);

internal sealed class PlannerApplication
{
	public async Task<int> RunAsync(string[] args)
	{
		var service = BuildService();
		var rootCommand = CommandFactory.Create(service);
		var parser = new CommandLineBuilder(rootCommand)
			.UseDefaults()
			.Build();

		return await parser.InvokeAsync(args).ConfigureAwait(false);
	}

	private static EventPlannerService BuildService()
	{
		var dataPath = DataPathResolver.Resolve();
		var dataStore = new JsonFileDataStore(dataPath);
		IGuidProvider guidProvider = new SystemGuidProvider();
		return new EventPlannerService(dataStore, guidProvider);
	}
}
