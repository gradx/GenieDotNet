namespace Genie.Common.Types;

public record Assessment
{
    public DateTime BeginDate { get; set; }
    public string? Description { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Reason { get; set; }
    public List<AssessmentPartyRole> PartyRoles { get; set; } = [];
    public List<AssessmentResult> Results { get; set; } = [];
}