using CanadaImmigration.Api.Models;
using CanadaImmigration.Api.Services;
using Xunit;

namespace CanadaImmigration.Api.Tests.Services;

public class CrsScoreServiceTests
{
    private readonly CrsScoreService _sut = new();

    private static LanguageAbilities Abilities(int level) => new()
    {
        Listening = level,
        Reading = level,
        Writing = level,
        Speaking = level
    };

    private static CrsScoreRequest BaseRequest() => new()
    {
        Age = 30,
        Education = EducationLevel.BachelorsOrThreeYear,
        FirstLanguageIsFrench = false,
        FirstLanguage = Abilities(9),
        CanadianWorkYears = 0,
        ForeignWorkYears = 0,
        CanadianEducation = CanadianEducationLevel.None
    };

    [Fact]
    public void Calculate_SingleStrongProfile_ReturnsExpectedTotal()
    {
        var request = BaseRequest();
        request.CanadianWorkYears = 1;
        request.ForeignWorkYears = 3;

        var result = _sut.Calculate(request);

        // age 105 + edu 120 + lang 4x31 + ca 40 + transferability 113 (capped 100)
        Assert.Equal(105, result.Breakdown.Age);
        Assert.Equal(120, result.Breakdown.Education);
        Assert.Equal(124, result.Breakdown.FirstLanguage);
        Assert.Equal(40, result.Breakdown.CanadianWorkExperience);
        Assert.Equal(100, result.Breakdown.SkillTransferability);
        Assert.Equal(489, result.TotalScore);
    }

    [Fact]
    public void Calculate_WithSpouse_UsesSpouseTablesAndAddsSpouseFactors()
    {
        var request = BaseRequest();
        request.CanadianWorkYears = 1;
        request.ForeignWorkYears = 3;
        request.Spouse = new SpouseInfo
        {
            Education = EducationLevel.BachelorsOrThreeYear,
            FirstLanguage = Abilities(9),
            CanadianWorkYears = 1
        };

        var result = _sut.Calculate(request);

        Assert.Equal(95, result.Breakdown.Age);
        Assert.Equal(112, result.Breakdown.Education);
        Assert.Equal(116, result.Breakdown.FirstLanguage); // 4 x 29
        Assert.Equal(35, result.Breakdown.CanadianWorkExperience);
        Assert.Equal(33, result.Breakdown.SpouseFactors); // edu 8 + lang 4x5 + work 5
        Assert.Equal(100, result.Breakdown.SkillTransferability);
        Assert.Equal(491, result.TotalScore);
    }

    [Fact]
    public void Calculate_SecondLanguage_IsCappedAt24WithoutSpouse()
    {
        var request = BaseRequest();
        request.SecondLanguage = Abilities(9); // 4 x 6 = 24

        var result = _sut.Calculate(request);

        Assert.Equal(24, result.Breakdown.SecondLanguage);
    }

    [Fact]
    public void Calculate_SecondLanguage_IsCappedAt22WithSpouse()
    {
        var request = BaseRequest();
        request.SecondLanguage = Abilities(9);
        request.Spouse = new SpouseInfo();

        var result = _sut.Calculate(request);

        Assert.Equal(22, result.Breakdown.SecondLanguage);
    }

    [Fact]
    public void Calculate_SkillTransferability_IsCappedAt100()
    {
        var request = BaseRequest();
        request.Education = EducationLevel.Doctoral;
        request.CanadianWorkYears = 2;
        request.ForeignWorkYears = 3;
        request.HasCertificateOfQualification = true;

        var result = _sut.Calculate(request);

        Assert.Equal(100, result.Breakdown.SkillTransferability);
    }

    [Fact]
    public void Calculate_SkillTransferability_RequiresClb7ForEducationCombo()
    {
        var request = BaseRequest();
        request.FirstLanguage = Abilities(6); // abaixo de CLB 7
        request.CanadianWorkYears = 2;
        request.ForeignWorkYears = 3;

        var result = _sut.Calculate(request);

        // só combos por experiência contam (sem língua CLB7+)
        Assert.Equal(75, result.Breakdown.SkillTransferability); // edu×ca 25 + foreign×ca 50
    }

    [Theory]
    [InlineData(4, 0)]   // sem francês forte
    [InlineData(6, 0)]
    [InlineData(7, 50)]  // NCLC7+ francês + inglês CLB5+
    [InlineData(9, 50)]
    public void Calculate_FrenchBonus_WhenFirstIsEnglishAndSecondIsFrench(int secondClb, int expectedBonus)
    {
        var request = BaseRequest();
        request.SecondLanguage = Abilities(secondClb);

        var result = _sut.Calculate(request);

        Assert.Equal(expectedBonus, result.Breakdown.Additional);
    }

    [Fact]
    public void Calculate_FrenchBonus_WithoutEnglishTest_Gives25()
    {
        var request = BaseRequest();
        request.FirstLanguageIsFrench = true; // francês NCLC9 como primeira língua, sem segunda

        var result = _sut.Calculate(request);

        Assert.Equal(25, result.Breakdown.Additional);
    }

    [Fact]
    public void Calculate_AdditionalPoints_StacksAllBonuses()
    {
        var request = BaseRequest();
        request.HasSiblingInCanada = true;
        request.HasProvincialNomination = true;
        request.CanadianEducation = CanadianEducationLevel.ThreeYearsOrMore;

        var result = _sut.Calculate(request);

        Assert.Equal(645, result.Breakdown.Additional); // 15 + 600 + 30
    }

    [Theory]
    [InlineData(17, 0)]
    [InlineData(18, 99)]
    [InlineData(29, 110)]
    [InlineData(30, 105)]
    [InlineData(44, 6)]
    [InlineData(45, 0)]
    [InlineData(60, 0)]
    public void Calculate_AgePoints_FollowOfficialTable(int age, int expected)
    {
        var request = BaseRequest();
        request.Age = age;
        // zera o resto para isolar a idade
        request.Education = EducationLevel.LessThanSecondary;
        request.FirstLanguage = Abilities(0);

        var result = _sut.Calculate(request);

        Assert.Equal(expected, result.Breakdown.Age);
    }

    [Fact]
    public void Calculate_SpouseFactors_ZeroWhenSpouseHasNoData()
    {
        var request = BaseRequest();
        request.Spouse = new SpouseInfo();

        var result = _sut.Calculate(request);

        Assert.Equal(0, result.Breakdown.SpouseFactors);
        Assert.Equal(95, result.Breakdown.Age); // ainda usa a tabela "com cônjuge"
    }

    [Fact]
    public void Calculate_CanadianEducationCredential_Gives15Or30()
    {
        var request = BaseRequest();
        request.CanadianEducation = CanadianEducationLevel.OneOrTwoYears;
        Assert.Equal(15, _sut.Calculate(request).Breakdown.Additional);

        request.CanadianEducation = CanadianEducationLevel.ThreeYearsOrMore;
        Assert.Equal(30, _sut.Calculate(request).Breakdown.Additional);
    }
}
