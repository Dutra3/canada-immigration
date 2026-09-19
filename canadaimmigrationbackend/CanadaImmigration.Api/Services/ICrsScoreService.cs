using CanadaImmigration.Api.Models;

namespace CanadaImmigration.Api.Services;

public interface ICrsScoreService
{
    CrsScoreResult Calculate(CrsScoreRequest request);
}
