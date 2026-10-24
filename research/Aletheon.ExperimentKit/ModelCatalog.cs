using System.Text.Json;

namespace Aletheon.ExperimentKit;

public sealed record ModelInfo(string Id, decimal InputUsdPerMillionTokens, decimal OutputUsdPerMillionTokens,
    int MaxCompletionTokens = 16000);

public sealed class ModelCatalog
{
    private readonly Dictionary<string, ModelInfo> models;
    private ModelCatalog(Dictionary<string, ModelInfo> models) => this.models = models;
    public static ModelCatalog Load(string path)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        var map = new Dictionary<string, ModelInfo>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in doc.RootElement.GetProperty("models").EnumerateObject())
        {
            var value = entry.Value;
            map.Add(entry.Name, new ModelInfo(entry.Name,
                value.GetProperty("inputUsdPerMillionTokens").GetDecimal(),
                value.GetProperty("outputUsdPerMillionTokens").GetDecimal(),
                value.TryGetProperty("maxCompletionTokens", out var cap) ? cap.GetInt32() : 16000));
        }
        return new(map);
    }
    public ModelInfo Get(string id) => models.TryGetValue(id, out var info) ? info :
        throw new InvalidOperationException($"Model {id} is not in the pinned catalog.");
    public IReadOnlyCollection<ModelInfo> All => models.Values;
}
