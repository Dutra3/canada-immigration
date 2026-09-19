using CanadaImmigration.Api.Models;

namespace CanadaImmigration.Api.Services;

// Tabelas oficiais do IRCC — "Comprehensive Ranking System (CRS) criteria" (canada.ca).
// Pontos de job offer foram removidos pelo IRCC em 25/03/2025 e não entram no cálculo.
public class CrsScoreService : ICrsScoreService
{
    private static readonly int[] EducationWithSpouse = { 0, 28, 84, 91, 112, 119, 126, 140 };
    private static readonly int[] EducationWithoutSpouse = { 0, 30, 90, 98, 120, 128, 135, 150 };

    private static readonly int[] CanadianWorkWithSpouse = { 0, 35, 46, 56, 63, 70 };
    private static readonly int[] CanadianWorkWithoutSpouse = { 0, 40, 53, 64, 72, 80 };

    private static readonly int[] SpouseEducation = { 0, 2, 6, 7, 8, 9, 10, 10 };
    private static readonly int[] SpouseCanadianWork = { 0, 5, 7, 8, 9, 10 };

    public CrsScoreResult Calculate(CrsScoreRequest request)
    {
        var withSpouse = request.Spouse is not null;
        var firstLanguage = request.FirstLanguage.All;

        var breakdown = new CrsBreakdown
        {
            Age = AgePoints(request.Age, withSpouse),
            Education = (withSpouse ? EducationWithSpouse : EducationWithoutSpouse)[(int)request.Education],
            FirstLanguage = firstLanguage.Sum(clb => FirstLanguageAbilityPoints(clb, withSpouse)),
            SecondLanguage = SecondLanguagePoints(request.SecondLanguage, withSpouse),
            CanadianWorkExperience = (withSpouse ? CanadianWorkWithSpouse : CanadianWorkWithoutSpouse)[Math.Clamp(request.CanadianWorkYears, 0, 5)],
            SpouseFactors = withSpouse ? SpousePoints(request.Spouse!) : 0,
            SkillTransferability = SkillTransferabilityPoints(request, firstLanguage),
            Additional = AdditionalPoints(request)
        };

        return new CrsScoreResult
        {
            TotalScore = breakdown.Age + breakdown.Education + breakdown.FirstLanguage + breakdown.SecondLanguage
                + breakdown.CanadianWorkExperience + breakdown.SpouseFactors + breakdown.SkillTransferability + breakdown.Additional,
            Breakdown = breakdown
        };
    }

    private static int AgePoints(int age, bool withSpouse) => age switch
    {
        <= 17 => 0,
        18 => withSpouse ? 90 : 99,
        19 => withSpouse ? 95 : 105,
        >= 20 and <= 29 => withSpouse ? 100 : 110,
        30 => withSpouse ? 95 : 105,
        31 => withSpouse ? 90 : 99,
        32 => withSpouse ? 85 : 94,
        33 => withSpouse ? 80 : 88,
        34 => withSpouse ? 75 : 83,
        35 => withSpouse ? 70 : 77,
        36 => withSpouse ? 65 : 72,
        37 => withSpouse ? 60 : 66,
        38 => withSpouse ? 55 : 61,
        39 => withSpouse ? 50 : 55,
        40 => withSpouse ? 45 : 50,
        41 => withSpouse ? 35 : 39,
        42 => withSpouse ? 25 : 28,
        43 => withSpouse ? 15 : 17,
        44 => withSpouse ? 5 : 6,
        _ => 0
    };

    private static int FirstLanguageAbilityPoints(int clb, bool withSpouse) => clb switch
    {
        < 4 => 0,
        <= 5 => 6,
        6 => withSpouse ? 8 : 9,
        7 => withSpouse ? 16 : 17,
        8 => withSpouse ? 22 : 23,
        9 => withSpouse ? 29 : 31,
        _ => withSpouse ? 32 : 34
    };

    private static int SecondLanguageAbilityPoints(int clb) => clb switch
    {
        <= 4 => 0,
        <= 6 => 1,
        <= 8 => 3,
        _ => 6
    };

