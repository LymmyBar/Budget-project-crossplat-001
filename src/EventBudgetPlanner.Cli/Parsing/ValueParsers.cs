namespace EventBudgetPlanner.Cli.Parsing;

internal static class ValueParsers
{
    public static Guid EnsureGuid(string value, string parameterName)
    {
        if (Guid.TryParse(value, out var result))
        {
            return result;
        }

        throw new ArgumentException($"Value '{value}' is not a valid GUID.", parameterName);
    }

    public static DateOnly EnsureDate(string value, string parameterName)
    {
        if (DateOnly.TryParse(value, out var date))
        {
            return date;
        }

        throw new ArgumentException($"Value '{value}' is not a valid date. Use YYYY-MM-DD format.", parameterName);
    }
}
