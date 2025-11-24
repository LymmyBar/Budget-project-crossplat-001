# Event Budget & Personnel Planner

## Goal
A lightweight .NET 9 console application that helps single operators plan event budgets, track staff assignments, and monitor financial health for small event operations companies.

## Functional Scope
- Manage events with metadata (date, venue, target budget).
- Track planned/committed/paid budget items per event.
- Assign staff members with rates and booked hours.
- Summaries per event and portfolio (totals, overruns, staffing costs).
- JSON file persistence for offline usage.
- Simple CLI with subcommands (create, list, details, add-budget, add-staff, summary).

## Solution Topology
```
EventBudgetPlanner.sln
 ├─ src/EventBudgetPlanner.Core        # Domain models, DTOs, services, persistence
 ├─ src/EventBudgetPlanner.Cli         # Console UI (System.CommandLine)
 └─ tests/EventBudgetPlanner.Tests     # xUnit unit/integration tests
```

### Core Library
- `Entities`: `EventPlan`, `BudgetItem`, `StaffAssignment`.
- `Value Objects`: `Money`, `EventSummary`, `PortfolioSummary`.
- `Services`: `EventPlannerService` orchestrates CRUD and calculations.
- `Persistence`: `IDataStore` abstraction with `JsonFileDataStore` implementation.
- Use `System.Text.Json` for serialization with camelCase naming.

### CLI Application
- Uses `System.CommandLine` to provide subcommands: `event create`, `event list`, `event details`, `budget add`, `staff add`, `summary event`, `summary org`.
- Validates inputs, delegates to `EventPlannerService`.
- Friendly table-style console output.

### Tests
- xUnit project testing:
  - Domain calculations (totals, remaining budget, staff costs).
  - Persistence read/write round-trips (using temp file fixture).
  - CLI smoke tests via `System.CommandLine` invocation helper.

### Data Model Snapshot
```
EventPlan
 ├─ Id (Guid)
 ├─ Name
 ├─ Date (DateOnly)
 ├─ Venue
 ├─ TargetBudget (decimal)
 ├─ Currency (string)
 ├─ BudgetItems: List<BudgetItem>
 └─ Staff: List<StaffAssignment>

BudgetItem
 ├─ Id (Guid)
 ├─ Category
 ├─ Description
 ├─ Amount (decimal)
 └─ Status (Planned | Committed | Paid)

StaffAssignment
 ├─ Id (Guid)
 ├─ FullName
 ├─ Role
 ├─ HourlyRate
 └─ HoursBooked
```

### Persistence Strategy
- Default data file: `data/event-plans.json` (created lazily).
- Data store loads entire file into memory on startup, writes on each mutation.
- Thread safety ensured via simple `SemaphoreSlim` (single-user scenario).

### Testing Strategy
- Deterministic IDs in tests via injected `IGuidProvider`.
- Use `TempDirectoryFixture` to isolate persistence tests.
- Cover regression-prone calculations (overrun detection, cost rollups).

### Documentation Plan
- Expand `README.md` with setup, usage, command reference, sample outputs.
- Keep `docs/architecture.md` for teacher review.
