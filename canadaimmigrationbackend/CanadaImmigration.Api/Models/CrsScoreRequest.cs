using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CanadaImmigration.Api.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EducationLevel
{
    LessThanSecondary,
    Secondary,
    OneYearPostSecondary,
    TwoYearPostSecondary,
    BachelorsOrThreeYear,
    TwoOrMoreCredentials,
    MastersOrProfessional,
    Doctoral
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CanadianEducationLevel
{
    None,
    OneOrTwoYears,
    ThreeYearsOrMore
}

public class LanguageAbilities
{
    [Range(0, 12)]
    public int Listening { get; set; }

    [Range(0, 12)]
    public int Reading { get; set; }

    [Range(0, 12)]
    public int Writing { get; set; }

    [Range(0, 12)]
    public int Speaking { get; set; }

    public int[] All => new[] { Listening, Reading, Writing, Speaking };
}

public class SpouseInfo
{
    public EducationLevel? Education { get; set; }
    public LanguageAbilities? FirstLanguage { get; set; }

    [Range(0, 50)]
    public int? CanadianWorkYears { get; set; }
}

public class CrsScoreRequest
{
    [Range(0, 120)]
    public int Age { get; set; }

    public EducationLevel Education { get; set; }
    public bool FirstLanguageIsFrench { get; set; }
    public LanguageAbilities FirstLanguage { get; set; } = new();
    public LanguageAbilities? SecondLanguage { get; set; }

    [Range(0, 50)]
    public int CanadianWorkYears { get; set; }

    [Range(0, 50)]
    public int ForeignWorkYears { get; set; }
    public bool HasCertificateOfQualification { get; set; }
    public bool HasProvincialNomination { get; set; }
    public bool HasSiblingInCanada { get; set; }
    public CanadianEducationLevel CanadianEducation { get; set; }
    public SpouseInfo? Spouse { get; set; }
}
