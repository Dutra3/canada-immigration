using CanadaImmigration.Api.Services;
using CanadaImmigration.Api.Tests.TestHelpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;
using System.Text.Json;
using Xunit;

namespace CanadaImmigration.Api.Tests.Services;

public class ExpressEntryDrawServiceTests
{
    private static ExpressEntryDrawService CreateService(
        HttpMessageHandler handler,
        IMemoryCache? cache = null,
        IConfiguration? configuration = null)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://fake-api.test/api/")
        };

        return new ExpressEntryDrawService(
            httpClient,
            cache ?? new MemoryCache(new MemoryCacheOptions()),
            configuration ?? new ConfigurationManager(),
            NullLogger<ExpressEntryDrawService>.Instance);
    }

    private static ExpressEntryDrawService CreateService(
        string fakeJsonResponse,
        IMemoryCache? cache = null,
        IConfiguration? configuration = null)
    {
        return CreateService(FakeHttpMessageHandler.ReturningJson(fakeJsonResponse), cache, configuration);
    }

    private static FakeHttpMessageHandler CountingHandler(string json, Action onCall)
    {
        return new FakeHttpMessageHandler(_ =>
        {
            onCall();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
            };
        });
    }

    [Fact]
    public async Task GetDrawsAsync_ParsesEnvelopeCorrectly()
    {
        const string json = """
        {
            "draws": [
                { "drawNumber": 439, "date": "2026-09-01", "invitationsIssued": 2000, "minimumCRS": 521, "category": "CEC", "year": "2026" },
                { "drawNumber": 438, "date": "2026-08-31", "invitationsIssued": 562, "minimumCRS": 697, "category": "PNP", "year": "2026" }
            ]
        }
        """;

        var service = CreateService(json);

        var draws = await service.GetDrawsAsync();

        Assert.Equal(2, draws.Count);
        Assert.Equal(439, draws[0].DrawNumber);
        Assert.Equal("CEC", draws[0].Category);
        Assert.Equal(521, draws[0].MinimumCrs);
    }

    [Fact]
    public async Task GetDrawsAsync_WithNullCategory_DeserializesAsNull()
    {
        const string json = """
        {
            "draws": [
                { "drawNumber": 250, "date": "2023-06-08", "invitationsIssued": 4800, "minimumCRS": 486, "category": null, "year": "2023" }
            ]
        }
        """;

        var service = CreateService(json);

        var draws = await service.GetDrawsAsync();

        Assert.Null(draws[0].Category);
    }

    [Fact]
    public async Task GetLatestDrawAsync_ParsesSingleDrawEnvelope()
    {
        const string json = """
        {
            "draw": { "drawNumber": 439, "date": "2026-09-01", "invitationsIssued": 2000, "minimumCRS": 521, "category": "CEC", "year": "2026" }
        }
        """;

        var service = CreateService(json);

        var latest = await service.GetLatestDrawAsync();

        Assert.NotNull(latest);
        Assert.Equal(439, latest!.DrawNumber);
    }

    [Fact]
    public async Task GetPoolDistributionAsync_ParsesRangesCorrectly()
    {
        const string json = """
        {
            "pool": {
                "drawNumber": 439,
                "drawDate": "2026-09-01",
                "ranges": [
                    { "key": "dd2", "range": "501-600", "value": "19,542" },
                    { "key": "dd18", "range": "Total", "value": "226,673" }
                ]
            }
        }
        """;

        var service = CreateService(json);

        var pool = await service.GetPoolDistributionAsync();

        Assert.NotNull(pool);
        Assert.Equal(439, pool!.DrawNumber);
        Assert.Equal(19542m, pool.Ranges[0].Value);
    }

    [Fact]
    public async Task GetPoolDistributionAsync_ExtractsTotalCandidates_AndRemovesFromRanges()
    {
        const string json = """
        {
            "pool": {
                "drawNumber": 439,
                "drawDate": "2026-09-01",
                "ranges": [
                    { "key": "dd2", "range": "501-600", "value": "19,542" },
                    { "key": "dd3", "range": "601-700", "value": "8,201" },
                    { "key": "dd18", "range": "Total", "value": "226,673" }
                ]
            }
        }
        """;

        var service = CreateService(json);

        var pool = await service.GetPoolDistributionAsync();

        Assert.NotNull(pool);
        Assert.Equal(2, pool!.Ranges.Count);
        Assert.DoesNotContain(pool.Ranges, r => r.Range == "Total");
        Assert.Equal(226673m, pool.TotalCandidates);
    }

    [Fact]
    public async Task GetPoolDistributionAsync_WithoutTotalRow_LeavesTotalCandidatesAsZero()
    {
        const string json = """
        {
            "pool": {
                "drawNumber": 439,
                "drawDate": "2026-09-01",
                "ranges": [
                    { "key": "dd2", "range": "501-600", "value": "19,542" }
                ]
            }
        }
        """;

        var service = CreateService(json);

        var pool = await service.GetPoolDistributionAsync();

        Assert.NotNull(pool);
        Assert.Single(pool!.Ranges);
        Assert.Equal(0m, pool.TotalCandidates);
    }

    [Fact]
    public async Task GetPoolDistributionAsync_WithMalformedValue_ThrowsJsonException()
    {
        const string json = """
        {
            "pool": {
                "drawNumber": 439,
                "drawDate": "2026-09-01",
                "ranges": [
                    { "key": "dd2", "range": "501-600", "value": "not-a-number" }
                ]
            }
        }
        """;

        var service = CreateService(json);

        await Assert.ThrowsAsync<JsonException>(() => service.GetPoolDistributionAsync());
    }

    [Fact]
    public async Task GetPoolDistributionAsync_RemovesAggregateSubtotalRanges()
    {
        const string json = """
        {
            "pool": {
                "drawNumber": 441,
                "drawDate": "2026-09-04",
                "ranges": [
                    { "key": "dd1", "range": "601-1200", "value": "559" },
                    { "key": "dd2", "range": "501-600", "value": "19,542" },
                    { "key": "dd3", "range": "451-500", "value": "74,105" },
                    { "key": "dd4", "range": "491-500", "value": "12,952" },
                    { "key": "dd5", "range": "481-490", "value": "13,273" },
                    { "key": "dd6", "range": "471-480", "value": "16,981" },
                    { "key": "dd7", "range": "461-470", "value": "16,099" },
                    { "key": "dd8", "range": "451-460", "value": "14,800" },
                    { "key": "dd9", "range": "401-450", "value": "60,413" },
                    { "key": "dd10", "range": "441-450", "value": "13,463" },
                    { "key": "dd11", "range": "431-440", "value": "13,233" },
                    { "key": "dd12", "range": "421-430", "value": "11,811" },
                    { "key": "dd13", "range": "411-420", "value": "11,183" },
                    { "key": "dd14", "range": "401-410", "value": "10,723" },
                    { "key": "dd15", "range": "351-400", "value": "46,824" },
                    { "key": "dd16", "range": "301-350", "value": "17,495" },
                    { "key": "dd17", "range": "0-300", "value": "7,735" },
                    { "key": "dd18", "range": "Total", "value": "226,673" }
                ]
            }
        }
        """;

        var service = CreateService(json);
        var pool = await service.GetPoolDistributionAsync();

        Assert.NotNull(pool);
        Assert.DoesNotContain(pool!.Ranges, r => r.Range == "451-500");
        Assert.DoesNotContain(pool.Ranges, r => r.Range == "401-450");

        Assert.Contains(pool.Ranges, r => r.Range == "491-500");
        Assert.Contains(pool.Ranges, r => r.Range == "401-410");

        Assert.Contains(pool.Ranges, r => r.Range == "601-1200");
        Assert.Contains(pool.Ranges, r => r.Range == "301-350");
        Assert.Contains(pool.Ranges, r => r.Range == "0-300");

        Assert.Equal(15, pool.Ranges.Count);
        Assert.Equal(226673m, pool.TotalCandidates);
    }

    [Fact]
    public async Task GetDrawsAsync_SecondCallWithSameFilters_UsesCache()
    {
        const string json = """{ "draws": [] }""";
        var calls = 0;
        var service = CreateService(CountingHandler(json, () => calls++));

        await service.GetDrawsAsync();
        await service.GetDrawsAsync();

        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task GetDrawsAsync_DifferentFilters_CreateSeparateCacheEntries()
    {
        const string json = """{ "draws": [] }""";
        var calls = 0;
        var service = CreateService(CountingHandler(json, () => calls++));

        await service.GetDrawsAsync();
        await service.GetDrawsAsync(year: 2026);
        await service.GetDrawsAsync(category: "CEC");

        Assert.Equal(3, calls);
    }

    [Fact]
    public async Task GetLatestDrawAsync_SecondCall_UsesCache()
    {
        const string json = """
        { "draw": { "drawNumber": 439, "date": "2026-09-01", "invitationsIssued": 2000, "minimumCRS": 521, "category": "CEC", "year": "2026" } }
        """;
        var calls = 0;
        var service = CreateService(CountingHandler(json, () => calls++));

        await service.GetLatestDrawAsync();
        await service.GetLatestDrawAsync();

        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task GetDrawsAsync_WithZeroCacheMinutes_RefetchesEveryCall()
    {
        const string json = """{ "draws": [] }""";
        var configuration = new ConfigurationManager();
        configuration["ExternalApi:CacheMinutes"] = "0";

        var calls = 0;
        var service = CreateService(CountingHandler(json, () => calls++), configuration: configuration);

        await service.GetDrawsAsync();
        await service.GetDrawsAsync();

        Assert.Equal(2, calls);
    }
}