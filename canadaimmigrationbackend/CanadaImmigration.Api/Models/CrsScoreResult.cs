namespace CanadaImmigration.Api.Models;

public class CrsBreakdown
{
    public int Age { get; set; }
    public int Education { get; set; }
    public int FirstLanguage { get; set; }
    public int SecondLanguage { get; set; }
    public int CanadianWorkExperience { get; set; }
    public int SpouseFactors { get; set; }
    public int SkillTransferability { get; set; }
    public int Additional { get; set; }
}

public class CrsScoreResult
{
    public int TotalScore { get; set; }
    public CrsBreakdown Breakdown { get; set; } = new();
}
