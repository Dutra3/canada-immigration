using System.Text.Json.Serialization;

namespace CanadaImmigration.Api.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CategoryEligibility
{
    Eligible,
    NotEligible,
    Unknown
}

public class CategoryInvitation
{
    public string Category { get; set; } = string.Empty;
    public int LatestDrawNumber { get; set; }
    public DateTime LatestDrawDate { get; set; }
    public int LatestCutoff { get; set; }
    public bool WouldBeInvited { get; set; }
    public CategoryEligibility Eligibility { get; set; }
}

public class PoolPosition
{
    public string Range { get; set; } = string.Empty;
    public int CandidatesInRange { get; set; }
    public int CandidatesBelow { get; set; }
    public int TotalCandidates { get; set; }
    public double PercentBelow { get; set; }
}

public class InvitationAnalysis
{
    public int Score { get; set; }
    public List<CategoryInvitation> Categories { get; set; } = new();
    public PoolPosition? Pool { get; set; }
}
