using CanadaImmigration.Api.Models;

namespace CanadaImmigration.Api.Services;

// Regra baseada na tabela oficial do IRCC (50% do LICO), atualizada anualmente.
// Fonte: canada.ca/en/immigration-refugees-citizenship/services/immigrate-canada/express-entry/documents/proof-funds.html
// IMPORTANTE: o IRCC republica esses valores todo ano (geralmente em janeiro).
// Quando isso acontecer, é só atualizar a tabela abaixo — nenhuma outra
// parte do sistema precisa mudar.
public class ProofOfFundsService : IProofOfFundsService
{

    private static readonly Dictionary<int, decimal> BaseTable = new()
    {
        { 1, 15263m },
        { 2, 19001m },
        { 3, 23360m },
        { 4, 28362m },
        { 5, 32168m },
        { 6, 36280m },
        { 7, 40392m },
    };

    private const decimal AdditionalMemberAmount = 4112m;

    private const int MaxTableSize = 7;

    public ProofOfFundsResult Calculate(int familySize)
    {
        if (familySize < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(familySize),
                "O número de membros da família deve ser no mínimo 1.");
        }

        decimal requiredFunds;

        if (familySize <= MaxTableSize)
        {
            requiredFunds = BaseTable[familySize];
        }
        else
        {
            var extraMembers = familySize - MaxTableSize;
            requiredFunds = BaseTable[MaxTableSize] + (extraMembers * AdditionalMemberAmount);
        }

        return new ProofOfFundsResult
        {
            FamilySize = familySize,
            RequiredFundsCad = requiredFunds
        };
    }
}
