using EventBudgetPlanner.Core.Models;

namespace EventBudgetPlanner.Core.Persistence;

public interface IDataStore
{
    Task<IReadOnlyList<EventPlan>> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(IReadOnlyCollection<EventPlan> events, CancellationToken cancellationToken = default);
}
