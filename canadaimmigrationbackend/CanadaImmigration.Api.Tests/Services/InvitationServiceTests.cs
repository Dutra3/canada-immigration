using CanadaImmigration.Api.Models;
using CanadaImmigration.Api.Services;
using CanadaImmigration.Api.Tests.TestHelpers;
using Xunit;

namespace CanadaImmigration.Api.Tests.Services;

public class InvitationServiceTests
{
    private readonly FakeExpressEntryDrawService _fakeDrawService = new();
    private readonly InvitationService _sut;

    public InvitationServiceTests()
    {
        _sut = new InvitationService(_fakeDrawService);
        _fakeDrawService.DrawsToReturn = new List<ExpressEntryDraw>
        {
            new() { DrawNumber = 10, Date = new DateTime(2025, 6, 1), Category = "", MinimumCrs = 490 },
            new() { DrawNumber = 11, Date = new DateTime(2025, 7, 1), Category = "", MinimumCrs = 500 },
            new() { DrawNumber = 20, Date = new DateTime(2025, 5, 1), Category = "CEC", MinimumCrs = 530 },
            new() { DrawNumber = 21, Date = new DateTime(2025, 7, 15), Category = "CEC", MinimumCrs = 521 },
            new() { DrawNumber = 30, Date = new DateTime(2025, 6, 20), Category = "French", MinimumCrs = 430 },
            new() { DrawNumber = 40, Date = new DateTime(2025, 6, 25), Category = "PNP", MinimumCrs = 720 },
            new() { DrawNumber = 50, Date = new DateTime(2025, 4, 1), Category = "STEM", MinimumCrs = 480 },
        };
        _fakeDrawService.PoolToReturn = new PoolDistribution
        {
            DrawNumber = 11,
            DrawDate = new DateTime(2025, 7, 1),
            TotalCandidates = 2000,
            Ranges = new List<PoolScoreRange>
            {
                new() { Range = "0-300", Value = 100 },
                new() { Range = "301-350", Value = 200 },
                new() { Range = "351-400", Value = 300 },
                new() { Range = "401-450", Value = 400 },
                new() { Range = "451-500", Value = 500 },
                new() { Range = "501-600", Value = 400 },
                new() { Range = "601-1200", Value = 100 },
            }
        };
    }

    [Fact]
    public async Task Analyze_UsesLatestDrawPerCategory()
    {
        var result = await _sut.AnalyzeAsync(489, 0, false, false);

        var general = result.Categories.Single(c => c.Category == "");
        Assert.Equal(11, general.LatestDrawNumber);
        Assert.Equal(500, general.LatestCutoff);

        var cec = result.Categories.Single(c => c.Category == "CEC");
        Assert.Equal(21, cec.LatestDrawNumber);
        Assert.Equal(521, cec.LatestCutoff);
    }

    [Fact]
    public async Task Analyze_WouldBeInvited_ComparesScoreToCutoff()
    {
        var result = await _sut.AnalyzeAsync(489, 0, false, false);

        Assert.False(result.Categories.Single(c => c.Category == "").WouldBeInvited);   // 489 < 500
        Assert.False(result.Categories.Single(c => c.Category == "CEC").WouldBeInvited); // 489 < 521
        Assert.True(result.Categories.Single(c => c.Category == "French").WouldBeInvited); // 489 >= 430
        Assert.False(result.Categories.Single(c => c.Category == "PNP").WouldBeInvited);
        Assert.True(result.Categories.Single(c => c.Category == "STEM").WouldBeInvited); // 489 >= 480
    }

    [Fact]
    public async Task Analyze_GeneralCategory_IsAlwaysEligible()
    {
        var result = await _sut.AnalyzeAsync(100, 0, false, false);

        Assert.Equal(CategoryEligibility.Eligible, result.Categories.Single(c => c.Category == "").Eligibility);
    }

    [Theory]
    [InlineData(false, CategoryEligibility.NotEligible)]
    [InlineData(true, CategoryEligibility.Eligible)]
    public async Task Analyze_PnpEligibility_DependsOnNomination(bool hasNomination, CategoryEligibility expected)
    {
        var result = await _sut.AnalyzeAsync(489, 0, false, hasNomination);

        Assert.Equal(expected, result.Categories.Single(c => c.Category == "PNP").Eligibility);
    }

    [Theory]
    [InlineData(false, CategoryEligibility.NotEligible)]
    [InlineData(true, CategoryEligibility.Eligible)]
    public async Task Analyze_FrenchEligibility_DependsOnProficiency(bool hasFrench, CategoryEligibility expected)
    {
        var result = await _sut.AnalyzeAsync(489, 0, hasFrench, false);

        Assert.Equal(expected, result.Categories.Single(c => c.Category == "French").Eligibility);
    }

    [Theory]
    [InlineData(0, CategoryEligibility.NotEligible)]
    [InlineData(1, CategoryEligibility.Eligible)]
    [InlineData(3, CategoryEligibility.Eligible)]
    public async Task Analyze_CecEligibility_RequiresCanadianExperience(int canadianYears, CategoryEligibility expected)
    {
        var result = await _sut.AnalyzeAsync(489, canadianYears, false, false);

        Assert.Equal(expected, result.Categories.Single(c => c.Category == "CEC").Eligibility);
    }

    [Fact]
    public async Task Analyze_OccupationCategory_IsUnknown()
    {
        var result = await _sut.AnalyzeAsync(489, 0, false, false);

        Assert.Equal(CategoryEligibility.Unknown, result.Categories.Single(c => c.Category == "STEM").Eligibility);
    }

    [Fact]
    public async Task Analyze_PoolPosition_FindsRangeAndComputesPercentBelow()
    {
        var result = await _sut.AnalyzeAsync(489, 0, false, false);

        Assert.NotNull(result.Pool);
        var pool = result.Pool!;
        Assert.Equal("451-500", pool.Range);
        Assert.Equal(500, pool.CandidatesInRange);
        Assert.Equal(1000, pool.CandidatesBelow); // 100+200+300+400
        Assert.Equal(2000, pool.TotalCandidates);
        Assert.Equal(50.0, pool.PercentBelow);
    }

    [Fact]
    public async Task Analyze_WhenPoolUnavailable_ReturnsNullPoolButKeepsCategories()
    {
        _fakeDrawService.PoolToReturn = null;

        var result = await _sut.AnalyzeAsync(489, 0, false, false);

        Assert.Null(result.Pool);
        Assert.NotEmpty(result.Categories);
    }

    [Fact]
    public async Task Analyze_WhenScoreInLowestRange_HasZeroBelow()
    {
        var result = await _sut.AnalyzeAsync(150, 0, false, false);

        Assert.NotNull(result.Pool);
        Assert.Equal("0-300", result.Pool!.Range);
        Assert.Equal(0, result.Pool.CandidatesBelow);
    }

    [Fact]
    public async Task Analyze_WhenScoreFallsInRangeGap_ReturnsNullPool()
    {
        _fakeDrawService.PoolToReturn!.Ranges = new List<PoolScoreRange>
        {
            new() { Range = "0-300", Value = 100 },
            new() { Range = "500-600", Value = 50 },
        };
        _fakeDrawService.PoolToReturn.TotalCandidates = 150;

        var result = await _sut.AnalyzeAsync(400, 0, false, false); // não cai em nenhuma faixa

        Assert.Null(result.Pool);
    }
}
