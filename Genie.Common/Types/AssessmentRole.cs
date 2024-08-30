
namespace Genie.Common.Types;

public record AssessmentPartyRole
{
    public string PartyId { get; set; } = "";
    public enum AssessmentRole
    {
        Whatever = 0
    }
    public AssessmentRole PartyRole { get; set; }
    public DateTime BeginDate { get; set; }
    public DateTime? EndDate { get; set; }
}