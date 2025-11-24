using System.Text.Json;
using System.Text.Json.Serialization;
using EventBudgetPlanner.Core.Models;

namespace EventBudgetPlanner.Core.Persistence;

public sealed class JsonFileDataStore : IDataStore
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _options;

    public JsonFileDataStore(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    public async Task<IReadOnlyList<EventPlan>> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            return new List<EventPlan>();
        }

        await using var stream = File.OpenRead(_filePath);
        var payload = await JsonSerializer.DeserializeAsync<List<EventPlan>>(stream, _options, cancellationToken)
                      ?? new List<EventPlan>();
        return payload;
    }

    public async Task SaveAsync(IReadOnlyCollection<EventPlan> events, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(_filePath);
        await JsonSerializer.SerializeAsync(stream, events, _options, cancellationToken);
    }
}
