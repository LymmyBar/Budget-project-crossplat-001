namespace EventBudgetPlanner.Cli.Formatting;

internal static class ConsoleTable
{
    public static void Render(IReadOnlyList<string> headers, IEnumerable<IReadOnlyList<string>> rows)
    {
        var materializedRows = rows.Select(row => row.ToArray()).ToList();
        var widths = headers
            .Select((header, index) => Math.Max(header.Length, materializedRows.Select(r => index < r.Length ? r[index].Length : 0).DefaultIfEmpty(0).Max()))
            .ToArray();

        WriteRow(headers, widths);
        WriteSeparator(widths);

        foreach (var row in materializedRows)
        {
            WriteRow(row, widths);
        }
    }

    private static void WriteRow(IReadOnlyList<string> row, IReadOnlyList<int> widths)
    {
        for (var i = 0; i < widths.Count; i++)
        {
            var cell = i < row.Count ? row[i] : string.Empty;
            Console.Write($" {cell.PadRight(widths[i])} ");
            if (i < widths.Count - 1)
            {
                Console.Write("|");
            }
        }

        Console.WriteLine();
    }

    private static void WriteSeparator(IReadOnlyList<int> widths)
    {
        for (var i = 0; i < widths.Count; i++)
        {
            Console.Write(" " + new string('-', widths[i]) + " ");
            if (i < widths.Count - 1)
            {
                Console.Write("+");
            }
        }

        Console.WriteLine();
    }
}
