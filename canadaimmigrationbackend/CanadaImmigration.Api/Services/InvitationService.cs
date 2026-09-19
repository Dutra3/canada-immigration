using CanadaImmigration.Api.Models;

namespace CanadaImmigration.Api.Services;

// Compara a nota CRS do usuário com o corte do draw mais recente de cada
// categoria e com a distribuição atual do pool. Categorias baseadas em
// ocupação (NOC) ficam como Unknown — não dá pra inferir elegibilidade.
public class InvitationService : IInvitationService
{
    private readonly IExpressEntryDrawService _drawService;

    public InvitationService(IExpressEntryDrawService drawService)
    {
        _drawService = drawService;
    }

    public async Task<InvitationAnalysis> AnalyzeAsync(
        int score,
        int canadianWorkYears,
        bool hasFrenchProficiency,
        bool hasProvincialNomination,
        CancellationToken cancellationToken = default)
    {
        var draws = await _drawService.GetDrawsAsync(cancellationToken: cancellationToken);
        var pool = await _drawService.GetPoolDistributionAsync(cancellationToken);

        var categories = draws
            .GroupBy(d => d.Category ?? string.Empty)
            .Select(g => g.OrderByDescending(d => d.Date).ThenByDescending(d => d.DrawNumber).First())
            .Select(latest => new CategoryInvitation
            {
                Category = latest.Category ?? string.Empty,
                LatestDrawNumber = latest.DrawNumber,
                LatestDrawDate = latest.Date,
                LatestCutoff = latest.MinimumCrs,
                WouldBeInvited = score >= latest.MinimumCrs,
                Eligibility = EligibilityFor(latest.Category, canadianWorkYears, hasFrenchProficiency, hasProvincialNomination)
            })
            .OrderByDescending(c => c.LatestDrawDate)
            .ToList();

        return new InvitationAnalysis
        {
            Score = score,
            Categories = categories,
            Pool = pool is null ? null : PoolPositionFor(pool, score)
        };
    }

    private static CategoryEligibility EligibilityFor(
        string? category,
        int canadianWorkYears,
        bool hasFrenchProficiency,
        bool hasProvincialNomination)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return CategoryEligibility.Eligible;
        }

        var c = category.ToLowerInvariant();

        if (c == "pnp" || c.Contains("provincial"))
        {
            return hasProvincialNomination ? CategoryEligibility.Eligible : CategoryEligibility.NotEligible;
        }

        if (c.Contains("french"))
        {
            return hasFrenchProficiency ? CategoryEligibility.Eligible : CategoryEligibility.NotEligible;
        }

        if (c == "cec" || c.Contains("experience class"))
        {
            return canadianWorkYears >= 1 ? CategoryEligibility.Eligible : CategoryEligibility.NotEligible;
        }

        return CategoryEligibility.Unknown;
    }

    private static PoolPosition? PoolPositionFor(PoolDistribution pool, int score)
    {
        var parsed = pool.Ranges
            .Select(r => (Range: r, Bounds: ParseBounds(r.Range)))
            .Where(x => x.Bounds is not null)
            .ToList();

        var own = parsed.FirstOrDefault(x => x.Bounds!.Value.Min <= score && score <= x.Bounds!.Value.Max);
        if (own.Range is null)
        {
            return null;
        }

        var below = parsed
            .Where(x => x.Bounds!.Value.Max < own.Bounds!.Value.Min)
            .Sum(x => (int)x.Range.Value);

        var total = pool.TotalCandidates > 0 ? (int)pool.TotalCandidates : parsed.Sum(x => (int)x.Range.Value);

        return new PoolPosition
        {
            Range = own.Range.Range,
            CandidatesInRange = (int)own.Range.Value,
            CandidatesBelow = below,
            TotalCandidates = total,
            PercentBelow = total > 0 ? Math.Round(100.0 * below / total, 1) : 0
        };
    }

    private static (int Min, int Max)? ParseBounds(string range)
    {
        var parts = range.Split('-');
        return parts.Length == 2
            && int.TryParse(parts[0], out var min)
            && int.TryParse(parts[1], out var max)
            ? (min, max)
            : null;
    }
}
