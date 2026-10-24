using System.Text.Json;

namespace Aletheon.ExperimentKit;

public sealed class BudgetLedger
{
    private readonly object gate = new();
    private readonly string file;
    private readonly decimal total, perRequest;
    private decimal committed;
    public BudgetLedger(string file, decimal totalUsd, decimal perRequestUsd)
    {
        if (totalUsd <= 0 || perRequestUsd <= 0 || perRequestUsd > totalUsd)
            throw new ArgumentException("Invalid budget");
        this.file = file; total = totalUsd; perRequest = perRequestUsd;
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);
    }
    public decimal? Reserve(string label, ModelInfo model, string system, string prompt, int tokens)
    {
        // Conservative character estimate from legacy C# BudgetManager; not an actual tokenizer.
        var inputEstimate = system.Length + prompt.Length + 1024m;
        var estimate = Math.Ceiling((inputEstimate * model.InputUsdPerMillionTokens +
            tokens * model.OutputUsdPerMillionTokens) / 1_000_000m * 100000m) / 100000m;
        lock (gate)
        {
            if (estimate > perRequest || committed + estimate > total)
            {
                Log(new { label, model = model.Id, estimate, status = "blocked" });
                return null;
            }
            committed += estimate;
            Log(new { label, model = model.Id, estimate, committed, status = "reserved" });
            return estimate;
        }
    }
    public void Reconcile(string label, ModelInfo model, decimal reserved, decimal? actual)
    {
        lock (gate)
        {
            if (actual is >= 0) committed = Math.Max(0, committed - reserved + actual.Value);
            Log(new { label, model = model.Id, reserved, actual, committed,
                status = actual.HasValue ? "reconciled" : "reservation_retained" });
        }
    }
    private void Log(object data) => File.AppendAllText(file, JsonSerializer.Serialize(data) + Environment.NewLine);
}
