using System;

namespace Genie.Common.Types;

public record PartyRelationshipRole
{
    public enum PartyRole
    {
        Owner = 0,
        Member = 1,
        Follower = 2,
        Administrator = 3,
        Moderator = 4,
        Visitor = 5
    }

    public PartyRole Type { get; set; }
    public DateTime? BeginDate { get; set; }
    public DateTime? EndDate { get; set; }
}