
using Genie.Common;
using Genie.Grpc;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using NetTopologySuite.Features;
using Proto;
using Proto.Cluster;

namespace Genie.Extensions.Genius;

public class GeniusGrain : GeniusServiceBase
{
    private readonly ClusterIdentity clusterIdentity;
    private int processCount = 0;
    private readonly HashSet<string> CountyProvisioner = [];
    private readonly HashSet<string> StateProvisioner = [];
    private readonly HashSet<string> MunicipalProvisioner = [];
    private readonly HashSet<string> PostalCodeProvisioner = [];


    public GeniusGrain(IContext context, ClusterIdentity clusterIdentity) : base(context)
    {
        this.clusterIdentity = clusterIdentity;
        
        StateProvisioner.Add("Pennsylvania");
        CountyProvisioner.Add("Philadelphia");
        MunicipalProvisioner.Add("Philadelphia");
        PostalCodeProvisioner.Add("19134");
    }

    public override Task<GeniusResponse> Status(StatusRequest request)
    {
        return Task.FromResult(new GeniusResponse());
    }


    public override Task<GeniusResponse> Process(GeniusRequest request)
    {
        // Observers join, everyone receives every message
        // Observer is promoted to Governor
        // Governor joins and starts filtering messages to Observers

        // Passive delivery via polling
        // Active delivery via Webhooks, SMS, Push, E-mail, etc

        // Data is stored in DuckDB or QuestDB

        processCount++;

        if (request.Key == "Shutdown")
        {
            this.Context.Stop(this.Context.Self);
            return Task.FromResult(new GeniusResponse { Level = GeniusResponse.Types.ResponseLevel.Processed, Message = "Shutdown" });
        }

        if (request.Request.Offset + 1 < processCount)
            return Task.FromResult(new GeniusResponse { Level = GeniusResponse.Types.ResponseLevel.Errored, Exception = "Dupe" });

        var genieResp = request.Value.Unpack<GenieResponse>();

        IMessage message = genieResp.ResponseCase switch
        {
            GenieResponse.ResponseOneofCase.Party => ProcessLicenseRequest(genieResp.Party),
            _ => throw new TypeLoadException($"Type not mapped: {request.Key}")
        };

        var grainResp = new GeniusResponse { Message = "respMsg",
            Response = Any.Pack(message)
        };

        //Offsets.Add(request.Offset, grainResp);

        return Task.FromResult(grainResp);
    }


    private GeniusEventResponse ProcessLicenseRequest(PartyResponse party)
    {
        var geojson = party.Party.Communications[0].CommunicationIdentity.GeographicLocation.GeoJsonLocation.GeoJson;

        var reality = GeoJsonCosmosSerializer.FromJson<Common.Types.GeoJsonLocation>(geojson);

        var attr = reality.Features.FirstOrDefault()?.Attributes;

        if (attr != null)
        {
            return ValidateLicense(attr, "zcta5_code", @$"Postal Code [{attr["zcta5_code"]}] Provisioner - Menustrating MISERable",
                   "https://www.youtube.com/watch?v=22tVWwmTie8",
                   this.PostalCodeProvisioner);

            if (DateTime.Now.Second > 50)
                return ValidateLicense(attr, "ste_name", $@"State Provisioner [{attr["ste_name"]}] - Kristina Hackerella",
                    "https://www.youtube.com/watch?v=kIDWgqDBNXA",
                    this.StateProvisioner);
            else if (DateTime.Now.Second > 45)
                return ValidateLicense(attr, "coty_name", $@"County Provisioner [{attr["coty_name"]}] - Bruno Marz",
                    "https://www.youtube.com/watch?v=UqyT8IEBkvY&t",
                    this.CountyProvisioner);
            else if (DateTime.Now.Second > 40)
                return ValidateLicense(attr, "pla_name", $@"City Provisioner [{attr["pla_name"]}] - Tuposer FISHERville",
                    "https://youtu.be/41qC3w3UUkU?si=Vey51RuiLH8_h9kP",
                    this.MunicipalProvisioner);
            else if (DateTime.Now.Second > 35)
                return ValidateLicense(attr, "zcta5_code", @$"Postal Code [{attr["zcta5_code"]}] Provisioner - Menustrating MISERable",
                    "https://www.youtube.com/watch?v=22tVWwmTie8",
                    this.PostalCodeProvisioner);
            else if (DateTime.Now.Second > 30)
                return GenerateResponse("Default Provisioner [A Hello]", "https://www.youtube.com/watch?v=DDWKuo3gXMQ", false);
            else if (DateTime.Now.Second > 25)
                return GenerateResponse("Default Provisioner [Meditation Romance]", "https://www.youtube.com/watch?v=ADwfyxpriAM", false);
            else if (DateTime.Now.Second > 20)
                return GenerateResponse("Default Provisioner [A Dustball]", "https://www.youtube.com/watch?v=Iq3zo432sAU", false);
        }

        return GenerateResponse("Default Provisioner (https://defacto2.net/home)", "https://www.youtube.com/watch?v=4Q46xYqUwZQ", false);

        static GeniusEventResponse GenerateResponse(string name, string id, bool success)
        {
            return new GeniusEventResponse {
                License = new LicenseResponse {
                    Success = success,
                    Party = new GeniusPartyResponse {
                        Party = new Party {
                            Name = name,
                            CosmosBase = new CosmosBase { Identifier = new CosmosIdentifier { Id = id } }
                        }
                    }
                }
            };
        }

        static GeniusEventResponse ValidateLicense(IAttributesTable attr, string attribute, string name, string id, HashSet<string> licenses)
        {
            var value = (string)attr[attribute];

            return GenerateResponse(name, id, value != null && licenses.Contains(value));
        }
    }
}