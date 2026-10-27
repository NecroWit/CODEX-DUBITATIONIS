namespace Aletheon.ExperimentKit;

/// <summary>EXP-3 only: size per-request reservation for the full-context arm.
/// Does not change the overall experiment spending limit.</summary>
public static class Exp3BudgetPolicy
{
    public static decimal RequiredRequestCap(ModelInfo model, string system,
        string context, IEnumerable<string> prompts, int maxTokens, decimal configuredCap)
    {
        decimal Estimate(string input)
        {
            // Match BudgetLedger.Reserve's conservative estimation exactly.
            var inputTokens = Math.Ceiling(input.Length / 2m) + 1024m;
            var usd = (inputTokens * model.InputUsdPerMillionTokens +
                maxTokens * model.OutputUsdPerMillionTokens) / 1_000_000m;
            return Math.Ceiling(usd * 1.20m * 100000m) / 100000m;
        }
        var largest = prompts.Select(p => Math.Max(Estimate(system + p),
            Estimate(system + "\n" + context + p))).DefaultIfEmpty(0m).Max();
        return Math.Max(configuredCap, largest);
    }
}
