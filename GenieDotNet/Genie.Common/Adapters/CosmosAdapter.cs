using CommunityToolkit.HighPerformance;
using Genie.Common.Types;
using Genie.Common.Utils;
using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Genie.Common.Adapters;

public partial class CosmosAdapter
{
    public static PartyRequest ToCosmos(Grpc.PartyRequest req)
    {
        var r = ToCosmos<PartyRequest>(req.Request);
        r.Party = ToCosmos<Party>(req.Party);

        return r;
    }

    public static Grpc.PartyResponse FromCosmos(PartyResponse resp)
    {
        return new Grpc.PartyResponse
        {
            Party = FromCosmos(resp.Party!),
            Response = FromCosmos((BaseResponse)resp)
        };
    }

    public static Grpc.BaseResponse FromCosmos(BaseResponse r)
    {
        var b = new Grpc.BaseResponse { Info = r.Info ?? "", Success = r.Success, Time = Epoch.Convert(DateTime.UtcNow) };
        b.Errors.Add(r.Error ?? "");
        return b;
    }

    public static T ToCosmos<T>(Grpc.BaseRequest request) where T : BaseRequest, new()
    {
        var r = ToCosmos<T>(request.CosmosBase);

        r.Origin = ToCosmos(request.Origin);
        r.Info = request.Info;
        r.RequestTtl = Epoch.Convert(request.RequestTtl);
        r.GracePeriod = request.GracePeriod;
        r.HorizontalAccuracy = request.HorizontalAccuracy;
        r.VerticalAccuracy = request.VerticalAccuracy;
        r.Type = request.Type;
        r.IPAddressDestination = request.IpAddressDestination;
        r.IPAddressSource = request.IpAddressSource;
        r.Length = request.Length;
        r.Info = request.Info;
        r.Origin = ToCosmos(request.Origin);

        if (Guid.TryParse(request.DeviceId, out Guid deviceId))
            r.DeviceId = deviceId;

        return r;
    }

    public static Grpc.KeyValuePair FromCosmos(Types.KeyValuePair kvp)
    {
        return new Grpc.KeyValuePair { Key = kvp.Key ?? "", Value = kvp.Value ?? "" };
    }

    public static Types.KeyValuePair ToCosmos(Grpc.KeyValuePair kvp)
    {
        return new Types.KeyValuePair { Key = kvp.Key.Null(), Value = kvp.Value.Null() };
    }

    public static Grpc.Artifact? FromCosmos(Artifact ar)
    {
        if (ar == null) return null;

        var a = new Grpc.Artifact
        {
            Category = (Grpc.Artifact.Types.Category)ar.Category,
            Code = ar.Code ?? "",
            PublicUri = ar.PublicUri ?? "",
            ServerUri = ar.ServerUri ?? "",
            Length = ar.Length,
            Name = ar.Name ?? "",
            SealedEnvelope = FromCosmos(ar.SealedEnvelope!),
            //Hash = ar.Hash
        };

        foreach (var p in ar.Properties)
            a.Properties.Add(FromCosmos(p));

        return a;
    }

    public static Artifact? ToCosmos(Grpc.Artifact ar)
    {
        if (ar == null) return null;

        return new Artifact
        {
            Code = ar.Code.Null(),
            PublicUri = ar.PublicUri.Null(),
            ServerUri = ar.ServerUri.Null(),
            Length = ar.Length,
            Name = ar.Name.Null(),
            SealedEnvelope = ToCosmos(ar.SealedEnvelope),
            Category = (Artifact.ArtifactType)ar.Category,
            Container = ar.Container.Null(),
            ContentType = ar.ContentType.Null(),
            FileName = ar.Filename.Null(),
            Hash = ar.Hash.Null(),
            Properties = ar.Properties.Select(t => ToCosmos(t)).ToList()
        };
    }

    public static T ToCosmos<T>(Grpc.Party party) where T : Party, new()
    {
        var p = ToCosmos<T>(party.CosmosBase);

        p.Name = party.Name.Null();
        p.Certificate = ToCosmos(party.Certificate);
        p.Type = (Party.PartyType)party.Type;

        foreach (var pc in party.Communications)
            p.Communications.Add(ToCosmos(pc));

        foreach (var a in party.Artifacts)
            p.Artifacts.Add(ToCosmos(a)!);

        foreach (var e in party.Events)
            p.Events.Add(ToCosmos(e));

        foreach (var pr in party.Relationships)
            p.Relationships.Add(ToCosmos(pr));

        return p;
    }

