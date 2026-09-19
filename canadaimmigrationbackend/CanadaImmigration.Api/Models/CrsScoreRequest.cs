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
    public int Listening { get; set; }
    public int Reading { get; set; }
    public int Writing { get; set; }
    public int Speaking { get; set; }

    public int[] All => new[] { Listening, Reading, Writing, Speaking };
}

public class SpouseInfo
{
    public EducationLevel? Education { get; set; }
    public LanguageAbilities? FirstLanguage { get; set; }
    public int? CanadianWorkYears { get; set; }
}

public class CrsScoreRequest
{
    public int Age { get; set; }
    public EducationLevel Education { get; set; }
    public bool FirstLanguageIsFrench { get; set; }
    public LanguageAbilities FirstLanguage { get; set; } = new();
    public LanguageAbilities? SecondLanguage { get; set; }
    public int CanadianWorkYears { get; set; }
    public int ForeignWorkYears { get; set; }
    public bool HasCertificateOfQualification { get; set; }
    public bool HasProvincialNomination { get; set; }
    public bool HasSiblingInCanada { get; set; }
    public CanadianEducationLevel CanadianEducation { get; set; }
    public SpouseInfo? Spouse { get; set; }
}
