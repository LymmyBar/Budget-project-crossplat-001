using System.CommandLine;
using System.CommandLine.Invocation;
using EventBudgetPlanner.Cli.Formatting;
using EventBudgetPlanner.Cli.Parsing;
using EventBudgetPlanner.Core.Models;
using EventBudgetPlanner.Core.Services;

namespace EventBudgetPlanner.Cli.Commands;

internal static class CommandFactory
{
    public static RootCommand Create(EventPlannerService service)
    {
        var root = new RootCommand("Budget and staffing planner for event companies.");
        root.AddCommand(BuildEventCommand(service));
        root.AddCommand(BuildBudgetCommand(service));
        root.AddCommand(BuildStaffCommand(service));
        root.AddCommand(BuildSummaryCommand(service));

        root.SetHandler(context =>
        {
            context.Console.Out.Write("Event Budget Planner CLI. Use --help with any command to see options.\n");
        });

        return root;
    }

    private static Command BuildEventCommand(EventPlannerService service)
    {
        var command = new Command("event", "Manage events");
        command.AddCommand(BuildEventCreateCommand(service));
        command.AddCommand(BuildEventListCommand(service));
        command.AddCommand(BuildEventDetailsCommand(service));
        return command;
    }

    private static Command BuildBudgetCommand(EventPlannerService service)
    {
        var command = new Command("budget", "Manage budget line items");
        command.AddCommand(BuildBudgetAddCommand(service));
        return command;
    }

    private static Command BuildStaffCommand(EventPlannerService service)
    {
        var command = new Command("staff", "Manage staff assignments");
        command.AddCommand(BuildStaffAddCommand(service));
        return command;
    }

    private static Command BuildSummaryCommand(EventPlannerService service)
    {
        var command = new Command("summary", "Show budget summaries");
        command.AddCommand(BuildEventSummaryCommand(service));
        command.AddCommand(BuildPortfolioSummaryCommand(service));
        return command;
    }

    private static Command BuildEventCreateCommand(EventPlannerService service)
    {
        var command = new Command("create", "Create a new event");
        var nameOption = new Option<string>("--name", "Event name") { IsRequired = true };
        var dateOption = new Option<string>("--date", "Event date (YYYY-MM-DD)") { IsRequired = true };
        var venueOption = new Option<string>("--venue", () => string.Empty, "Venue or city");
        var budgetOption = new Option<decimal>("--budget", "Target budget") { IsRequired = true };
        var currencyOption = new Option<string>("--currency", () => "USD", "Currency code, e.g. USD");

        command.AddOption(nameOption);
        command.AddOption(dateOption);
        command.AddOption(venueOption);
        command.AddOption(budgetOption);
        command.AddOption(currencyOption);

        command.SetHandler(async context =>
        {
            await CommandRunner.RunAsync(context, async token =>
            {
                var name = Require(context, nameOption);
                var date = ValueParsers.EnsureDate(Require(context, dateOption), nameof(dateOption));
                var venue = context.ParseResult.GetValueForOption(venueOption) ?? string.Empty;
                var budget = Require(context, budgetOption);
                var currency = context.ParseResult.GetValueForOption(currencyOption) ?? "USD";

                var plan = await service.CreateEventAsync(name, date, venue, budget, currency, token).ConfigureAwait(false);
                Console.WriteLine($"Created event '{plan.Name}' with id {plan.Id} on {plan.Date:yyyy-MM-dd} ({MoneyFormatter.Format(plan.TargetBudget, plan.Currency)}).");
            }).ConfigureAwait(false);
        });

        return command;
    }

    private static Command BuildEventListCommand(EventPlannerService service)
    {
        var command = new Command("list", "List all events");

        command.SetHandler(async context =>
        {
            await CommandRunner.RunAsync(context, async token =>
            {
                var events = await service.ListEventsAsync(token).ConfigureAwait(false);
                if (events.Count == 0)
                {
                    Console.WriteLine("No events found. Use 'event create' to add one.");
                    return;
                }

                ConsoleTable.Render(
                    new[] { "Id", "Name", "Date", "Venue", "Budget" },
                    events.Select(e => new[]
                    {
                        e.Id.ToString(),
                        e.Name,
                        e.Date.ToString("yyyy-MM-dd"),
                        string.IsNullOrWhiteSpace(e.Venue) ? "—" : e.Venue,
                        MoneyFormatter.Format(e.TargetBudget, e.Currency)
                    }));
            }).ConfigureAwait(false);
        });

        return command;
    }