    public static Grpc.Event FromCosmos(Event ev)
    {
        var e = new Grpc.Event
        {
            Code = ev.Code ?? "",
            DeviceId = ev.DeviceId.ToString(),
            EventDate = Epoch.Convert(ev.EventDate),
            Info = ev.Info ?? "",
            IpAddressDestination = ev.IpAddressDestination ?? "",
            IpAddressSource = ev.IpAddressSource ?? "",
            Origin = FromCosmos(ev.Origin!),
            CosmosBase = FromCosmos((CosmosBase)ev)
        };

        return e;
    }
    public static Event ToCosmos(Grpc.Event ev)
    {
        var e = ToCosmos<Event>(ev.CosmosBase);

        e.Code = ev.Code;
        e.EventDate = (DateTime)Epoch.Convert(ev.EventDate)!;
        e.Info = ev.Info.Null();
        e.IpAddressDestination = ev.IpAddressDestination.Null();
        e.IpAddressSource = ev.IpAddressSource.Null();
        e.Origin = ToCosmos(ev.Origin);

        if (!string.IsNullOrEmpty(ev.DeviceId))
            e.DeviceId = Guid.Parse(ev.DeviceId);

        return e;
    }

    public static Grpc.Party FromCosmos(Party party)
    {
        var p = new Grpc.Party
        {
            Name = party.Name ?? "",
            Type = (Grpc.Party.Types.PartyType)party.Type,
            CosmosBase = FromCosmos((CosmosBase)party),
        };

        p.CosmosBase.Ttl = party._ttl ?? 0;
        p.CosmosBase.Ts = party._ts ?? 0;

        foreach (var a in party.Artifacts)
            p.Artifacts.Add(FromCosmos(a));

        foreach (var pc in party.Communications)
            p.Communications.Add(FromCosmos(pc));

        foreach (var e in party.Events)
            p.Events.Add(FromCosmos(e));

        foreach (var pr in party.Relationships)
            p.Relationships.Add(FromCosmos(pr));

        return p;
    }

    public static Grpc.AssessmentPartyRole FromCosmos(AssessmentPartyRole apr)
    {
        return new Grpc.AssessmentPartyRole
        {
            BeginDate = Epoch.Convert(apr.BeginDate),
            EndDate = Epoch.Convert(apr.EndDate),
            PartyId = apr.PartyId,
            PartyRole = (Grpc.AssessmentPartyRole.Types.Role)apr.PartyRole!
        };
    }

    public static AssessmentPartyRole ToCosmos(Grpc.AssessmentPartyRole apr)
    {
        return new AssessmentPartyRole
        {
            BeginDate = (DateTime)Epoch.Convert(apr.BeginDate)!,
            EndDate = Epoch.Convert(apr.EndDate),
            PartyId = apr.PartyId,
            PartyRole = (AssessmentPartyRole.AssessmentRole)apr.PartyRole
        };
    }



    public static Grpc.PartyRelationship FromCosmos(PartyRelationship pr)
    {
        var r = new Grpc.PartyRelationship
        {
            RelatedPartyId = pr.RelatedPartyId ?? "",
            Relationship = (Grpc.PartyRelationship.Types.RelationshipType)pr.Relationship!,
            BeginDate = Epoch.Convert(pr.BeginDate),
            EndDate = Epoch.Convert(pr.EndDate)
        };

        foreach (var prr in pr.PartyRelationshipRoles)
            r.PartyRelationshipRoles.Add(FromCosmos(prr));

        return r;
    }

    public static PartyRelationship ToCosmos(Grpc.PartyRelationship pr)
    {
        var r = new PartyRelationship
        {
            RelatedPartyId = pr.RelatedPartyId.Null(),
            Relationship = (PartyRelationship.RelationshipType)pr.Relationship,
            BeginDate = Epoch.Convert(pr.BeginDate),
            EndDate = Epoch.Convert(pr.EndDate)
        };

        foreach (var prr in pr.PartyRelationshipRoles)
            r.PartyRelationshipRoles.Add(ToCosmos(prr));

        return r;
    }

