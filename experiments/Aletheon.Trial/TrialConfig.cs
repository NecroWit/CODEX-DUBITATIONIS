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
    public List<string> StandbyModels { get; set; } = new();
    public List<string> RotationModels { get; set; } = new();
    public Dictionary<string, List<string>> MixedTiers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public int Mode { get; set; } = 10;
    public Dictionary<string, ModelPrice> ModelPrices { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public int MaxReviewCharacters { get; set; } = 4500;

    public static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true, WriteIndented = true };

    public static string ModelFamily(string model) => model.Split('/')[0].ToLowerInvariant();

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
        if (config.MixedTiers.Count > 0)
        {
            var tiers = new[] { "cheap", "medium", "premium" };
            if (config.MixedTiers.Count != 3 || tiers.Any(t => !config.MixedTiers.ContainsKey(t) ||
                config.MixedTiers[t].Count == 0))
                throw new InvalidOperationException("Mixed tiers must contain nonempty cheap, medium and premium pools.");
            var all = tiers.SelectMany(t => config.MixedTiers[t]).ToList();
            if (all.Distinct(StringComparer.OrdinalIgnoreCase).Count() != all.Count ||
                all.Any(m => string.IsNullOrWhiteSpace(m) || config.ArbiterModels.Contains(m, StringComparer.OrdinalIgnoreCase) ||
                    !config.ModelPrices.TryGetValue(m, out var price) ||
                    price.InputUsdPerMillionTokens <= 0 || price.OutputUsdPerMillionTokens <= 0))
                throw new InvalidOperationException("Mixed tier models must be distinct, priced and exclude arbiters.");
            var counts = tiers.ToDictionary(t => t, _ => config.Mode / 3);
            var extras = config.Mode % 3;
            var shuffledTiers = tiers.ToList();
            for (var i = shuffledTiers.Count - 1; i > 0; i--)
            {
                var j = System.Security.Cryptography.RandomNumberGenerator.GetInt32(i + 1);
                (shuffledTiers[i], shuffledTiers[j]) = (shuffledTiers[j], shuffledTiers[i]);
            }
            foreach (var tier in shuffledTiers.Take(extras)) counts[tier]++;
            // Choose the most provider-diverse roster across all tiers, not separately
            // within each tier. Enumerate distinct combinations; break ties randomly.
            var selected = new List<string>();
            List<string>? best = null;
            var bestFamilies = -1;
            var ties = 0;
            void Search(int tierIndex)
            {
                if (tierIndex == tiers.Length)
                {
                    var diversity = selected.Select(ModelFamily).Distinct(StringComparer.OrdinalIgnoreCase).Count();
                    if (diversity > bestFamilies)
                    {
                        bestFamilies = diversity;
                        best = selected.ToList();
                        ties = 1;
                    }
                    else if (diversity == bestFamilies &&
                             System.Security.Cryptography.RandomNumberGenerator.GetInt32(++ties) == 0)
                        best = selected.ToList();
                    return;
                }
                var pool = config.MixedTiers[tiers[tierIndex]];
                void Choose(int next, int remaining)
                {
                    if (remaining == 0) { Search(tierIndex + 1); return; }
                    for (var i = next; i <= pool.Count - remaining; i++)
                    {
                        selected.Add(pool[i]);
                        Choose(i + 1, remaining - 1);
                        selected.RemoveAt(selected.Count - 1);
                    }
                }
                Choose(0, counts[tiers[tierIndex]]);
            }
            foreach (var tier in tiers)
                if (config.MixedTiers[tier].Count < counts[tier])
                    throw new InvalidOperationException($"Mixed tier {tier} lacks {counts[tier]} candidates.");
            Search(0);
            selected = best ?? throw new InvalidOperationException("No valid mixed roster.");
            if (bestFamilies < config.Mode)
                Console.WriteLine($"MIXED FAMILY DIVERSITY: {bestFamilies}/{config.Mode} distinct providers; unique providers unavailable for this tier split.");
            for (var i = selected.Count - 1; i > 0; i--)
            {
                var j = System.Security.Cryptography.RandomNumberGenerator.GetInt32(i + 1);
                (selected[i], selected[j]) = (selected[j], selected[i]);
            }
            for (var i = 0; i < config.Mode; i++) config.Agents[i].Model = selected[i];
            config.RotationModels = all;
        }
        else if (config.RotationModels.Count > 0)
        {
            if (config.RotationModels.Any(string.IsNullOrWhiteSpace) ||
                config.RotationModels.Distinct(StringComparer.OrdinalIgnoreCase).Count() != config.RotationModels.Count ||
                config.RotationModels.Count < config.Mode ||
                config.RotationModels.Any(m => config.ArbiterModels.Contains(m, StringComparer.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Rotation pool must contain at least Mode distinct models, excluding arbiters.");
            foreach (var model in config.RotationModels)
                if (!config.ModelPrices.TryGetValue(model, out var price) ||
                    price.InputUsdPerMillionTokens <= 0 || price.OutputUsdPerMillionTokens <= 0)
                    throw new InvalidOperationException($"Missing positive modelPrices entry for rotation model {model}.");
            // Fisher-Yates: unbiased random assignment, with no replacement within a run.
            var pool = config.RotationModels.ToList();
            for (var i = pool.Count - 1; i > 0; i--)
            {
                var j = System.Security.Cryptography.RandomNumberGenerator.GetInt32(i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }
            // Prefer different model providers/families (e.g. avoid two Grok versions).
            // If the pool has too few families, fill remaining slots with distinct models.
            var chosen = new List<string>();
            var families = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var candidate in pool)
                if (families.Add(ModelFamily(candidate))) chosen.Add(candidate);
            foreach (var candidate in pool)
                if (chosen.Count < config.Mode && !chosen.Contains(candidate, StringComparer.OrdinalIgnoreCase))
                    chosen.Add(candidate);
            for (var i = 0; i < config.Mode; i++)
                config.Agents[i].Model = chosen[i];
        }
        config.Agents = config.Agents.Take(config.Mode).ToList();
        config.ArbiterModels = config.ArbiterModels.Take(config.Mode >= 10 ? 2 : 1).ToList();
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
        if (config.StandbyModels.Count != 0 &&
            (config.StandbyModels.Count != 5 ||
             config.StandbyModels.Any(string.IsNullOrWhiteSpace) ||
             config.StandbyModels.Distinct(StringComparer.OrdinalIgnoreCase).Count() != 5 ||
             config.StandbyModels.Any(m => config.Agents.Any(a => string.Equals(a.Model, m, StringComparison.OrdinalIgnoreCase)) ||
                                           config.ArbiterModels.Contains(m, StringComparer.OrdinalIgnoreCase))))
            throw new InvalidOperationException("Configure exactly five distinct standby models, different from active critics and arbiters.");
        foreach (var model in config.Agents.Select(a => a.Model).Concat(config.ArbiterModels).Concat(config.StandbyModels))
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