    private static Command BuildEventDetailsCommand(EventPlannerService service)
    {
        var command = new Command("details", "Show event details");
        var idOption = new Option<string>("--id", "Event id") { IsRequired = true };
        command.AddOption(idOption);

        command.SetHandler(async context =>
        {
            await CommandRunner.RunAsync(context, async token =>
            {
                var eventId = ValueParsers.EnsureGuid(Require(context, idOption), nameof(idOption));
                var plan = await service.GetEventAsync(eventId, token).ConfigureAwait(false);
                if (plan is null)
                {
                    Console.WriteLine($"Event {eventId} not found.");
                    return;
                }

                PrintEvent(plan);
            }).ConfigureAwait(false);
        });

        return command;
    }

    private static Command BuildBudgetAddCommand(EventPlannerService service)
    {
        var command = new Command("add", "Add a budget line item");
        var eventIdOption = new Option<string>("--event-id", "Event id") { IsRequired = true };
        var categoryOption = new Option<string>("--category", () => "General", "Category name");
        var descriptionOption = new Option<string>("--description", () => string.Empty, "Item description");
        var amountOption = new Option<decimal>("--amount", "Amount") { IsRequired = true };
        var statusOption = new Option<BudgetStatus>("--status", () => BudgetStatus.Planned, "Status: Planned, Committed, Paid");

        command.AddOption(eventIdOption);
        command.AddOption(categoryOption);
        command.AddOption(descriptionOption);
        command.AddOption(amountOption);
        command.AddOption(statusOption);

        command.SetHandler(async context =>
        {
            await CommandRunner.RunAsync(context, async token =>
            {
                var eventId = ValueParsers.EnsureGuid(Require(context, eventIdOption), nameof(eventIdOption));
                var category = context.ParseResult.GetValueForOption(categoryOption) ?? "General";
                var description = context.ParseResult.GetValueForOption(descriptionOption) ?? string.Empty;
                var amount = Require(context, amountOption);
                var status = Require(context, statusOption);

                var item = await service.AddBudgetItemAsync(eventId, category, description, amount, status, token).ConfigureAwait(false);
                Console.WriteLine($"Added {MoneyFormatter.Format(item.Amount, string.Empty)} {item.Category} ({item.Status}) to event {eventId}.");
            }).ConfigureAwait(false);
        });

        return command;
    }

    private static Command BuildStaffAddCommand(EventPlannerService service)
    {
        var command = new Command("add", "Assign a staff member");
        var eventIdOption = new Option<string>("--event-id", "Event id") { IsRequired = true };
        var nameOption = new Option<string>("--name", "Full name") { IsRequired = true };
        var roleOption = new Option<string>("--role", () => string.Empty, "Role or responsibility");
        var rateOption = new Option<decimal>("--rate", "Hourly rate") { IsRequired = true };
        var hoursOption = new Option<double>("--hours", "Booked hours") { IsRequired = true };

        command.AddOption(eventIdOption);
        command.AddOption(nameOption);
        command.AddOption(roleOption);
        command.AddOption(rateOption);
        command.AddOption(hoursOption);

        command.SetHandler(async context =>
        {
            await CommandRunner.RunAsync(context, async token =>
            {
                var eventId = ValueParsers.EnsureGuid(Require(context, eventIdOption), nameof(eventIdOption));
                var name = Require(context, nameOption);
                var role = context.ParseResult.GetValueForOption(roleOption) ?? string.Empty;
                var rate = Require(context, rateOption);
                var hours = Require(context, hoursOption);

                var staff = await service.AddStaffAssignmentAsync(eventId, name, role, rate, hours, token).ConfigureAwait(false);
                Console.WriteLine($"Assigned {staff.FullName} ({staff.Role}) costing {staff.TotalCost:0.00} to event {eventId}.");
            }).ConfigureAwait(false);
        });

        return command;
    }