    public static Grpc.PartyRelationshipRole FromCosmos(PartyRelationshipRole pr)
    {
        return new Grpc.PartyRelationshipRole
        {
            Type = (Grpc.PartyRelationshipRole.Types.RelationshipRole)pr.Type,
            BeginDate = Epoch.Convert(pr.BeginDate),
            EndDate = Epoch.Convert(pr.EndDate)
        };
    }

    public static PartyRelationshipRole ToCosmos(Grpc.PartyRelationshipRole pr)
    {
        return new PartyRelationshipRole
        {
            Type = (PartyRelationshipRole.PartyRole)pr.Type,
            BeginDate = Epoch.Convert(pr.BeginDate),
            EndDate = Epoch.Convert(pr.EndDate)
        };
    }

    public static Grpc.GeographicLocation FromCosmos(Types.GeographicLocation loc)
    {
        var gl = new Grpc.GeographicLocation
        {
            GeographicLocationTypeCode = loc.GeographicLocationTypeCode ?? "",
            LocationCode = loc.LocationCode ?? "",
            LocationName = loc.LocationName ?? "",
            LocationNumber = loc.LocationNumber ?? "",
            StateCode = loc.StateCode ?? ""
        };

        if (loc.LocationAddress != null)
            gl.LocationAddress = FromCosmos(loc.LocationAddress);

        if (loc.GeoJsonLocation != null)
            gl.GeoJsonLocation = FromCosmos(loc.GeoJsonLocation);

        return gl;
    }

    public static FileContentResult FileContentResult<T>(T r) where T : Google.Protobuf.IMessage, new()
    {
        using (var ms = new MemoryStream())
        {
            MessageExtensions.WriteTo(r, ms);
            return new FileContentResult(ms.ToArray(), "application/octet-stream") { FileDownloadName = "grpc" };
        }
    }

    public static Grpc.LocationAddress FromCosmos(LocationAddress la)
    {
        return new Grpc.LocationAddress
        {
            CountryCode = la.CountryCode ?? "",
            Line1Address = la.Line1Address ?? "",
            Line2Address = la.Line2Address ?? "",
            MunicipalityName = la.MunicipalityName ?? "",
            PostalCode = la.PostalCode ?? "",
            StateCode = la.StateCode ?? ""
        };
    }

    public static LocationAddress ToCosmos(Grpc.LocationAddress la)
    {
        return new LocationAddress
        {
            CountryCode = la.CountryCode.Null(),
            Line1Address = la.Line1Address.Null(),
            Line2Address = la.Line2Address.Null(),
            MunicipalityName = la.MunicipalityName.Null(),
            PostalCode = la.PostalCode.Null(),
            StateCode = la.StateCode.Null()
        };
    }


    public static GeographicLocation ToCosmos(Grpc.GeographicLocation loc)
    {
        var gl = new GeographicLocation
        {
            GeographicLocationTypeCode = loc.GeographicLocationTypeCode.Null(),
            LocationCode = loc.LocationCode.Null(),
            LocationName = loc.LocationName.Null(),
            LocationNumber = loc.LocationNumber.Null(),
            StateCode = loc.StateCode.Null()
        };

        if (loc.LocationAddress != null)
            gl.LocationAddress = ToCosmos(loc.LocationAddress);

        if (loc.GeoJsonLocation != null)
            gl.GeoJsonLocation = ToCosmos(loc.GeoJsonLocation);

        return gl;
    }

    public static GeoJsonLocation? ToCosmos(Grpc.GeoJsonLocation gs)
    {
        GeoJsonLocation? g = null;

        if (!string.IsNullOrEmpty(gs.GeoJson))
            g = GeoJsonCosmosSerializer.FromJson<GeoJsonLocation>(gs.GeoJson);
        else if (gs.ShapeCase == Grpc.GeoJsonLocation.ShapeOneofCase.Circle)
        {
            var geo = GeometryCalculator.Polygon(gs.Circle.Centroid.Latitude, gs.Circle.Centroid.Longitude, gs.Circle.Radius, GeometryCalculator.Shape.Circle);

            var list = gs.Attributes.Select(e => CosmosAdapter.ToCosmos(e));
            var projected = list.Select(e => new KeyValuePair<string, object>(e.Key!, e.Value!)).ToList();
            //var attrTable = new NetTopologySuite.Features.AttributesTable(projected);

            // TODO: Move
            g = new GeoJsonLocation();
            g.Features.Add(new NetTopologySuite.Features.Feature(geo, new NetTopologySuite.Features.AttributesTable(projected)));

            g.Shape = new GeoJsonCircle { Centroid = ToCosmos(gs.Circle.Centroid), Radius = gs.Circle.Radius };
        }

        foreach (var s in gs.Schedules)
            g?.Schedules.Add(ToCosmos(s));

        return g;
    }

