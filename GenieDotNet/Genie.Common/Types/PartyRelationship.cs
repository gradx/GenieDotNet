using System;
using System.Collections.Generic;

namespace Genie.Common.Types;
public record PartyRelationship
{
    public string? RelatedPartyId { get; set; }
    public enum RelationshipType
    {
        Channel = 0,
        Member = 1,
    }
    public RelationshipType? Relationship { get; set; }
    public DateTime? BeginDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<PartyRelationshipRole> PartyRelationshipRoles { get; set; } = [];
}