    private static Command BuildEventSummaryCommand(EventPlannerService service)
    {
        var command = new Command("event", "Show a single event summary");
        var idOption = new Option<string>("--id", "Event id") { IsRequired = true };
        command.AddOption(idOption);

        command.SetHandler(async context =>
        {
            await CommandRunner.RunAsync(context, async token =>
            {
                var eventId = ValueParsers.EnsureGuid(Require(context, idOption), nameof(idOption));
                var summary = await service.GetEventSummaryAsync(eventId, token).ConfigureAwait(false);
                if (summary is null)
                {
                    Console.WriteLine($"Event {eventId} not found.");
                    return;
                }

                PrintEventSummary(summary);
            }).ConfigureAwait(false);
        });

        return command;
    }

    private static Command BuildPortfolioSummaryCommand(EventPlannerService service)
    {
        var command = new Command("portfolio", "Show summary across all events");

        command.SetHandler(async context =>
        {
            await CommandRunner.RunAsync(context, async token =>
            {
                var events = await service.ListEventsAsync(token).ConfigureAwait(false);
                if (events.Count == 0)
                {
                    Console.WriteLine("No events found.");
                    return;
                }

                var summary = service.BuildPortfolioSummary(events);
                Console.WriteLine($"Events tracked: {summary.EventCount}");
                Console.WriteLine($"Total target budget: {summary.TotalTargetBudget:0.00}");
                Console.WriteLine($"Planned: {summary.TotalPlanned:0.00} | Committed: {summary.TotalCommitted:0.00} | Paid: {summary.TotalPaid:0.00}");
                Console.WriteLine($"Staff cost: {summary.TotalStaffCost:0.00}");
                Console.WriteLine($"Over budget events: {summary.OverBudgetEvents}");
            }).ConfigureAwait(false);
        });

        return command;
    }

    private static void PrintEvent(EventPlan plan)
    {
        Console.WriteLine($"Event {plan.Name} ({plan.Id})");
        Console.WriteLine($" Date: {plan.Date:yyyy-MM-dd}");
        Console.WriteLine($" Venue: {plan.Venue}");
        Console.WriteLine($" Budget: {MoneyFormatter.Format(plan.TargetBudget, plan.Currency)}");
        Console.WriteLine();

        if (plan.BudgetItems.Count == 0)
        {
            Console.WriteLine("No budget items yet.");
        }
        else
        {
            Console.WriteLine("Budget Items:");
            ConsoleTable.Render(
                new[] { "Category", "Description", "Amount", "Status" },
                plan.BudgetItems.Select(item => new[]
                {
                    item.Category,
                    string.IsNullOrWhiteSpace(item.Description) ? "—" : item.Description,
                    MoneyFormatter.Format(item.Amount, plan.Currency),
                    item.Status.ToString()
                }));
        }

        Console.WriteLine();

        if (plan.Staff.Count == 0)
        {
            Console.WriteLine("No staff assignments yet.");
        }
        else
        {
            Console.WriteLine("Staff Assignments:");
            ConsoleTable.Render(
                new[] { "Name", "Role", "Hours", "Rate", "Cost" },
                plan.Staff.Select(s => new[]
                {
                    s.FullName,
                    string.IsNullOrWhiteSpace(s.Role) ? "—" : s.Role,
                    s.HoursBooked.ToString("0.##"),
                    MoneyFormatter.Format(s.HourlyRate, plan.Currency),
                    MoneyFormatter.Format(s.TotalCost, plan.Currency)
                }));
        }
    }

    private static void PrintEventSummary(EventSummary summary)
    {
        Console.WriteLine($"Event {summary.Name} ({summary.EventId})");
        Console.WriteLine($" Target budget: {summary.TargetBudget:0.00}");
        Console.WriteLine($" Planned: {summary.TotalPlanned:0.00}");
        Console.WriteLine($" Committed: {summary.TotalCommitted:0.00}");
        Console.WriteLine($" Paid: {summary.TotalPaid:0.00}");
        Console.WriteLine($" Staff cost: {summary.StaffCost:0.00}");
        Console.WriteLine($" Remaining: {summary.RemainingBudget:0.00}");
        Console.WriteLine(summary.IsOverBudget ? " Status: OVER BUDGET" : " Status: on track");
    }

    private static T Require<T>(InvocationContext context, Option<T> option)
    {
        var value = context.ParseResult.GetValueForOption(option);
        if (value is null)
        {
            throw new ArgumentException($"Option {option.Name} is required.");
        }

        return value;
    }
}
