using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CanadaImmigration.Api.Services;

public class ExternalApiHealthCheck : IHealthCheck
{
    private static readonly TimeSpan ProbeTimeout = TimeSpan.FromSeconds(5);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _baseUrl;

    public ExternalApiHealthCheck(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _baseUrl = configuration["ExternalApi:BaseUrl"]
            ?? throw new InvalidOperationException("ExternalApi:BaseUrl não configurado.");
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(ProbeTimeout);

            var client = _httpClientFactory.CreateClient();
            using var response = await client.GetAsync(_baseUrl, cts.Token);

            return HealthCheckResult.Healthy($"API externa respondeu com {(int)response.StatusCode}.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("API externa inacessível.", ex);
        }
    }
}
