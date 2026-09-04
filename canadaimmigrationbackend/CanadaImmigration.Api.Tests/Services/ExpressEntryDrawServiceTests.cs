using CanadaImmigration.Api.Services;
using CanadaImmigration.Api.Tests.TestHelpers;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CanadaImmigration.Api.Tests.Services;

public class ExpressEntryDrawServiceTests
{
    private static ExpressEntryDrawService CreateService(string fakeJsonResponse)
    {
        var handler = FakeHttpMessageHandler.ReturningJson(fakeJsonResponse);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://fake-api.test/api/")
        };

        return new ExpressEntryDrawService(httpClient, NullLogger<ExpressEntryDrawService>.Instance);
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
        Assert.Equal(2, pool.Ranges.Count);
        Assert.Equal("19,542", pool.Ranges[0].Value);
    }
}