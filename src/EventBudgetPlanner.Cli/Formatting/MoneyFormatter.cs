namespace EventBudgetPlanner.Cli.Formatting;

internal static class MoneyFormatter
{
    public static string Format(decimal amount, string currency) =>
        string.IsNullOrWhiteSpace(currency)
            ? amount.ToString("0.00")
            : $"{currency.ToUpperInvariant()} {amount:0.00}";
}
