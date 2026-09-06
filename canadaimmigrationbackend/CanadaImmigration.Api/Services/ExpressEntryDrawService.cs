using System.Net.Http.Json;
using System.Linq;
using CanadaImmigration.Api.Models;

namespace CanadaImmigration.Api.Services;

public class ExpressEntryDrawService : IExpressEntryDrawService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExpressEntryDrawService> _logger;

    public ExpressEntryDrawService(HttpClient httpClient, ILogger<ExpressEntryDrawService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ExpressEntryDraw>> GetDrawsAsync(
        int? year = null,
        string? category = null,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (year.HasValue) query.Add($"year={year.Value}");
        if (!string.IsNullOrWhiteSpace(category)) query.Add($"category={Uri.EscapeDataString(category)}");

        // Caminho relativo (SEM barra inicial) porque o BaseAddress já termina em /api/
        var url = "draws" + (query.Count > 0 ? "?" + string.Join("&", query) : string.Empty);

        try
        {
            var envelope = await _httpClient.GetFromJsonAsync<DrawsApiEnvelope>(url, cancellationToken);
            return envelope?.Draws ?? new List<ExpressEntryDraw>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Falha ao buscar draws na can-ee-draws API ({Url})", url);
            throw;
        }
    }

    public async Task<ExpressEntryDraw?> GetLatestDrawAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var envelope = await _httpClient.GetFromJsonAsync<SingleDrawApiEnvelope>("draws/latest", cancellationToken);
            return envelope?.Draw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Falha ao buscar o último draw na can-ee-draws API");
            throw;
        }
    }

    public async Task<PoolDistribution?> GetPoolDistributionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var envelope = await _httpClient.GetFromJsonAsync<PoolApiEnvelope>("pool", cancellationToken);
            var pool = envelope?.Pool;

            if (pool is null)
            {
                return null;
            }

            ExtractTotalFromRanges(pool);
            pool.Ranges = RemoveAggregateRanges(pool.Ranges);

            return pool;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Falha ao buscar distribuição do pool na can-ee-draws API");
            throw;
        }
    }

    private void ExtractTotalFromRanges(PoolDistribution pool)
    {
        var totalRange = pool.Ranges.FirstOrDefault(r =>
            string.Equals(r.Range, "Total", StringComparison.OrdinalIgnoreCase));

        if (totalRange is null)
        {
            _logger.LogWarning("Linha 'Total' não encontrada na resposta do pool — TotalCandidates ficará 0.");
            return;
        }

        pool.TotalCandidates = totalRange.Value;
        pool.Ranges.Remove(totalRange);
    }

    private static List<PoolScoreRange> RemoveAggregateRanges(List<PoolScoreRange> ranges)
    {
        var parsed = ranges
            .Select(r => (Range: r, Bounds: ParseBounds(r.Range)))
            .Where(x => x.Bounds is not null)
            .ToList();

        var toRemove = new HashSet<PoolScoreRange>();

        foreach (var (candidate, bounds) in parsed)
        {
            var (min, max) = bounds!.Value;

            var subRanges = parsed
                .Where(x => x.Range != candidate && x.Bounds!.Value.Item1 >= min && x.Bounds.Value.Item2 <= max)
                .OrderBy(x => x.Bounds!.Value.Item1)
                .ToList();

            if (subRanges.Count < 2)
            {
                continue;
            }

            var coversExactly = subRanges[0].Bounds!.Value.Item1 == min
                && subRanges[^1].Bounds!.Value.Item2 == max;

            for (var i = 1; i < subRanges.Count && coversExactly; i++)
            {
                if (subRanges[i].Bounds!.Value.Item1 != subRanges[i - 1].Bounds!.Value.Item2 + 1)
                {
                    coversExactly = false;
                }
            }

            if (coversExactly)
            {
                toRemove.Add(candidate);
            }
        }

        return ranges.Where(r => !toRemove.Contains(r)).ToList();
    }

    private static (int Min, int Max)? ParseBounds(string range)
    {
        var parts = range.Split('-');

        if (parts.Length != 2
            || !int.TryParse(parts[0], out var min)
            || !int.TryParse(parts[1], out var max))
        {
            return null;
        }

        return (min, max);
    }
}
