using System.Text.Json;

internal sealed class BudgetManager(TrialConfig config, string output)
{
    private decimal committed;
    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly string ledger = Path.Combine(output, "cost-ledger.jsonl");
    public decimal Remaining => config.MaxBudgetUsd - committed;

    public async Task<decimal?> ReserveAsync(string label, string model, string system, string user, int maxTokens)
    {
        var price = config.ModelPrices[model];
        // Deliberately conservative character-based estimate; NOT a tokenizer or billing guarantee.
        var inputTokens = (long)system.Length + user.Length + 1024L;
        var estimate = Math.Ceiling((inputTokens * price.InputUsdPerMillionTokens +
            maxTokens * price.OutputUsdPerMillionTokens) / 1_000_000m * 100_000m) / 100_000m;
        await gate.WaitAsync();
        try
        {
        if (estimate > config.MaxRequestUsd || estimate + committed > config.MaxBudgetUsd)
        {
            Console.Error.WriteLine($"BUDGET STOP before {label}: reserve ${estimate:F5}, remaining ${Remaining:F5}.");
            await LogAsync(label, model, estimate, null, "blocked-before-request");
            return null;
        }
        committed += estimate;
        await LogAsync(label, model, estimate, null, "reserved-before-request");
        return estimate;
        }
        finally { gate.Release(); }
    }

    public async Task ReconcileAsync(string label, string model, decimal reserved, decimal? actual)
    {
        await gate.WaitAsync();
        try
        {
            if (actual.HasValue) committed = Math.Max(0m, committed - reserved + actual.Value);
            await LogAsync(label, model, reserved, actual, "response-received");
        }
        finally { gate.Release(); }
    }

    public async Task RecordErrorAsync(string label, string model, decimal reserved)
    {
        await gate.WaitAsync();
        try { await LogAsync(label, model, reserved, null, "error-reservation-retained"); }
        finally { gate.Release(); }
    }

    private Task LogAsync(string label, string model, decimal reserved, decimal? reported, string status) =>
        File.AppendAllTextAsync(ledger, JsonSerializer.Serialize(new {
            label, model, reservedUsd = reserved, reportedUsd = reported,
            status, cumulativeUsd = committed
        }) + Environment.NewLine);
}
