using System.Collections.Generic;

namespace Genie.Common.Types;

public record Party : CosmosBase
{
    public string? Name { get; set; }
    public Certificate? Certificate { get; set; }
    public enum PartyType
    {
        Person = 0,
        Channel = 1,
        IdentityProvider = 2
    }
    public PartyType Type { get; set; }
    public List<Assessment> Assessments { get; set; } = [];
    public List<PartyCommunication> Communications { get; set; } = [];
    public List<PartyRelationship> Relationships { get; set; } = [];
    public List<Event> Events { get; set; } = [];
    public List<Artifact> Artifacts { get; set; } = [];

    public CommunicationIdentity? FindProvider()
    {
        return this.Communications.Single(t => t.LocalityCode == "ProviderId" && t.CommunicationIdentity != null && t.CommunicationIdentity.Relationship == CommunicationIdentity.CommunicationType.Provider).CommunicationIdentity;

    }
}
