using CanadaImmigration.Api.Models;

namespace CanadaImmigration.Api.Services;

public interface IInvitationService
{
    Task<InvitationAnalysis> AnalyzeAsync(
        int score,
        int canadianWorkYears,
        bool hasFrenchProficiency,
        bool hasProvincialNomination,
        CancellationToken cancellationToken = default);
}