    public static Grpc.GeoJsonCircle FromCosmos(GeoJsonCircle circle)
    {
        return new Grpc.GeoJsonCircle { Centroid = FromCosmos(circle.Centroid!), Radius = circle.Radius };
    }

    public static Grpc.Coordinate FromCosmos(Coordinate coord)
    {
        return new Grpc.Coordinate() { Altitude = coord.Altitude, Latitude = coord.Latitude, Longitude = coord.Longitude };
    }

    public static Coordinate ToCosmos(Grpc.Coordinate c)
    {
        return new Coordinate { Latitude = c.Latitude, Longitude = c.Longitude, Altitude = c.Altitude };
    }

    public static Grpc.Assessment FromCosmos(Assessment ass)
    {
        var a = new Grpc.Assessment
        {
            BeginDate = Epoch.Convert(ass.BeginDate),
            Description = ass.Description ?? "",
            Reason = ass.Description ?? "",
            EndDate = Epoch.Convert(ass.EndDate),
        };

        foreach (var s in ass.PartyRoles)
            a.PartyRoles.Add(FromCosmos(s));

        return a;
    }

    public static T? ToCosmos<T>(Grpc.Assessment ma) where T : Assessment, new()
    {
        return (ma == null) ? null : new T
        {
            BeginDate = (DateTime)Epoch.Convert(ma.BeginDate)!,
            Description = ma.Description.Null(),
            EndDate = Epoch.Convert(ma.EndDate),
            Reason = ma.Reason.Null(),
            PartyRoles = ma.PartyRoles.Select(e => ToCosmos(e)).ToList()
        };

    }

    public static Grpc.Schedule FromCosmos(Schedule s)
    {
        return new Grpc.Schedule
        {
            BeginTime = Epoch.Convert(s.BeginTime),
            EndTime = Epoch.Convert(s.EndTime),
            Sunday = s.Sunday,
            Monday = s.Monday,
            Tuesday = s.Tuesday,
            Wednesday = s.Wednesday,
            Thursday = s.Thursday,
            Friday = s.Friday,
            Saturday = s.Saturday
        };
    }

    public static Schedule ToCosmos(Grpc.Schedule s)
    {
        return new Schedule
        {
            BeginTime = Epoch.Convert(s.BeginTime),
            EndTime = Epoch.Convert(s.EndTime),
            Sunday = s.Sunday,
            Monday = s.Monday,
            Tuesday = s.Tuesday,
            Wednesday = s.Wednesday,
            Thursday = s.Thursday,
            Friday = s.Friday,
            Saturday = s.Saturday
        };
    }

    public static Grpc.GeoJsonLocation FromCosmos(GeoJsonLocation gs)
    {
        var g = new Grpc.GeoJsonLocation { GeoJson = GeoJsonCosmosSerializer.ToJson(gs) };

        if (gs.Shape != null) // may be others
            g.Circle = new Grpc.GeoJsonCircle
            {
                Centroid = FromCosmos(gs.Shape.Centroid!),
                //Radius = gs.Shape.Radius 
            };

        foreach (var s in gs.Schedules)
            g.Schedules.Add(FromCosmos(s));

        foreach (var s in gs.Assessments)
            g.Assessments.Add(FromCosmos(s));

        //gs.Features[0].Attributes


        return g;
    }

    public static Grpc.CosmosBase FromCosmos(CosmosBase cb)
    {
        var r = new Grpc.CosmosBase
        {
            Identifier = new Grpc.CosmosIdentifier { Id = cb.Id ?? "", PartitionKey = cb.PartitionKey ?? "" },
            Self = cb._self ?? "",
            Rid = cb._rid ?? "",
            Etag = cb._etag ?? "",
            Attachments = cb._attachments ?? "",
            Created = Epoch.Convert(cb.Created),
            Ts = cb._ts ?? 0,
            Ttl = cb._ttl ?? 0
        };


        return r;
    }

