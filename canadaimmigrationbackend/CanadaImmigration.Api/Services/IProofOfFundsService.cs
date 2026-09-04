using CanadaImmigration.Api.Models;

namespace CanadaImmigration.Api.Services;

public interface IProofOfFundsService
{
    ProofOfFundsResult Calculate(int familySize);
}
