namespace EventBudgetPlanner.Core.Abstractions;

public sealed class SystemGuidProvider : IGuidProvider
{
    public Guid Create() => Guid.NewGuid();
}