    private static int SecondLanguagePoints(LanguageAbilities? language, bool withSpouse)
    {
        if (language is null)
        {
            return 0;
        }

        var points = language.All.Sum(SecondLanguageAbilityPoints);
        return Math.Min(points, withSpouse ? 22 : 24);
    }

    private static int SpousePoints(SpouseInfo spouse)
    {
        var education = spouse.Education.HasValue ? SpouseEducation[(int)spouse.Education.Value] : 0;
        var language = spouse.FirstLanguage?.All.Sum(SpouseLanguageAbilityPoints) ?? 0;
        var work = spouse.CanadianWorkYears.HasValue ? SpouseCanadianWork[Math.Clamp(spouse.CanadianWorkYears.Value, 0, 5)] : 0;

        return education + language + work;
    }

    private static int SpouseLanguageAbilityPoints(int clb) => clb switch
    {
        <= 4 => 0,
        <= 6 => 1,
        <= 8 => 3,
        _ => 5
    };

    private static int SkillTransferabilityPoints(CrsScoreRequest request, int[] firstLanguage)
    {
        var allSevenOrMore = firstLanguage.All(clb => clb >= 7);
        var allNineOrMore = firstLanguage.All(clb => clb >= 9);
        var allFiveOrMore = firstLanguage.All(clb => clb >= 5);

        var educationTier = request.Education switch
        {
            EducationLevel.LessThanSecondary or EducationLevel.Secondary => 0,
            EducationLevel.OneYearPostSecondary or EducationLevel.TwoYearPostSecondary
                or EducationLevel.BachelorsOrThreeYear => 1,
            _ => 2
        };

        var foreignTier = request.ForeignWorkYears >= 3 ? 2 : request.ForeignWorkYears >= 1 ? 1 : 0;
        var canadianTier = request.CanadianWorkYears >= 2 ? 2 : request.CanadianWorkYears >= 1 ? 1 : 0;

        var points = 0;

        if (allSevenOrMore && educationTier > 0)
        {
            points += (educationTier, allNineOrMore) switch
            {
                (1, false) => 13,
                (1, true) => 25,
                (2, false) => 25,
                _ => 50
            };
        }

        if (canadianTier > 0 && educationTier > 0)
        {
            points += (educationTier, canadianTier) switch
            {
                (1, 1) => 13,
                (1, _) => 25,
                (2, 1) => 25,
                _ => 50
            };
        }

        if (allSevenOrMore && foreignTier > 0)
        {
            points += (foreignTier, allNineOrMore) switch
            {
                (1, false) => 13,
                (1, true) => 25,
                (2, false) => 25,
                _ => 50
            };
        }

        if (canadianTier > 0 && foreignTier > 0)
        {
            points += (foreignTier, canadianTier) switch
            {
                (1, 1) => 13,
                (1, _) => 25,
                (2, 1) => 25,
                _ => 50
            };
        }

        if (request.HasCertificateOfQualification && allFiveOrMore)
        {
            points += allSevenOrMore ? 50 : 25;
        }

        return Math.Min(points, 100);
    }

    private static int AdditionalPoints(CrsScoreRequest request)
    {
        var points = 0;

        if (request.HasSiblingInCanada)
        {
            points += 15;
        }

        var french = request.FirstLanguageIsFrench ? request.FirstLanguage : request.SecondLanguage;
        var english = request.FirstLanguageIsFrench ? request.SecondLanguage : (LanguageAbilities?)request.FirstLanguage;

        if (french is not null && french.All.All(clb => clb >= 7))
        {
            points += english is not null && english.All.All(clb => clb >= 5) ? 50 : 25;
        }

        points += request.CanadianEducation switch
        {
            CanadianEducationLevel.OneOrTwoYears => 15,
            CanadianEducationLevel.ThreeYearsOrMore => 30,
            _ => 0
        };

        if (request.HasProvincialNomination)
        {
            points += 600;
        }

        return points;
    }
}
