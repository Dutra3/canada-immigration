using CanadaImmigration.Api.Models;
using CanadaImmigration.Api.Services;

namespace CanadaImmigration.Api.Tests.TestHelpers;

public class FakeExpressEntryDrawService : IExpressEntryDrawService
{
    public List<ExpressEntryDraw> DrawsToReturn { get; set; } = new();
    public ExpressEntryDraw? LatestToReturn { get; set; }
    public PoolDistribution? PoolToReturn { get; set; }

    public Task<IReadOnlyList<ExpressEntryDraw>> GetDrawsAsync(
        int? year = null,
        string? category = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult((IReadOnlyList<ExpressEntryDraw>)DrawsToReturn);
    }

    public Task<ExpressEntryDraw?> GetLatestDrawAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(LatestToReturn);
    }

    public Task<PoolDistribution?> GetPoolDistributionAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(PoolToReturn);
    }
}
