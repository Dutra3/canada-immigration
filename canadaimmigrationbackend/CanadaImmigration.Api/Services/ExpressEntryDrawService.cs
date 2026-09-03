using System.Net.Http.Json;
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
            return envelope?.Pool;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Falha ao buscar distribuição do pool na can-ee-draws API");
            throw;
        }
    }
}
