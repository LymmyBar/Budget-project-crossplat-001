# Event Budget & Personnel Planner

.NET 8 console application for Laboratory Work #1 ("Development of an application with C#"). The chosen topic is a **budget planning and personnel management system for companies that organize events**. The solution targets single-operator teams and keeps the scope intentionally lightweight while still covering domain, tests, and documentation.

## Requirements
- .NET SDK 8.0+
- Git (optional, for version control)

## Solution Layout
```
EventBudgetPlanner.sln
├─ src/EventBudgetPlanner.Core      # Domain models, services, persistence
├─ src/EventBudgetPlanner.Cli       # System.CommandLine powered CLI
├─ tests/EventBudgetPlanner.Tests   # xUnit tests for services + persistence
└─ docs/architecture.md             # High-level design document
```

Domain capabilities:
- Create events with budget target, venue, date, and currency
- Track budget line items with Planned/Committed/Paid statuses
- Assign staff (hourly rate × hours tracked for total labour cost)
- Summaries per event and portfolio (remaining budget, overruns, staffing cost)
- JSON file persistence in `data/event-plans.json`

## Running the CLI
Execute commands from the repository root:

```bash
dotnet run --project src/EventBudgetPlanner.Cli -- [command] [options]
```

Common scenarios:

```bash
# Create an event
dotnet run --project src/EventBudgetPlanner.Cli -- event create \
	--name "Kyiv Tech Expo" \
	--date 2025-03-14 \
	--venue "Unit.City" \
	--budget 15000 \
	--currency USD

# List events
dotnet run --project src/EventBudgetPlanner.Cli -- event list

# Add budget item
dotnet run --project src/EventBudgetPlanner.Cli -- budget add \
	--event-id <event-guid> --category Venue --amount 4200 --status Paid

# Assign staff
dotnet run --project src/EventBudgetPlanner.Cli -- staff add \
	--event-id <event-guid> --name "Anna" --role Coordinator --rate 45 --hours 30

# Summaries
dotnet run --project src/EventBudgetPlanner.Cli -- summary event --id <event-guid>
dotnet run --project src/EventBudgetPlanner.Cli -- summary portfolio
```

All state is stored in `data/event-plans.json` (created on first write). Delete the file to start from a clean slate.

## Tests
Run the automated suite (unit + lightweight persistence tests):

```bash
dotnet test
```

The tests rely on deterministic GUID providers and in-memory stores to keep execution quick and reproducible.

## Documentation
- `docs/architecture.md` — scope, domain model, persistence strategy, and testing plan
- This README — setup, usage, and lab context

Show this project, walk through the CLI, and discuss the tests/design with the instructor to complete the remaining steps of the lab rubric.