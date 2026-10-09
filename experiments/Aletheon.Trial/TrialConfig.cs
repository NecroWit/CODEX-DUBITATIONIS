using System.Text.Json;

internal sealed class TrialConfig
{
    public string Endpoint { get; set; } = "https://openrouter.ai/api/v1/chat/completions";
    public double Temperature { get; set; } = 0.2;
    public int MaxTokens { get; set; } = 4000;
    public decimal MaxBudgetUsd { get; set; } = 1m;
    public decimal MaxRequestUsd { get; set; } = 0.25m;
    public List<AgentConfig> Agents { get; set; } = new();
    public string ArbiterModel { get; set; } = "";
    public List<string> ArbiterModels { get; set; } = new();
    public int Mode { get; set; } = 10;
    public Dictionary<string, ModelPrice> ModelPrices { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public int MaxReviewCharacters { get; set; } = 4500;

    public static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true, WriteIndented = true };

    public static TrialConfig Load(string path, int? mode = null)
    {
        var config = JsonSerializer.Deserialize<TrialConfig>(File.ReadAllText(path), Json)
            ?? throw new InvalidOperationException("Invalid agents.json.");
        config.Mode = mode ?? config.Mode;
        if (config.Mode is not (4 or 10 or 14))
            throw new InvalidOperationException("Mode must be 4, 10, or 14.");
        if (config.Agents.Count != 14 ||
            config.Agents.Select(a => a.Id).Distinct(StringComparer.OrdinalIgnoreCase).Count() != config.Agents.Count ||
            config.Agents.Any(a => string.IsNullOrWhiteSpace(a.Id) || a.Id.Any(c => !char.IsAsciiLetterOrDigit(c) && c is not ('-' or '_')) ||
                string.IsNullOrWhiteSpace(a.Model) || a.Model.Contains("REPLACE_", StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Configure fourteen critics with unique safe IDs and real model IDs.");
        if (config.ArbiterModels.Count != 2 ||
            config.ArbiterModels.Any(m => string.IsNullOrWhiteSpace(m) || m.Contains("REPLACE_", StringComparison.OrdinalIgnoreCase)) ||
            config.ArbiterModels.Distinct(StringComparer.OrdinalIgnoreCase).Count() != 2 ||
            config.Agents.Any(a => config.ArbiterModels.Contains(a.Model, StringComparer.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Configure two distinct arbiters, neither used as a critic.");
        config.Agents = config.Agents.Take(config.Mode).ToList();
        config.ArbiterModels = config.ArbiterModels.Take(config.Mode == 14 ? 2 : 1).ToList();
        config.ArbiterModel = config.ArbiterModels[0];
        if (config.MaxTokens is < 100 or > 16000 || config.Temperature is < 0 or > 2 ||
            config.MaxBudgetUsd is <= 0 or > 10 || config.MaxRequestUsd <= 0 || config.MaxRequestUsd > config.MaxBudgetUsd ||
            config.MaxReviewCharacters is < 1000 or > 20000)
            throw new InvalidOperationException("Invalid generation, review, or budget settings.");
        if (!Uri.TryCreate(config.Endpoint, UriKind.Absolute, out var endpoint) ||
            endpoint.Scheme != Uri.UriSchemeHttps || endpoint.Host != "openrouter.ai" ||
            endpoint.AbsolutePath != "/api/v1/chat/completions" || !string.IsNullOrEmpty(endpoint.Query) ||
            !string.IsNullOrEmpty(endpoint.UserInfo))
            throw new InvalidOperationException("Only the HTTPS OpenRouter chat completions endpoint is supported.");
        foreach (var model in config.Agents.Select(a => a.Model).Concat(config.ArbiterModels))
            if (!config.ModelPrices.TryGetValue(model, out var price) ||
                price.InputUsdPerMillionTokens <= 0 || price.OutputUsdPerMillionTokens <= 0)
                throw new InvalidOperationException($"Missing positive modelPrices entry for {model}.");
        return config;
    }
}
internal sealed class AgentConfig
{
    public string Id { get; set; } = "";
    public string Model { get; set; } = "";
    public string Role { get; set; } = "";
}
internal sealed class ModelPrice
{
    public decimal InputUsdPerMillionTokens { get; set; }
    public decimal OutputUsdPerMillionTokens { get; set; }
}
