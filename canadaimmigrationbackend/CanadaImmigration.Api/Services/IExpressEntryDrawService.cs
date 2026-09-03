using CanadaImmigration.Api.Models;

namespace CanadaImmigration.Api.Services;

public interface IExpressEntryDrawService
{
    Task<IReadOnlyList<ExpressEntryDraw>> GetDrawsAsync(
        int? year = null,
        string? category = null,
        CancellationToken cancellationToken = default);

    Task<ExpressEntryDraw?> GetLatestDrawAsync(CancellationToken cancellationToken = default);

    Task<PoolDistribution?> GetPoolDistributionAsync(CancellationToken cancellationToken = default);
}