    public static T ToCosmos<T>(Grpc.CosmosBase cb) where T : CosmosBase, new()
    {
        if (cb == null)
            return new T();

        var r = new T
        {
            Id = cb.Identifier?.Id ?? "",
            PartitionKey = cb.Identifier?.PartitionKey ?? "",
            Created = Epoch.Convert(cb.Created),
            _self = cb.Self,
            _rid = cb.Rid,
            _etag = cb.Etag,
            _attachments = cb.Attachments,
        };

        if (cb.Ts > 0)
            r._ts = cb.Ts;

        if (cb.Ttl != 0)
            r._ttl = cb.Ttl;

        return r;
    }

    public static Grpc.GeoCryptoKey FromCosmos(GeoCryptoKey k)
    {
        return new Grpc.GeoCryptoKey { Key = k.Key ?? "", KeyType = (Grpc.KeyType)k.KeyType, KeyUsage = (Grpc.KeyUsage)k.KeyUsage, IsPrivate = k.IsPrivate, Id = k.Id ?? "" };
    }

    public static Certificate? ToCosmos(Grpc.Certificate c)
    {
        if (c == null) return null;

        var cert = new Certificate { SubscriptionId = c.SubscriptionId.Null(), Exportable = c.Exportable };
        foreach (var k in c.Keys)
            cert.Keys.Add(ToCosmos(k)!);

        return cert;

    }

    public static T ToCosmos<T>(Grpc.GeoCryptoKey k) where T : GeoCryptoKey, new()
    {
        return new T { Key = k.Key.Null(), KeyType = (GeoCryptoKey.CryptoKeyType)k.KeyType, IsPrivate = k.IsPrivate, Id = k.Id };
    }


    public static SealedEnvelope? ToCosmos(Grpc.SealedEnvelope d)
    {
        if (d == null) return null;

        var s = ToCosmos<SealedEnvelope>(d.Key);
        s.Hkdf = d.Hkdf.Null();
        s.Data = d.Data.Null();
        s.Nonce = d.Nonce.Null();
        s.Tag = d.Tag.Null();
        s.Cipher = (SealedEnvelope.CipherType)d.Cipher;
        return s;

    }

    public static Grpc.SealedEnvelope? FromCosmos(SealedEnvelope d)
    {
        if (d == null) return null;

        return new Grpc.SealedEnvelope
        {
            Key = FromCosmos((GeoCryptoKey)d),
            Hkdf = d.Hkdf ?? "",
            Data = d.Data ?? "",
            Nonce = d.Nonce ?? "",
            Tag = d.Tag ?? "",
            Cipher = (Grpc.SealedEnvelope.Types.SealedEnvelopeType)d.Cipher
        };
    }


    public static PartyCommunication ToCosmos(Grpc.PartyCommunication com)
    {
        var ci = new CommunicationIdentity { QualifierValue = com.CommunicationIdentity.QualifierValue, Relationship = (CommunicationIdentity.CommunicationType)com.CommunicationIdentity.Relationship };

        if (com.CommunicationIdentity.GeographicLocation != null)
            ci.GeographicLocation = ToCosmos(com.CommunicationIdentity.GeographicLocation);

        return new PartyCommunication { CommunicationIdentity = ci, BeginDate = (DateTime)Epoch.Convert(com.BeginDate)!, EndDate = Epoch.Convert(com.EndDate), LocalityCode = com.LocalityCode.Null() };
    }

    public static Grpc.PartyCommunication FromCosmos(PartyCommunication com)
    {
        var pc = new Grpc.PartyCommunication { CommunicationIdentity = new Grpc.CommunicationIdentity() };
        pc.CommunicationIdentity.QualifierValue = com.CommunicationIdentity?.QualifierValue ?? "";
        pc.CommunicationIdentity.Relationship = (Grpc.CommunicationIdentity.Types.CommunicationType)com.CommunicationIdentity?.Relationship!;

        if (com.CommunicationIdentity.GeographicLocation != null)
            pc.CommunicationIdentity.GeographicLocation = FromCosmos(com.CommunicationIdentity.GeographicLocation);

        pc.BeginDate = Epoch.Convert(com.BeginDate);
        pc.EndDate = Epoch.Convert(com.EndDate);
        pc.LocalityCode = com.LocalityCode ?? "";

        return pc;
    }
}