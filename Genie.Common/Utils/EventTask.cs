using DeepEqual;
using Genie.Common.Types;
using Genie.Common.Utils.ChangeFeed;
using Microsoft.Extensions.Logging;
using ZLogger;
using Confluent.SchemaRegistry;
using Genie.Common.Adapters;
using Google.Protobuf;
using Genie.Common.Adapters.Kafka;
using Microsoft.Extensions.ObjectPool;
using Genie.Common.Web;


namespace Genie.Common.Utils;

public class EventTask
{
    public static async Task<Grpc.GenieResponse> Process<T>(GenieContext context, IMessage message, ILogger logger, ObjectPool<T> pool, CancellationToken cancellationToken) where T : class
    {
        switch (message)
        {
            case Grpc.PartyRequest c: return await PartyRequest(context, CosmosAdapter.ToCosmos(c), pool, cancellationToken);
            case Grpc.PartyBenchmarkRequest b: return await PartyRequestBenchmark(CosmosAdapter.ToCosmos(b), pool);
            default: logger.ZLogInformation($"{message}"); return new Grpc.GenieResponse { };
        }

    }

    public static async Task<Grpc.GenieResponse> Process<T>(GenieContext context, ObjectPool<T> pool, BaseRequest request, ILogger logger, CancellationToken cancellationToken) where T : class
    {
        Grpc.GenieResponse a = request switch
        {
            PartyRequest c => await PartyRequest(context, c, pool, cancellationToken),
            _ => Log(request, logger)
        };

        return a;
    }

    private static Grpc.GenieResponse Log(BaseRequest request, ILogger logger)
    {
        logger.ZLogInformation($"{request}");
        return new Grpc.GenieResponse { };
    }


    private static async Task<Grpc.GenieResponse> PartyRequest<T>(GenieContext context, PartyRequest request, ObjectPool<T> pool, CancellationToken cancellationToken) where T : class
    {
        try
        {
            request.Party?.ReversGeoCode(pool);

            return new Grpc.GenieResponse { 
                Party = CosmosAdapter.FromCosmos(new PartyResponse { 
                    Party = request.Party, 
                    Success = true 
                }) 
            }; 
        }
        catch (AggregateException ex)
        {
            await KafkaUtils.Post(AvroSupport.GetSchemaBuilder(), 
                new CachedSchemaRegistryClient(AvroSupport.GetSchemaRegistryConfig()),
                context.Kafka.Host, 
                request.Id, 
                new EventTaskJob(request.Id, 
                "Job Error",
                EventTaskJobStatus.Errored, 
                DateTime.UtcNow, 
                ex: ex.Message), 
                cancellationToken);

            return new Grpc.GenieResponse { 
                Party = CosmosAdapter.FromCosmos(new PartyResponse { 
                    Party = request.Party, 
                    Success = false, 
                    Error = ex.ToString() 
                }) };
        }
        catch (Exception ex)
        {
            return new Grpc.GenieResponse { 
                Party = CosmosAdapter.FromCosmos(new PartyResponse { 
                    Party = request.Party, 
                    Success = false, 
                    Error = ex.ToString() 
                }) 
            };
        }
    }


    public static async Task<Grpc.GenieResponse> PartyRequestBenchmark<T>(PartyRequest request, ObjectPool<T> pool) where T: class
    {

        // Simulate CDC
        var cosmos = CosmosAdapter.ToCosmos(GetOtherPartyRequest());
        var diff = request?.Party?.GetDeepEqualComparison(cosmos.Party!)!;
        _ = GetChangeLog(diff);

        request?.Party?.ReversGeoCode(pool);
        await Task.CompletedTask;

        //logger.ZLogInformation($"{new AvroOneOf(request, changeLog)}");

        return new Grpc.GenieResponse
        {
            Party = CosmosAdapter.FromCosmos(new PartyResponse
            {
                Party = request?.Party,
                Success = true
            })
        };


        static Grpc.PartyRequest GetOtherPartyRequest()
        {
            var p = new Grpc.PartyRequest
            {
                Request = new Grpc.BaseRequest
                {
                    CosmosBase = new Grpc.CosmosBase
                    {
                        Identifier = new Grpc.CosmosIdentifier
                        {
                            Id = Guid.NewGuid().ToString("N")
                        }
                    },
                    Origin = new Grpc.Coordinate { Latitude = 38.897678, Longitude = -77.036552 }
                },
                Party = new Grpc.Party
                {
                    CosmosBase = new Grpc.CosmosBase
                    {
                        Identifier = new Grpc.CosmosIdentifier
                        {
                            Id = Guid.NewGuid().ToString("N")
                        }
                    },
                    Name = "Senate Chambers",
                    Type = Grpc.Party.Types.PartyType.Party
                }
            };

            p.Party.Communications.Add(new Grpc.PartyCommunication
            {
                BeginDate = Epoch.Convert(DateTime.UtcNow),
                CommunicationIdentity = new Grpc.CommunicationIdentity
                {
                    GeographicLocation = new Grpc.GeographicLocation
                    {
                        LocationName = "U.S. Capitol",
                        LocationAddress = new Grpc.LocationAddress { Line1Address = "210 15th St NW", MunicipalityName = "Washington", StateCode = "DC", PostalCode = "20006" },
                        GeoJsonLocation = new Grpc.GeoJsonLocation
                        {
                            Circle = new Grpc.GeoJsonCircle
                            {
                                Centroid = new Grpc.Coordinate { Latitude = 38.889755, Longitude = -77.009132 },
                                Radius = 50
                            }
                        }
                    }
                }
            });

            p.Party.Communications.Add(new Grpc.PartyCommunication
            {
                BeginDate = Epoch.Convert(DateTime.UtcNow),
                CommunicationIdentity = new Grpc.CommunicationIdentity
                {
                    GeographicLocation = new Grpc.GeographicLocation
                    {
                        LocationName = "Washington Monument",
                        LocationAddress = new Grpc.LocationAddress { Line1Address = "2 15th St NW", MunicipalityName = "Washington", StateCode = "DC", PostalCode = "20024" },
                        GeoJsonLocation = new Grpc.GeoJsonLocation
                        {
                            Circle = new Grpc.GeoJsonCircle
                            {
                                Centroid = new Grpc.Coordinate { Latitude = 38.889477, Longitude = -77.035248 },
                                Radius = 50
                            }
                        }
                    }
                }
            });

            return p;
        }
    }


    private static ChangeLog GetChangeLog(IComparisonContext context)
    {
        var differences = new ChangeLog();

        context.Differences.ForEach(e =>
        {
            switch (e)
            {
                case DeepEqual.BasicDifference b:
                    differences.Add(new ChangeFeed.BasicDifference
                    {
                        Breadcrumb = b.Breadcrumb,
                        Value1 = SpanJson.JsonSerializer.Generic.Utf16.Serialize(b.Value1),
                        Value2 = SpanJson.JsonSerializer.Generic.Utf16.Serialize(b.Value2),
                        ChildProperty = b.ChildProperty.Empty()
                    });
                    break;
                case DeepEqual.SetDifference s:
                    differences.Add(GetSetDifference(s));
                    break;
                case DeepEqual.MissingEntryDifference m:
                    differences.Add(GetMissingDifference(m));
                    break;
                case DeepEqual.Difference d:
                    differences.Add(new ChangeFeed.Difference { Breadcrumb = d.Breadcrumb });
                    break;
            }
        });

        return differences;
    }

    private static ChangeFeed.SetDifference GetSetDifference(DeepEqual.SetDifference diff)
    {
        var result = new ChangeFeed.SetDifference
        {
            Breadcrumb =
            diff.Breadcrumb
        };

        diff.Expected.ForEach(e => result.Expected.Add(SpanJson.JsonSerializer.Generic.Utf16.Serialize(e)));
        diff.Extra.ForEach(e => result.Extra.Add(SpanJson.JsonSerializer.Generic.Utf16.Serialize(e)));

        return result;
    }

    private static ChangeFeed.MissingEntryDifference GetMissingDifference(DeepEqual.MissingEntryDifference diff)
    {
        return new ChangeFeed.MissingEntryDifference { Breadcrumb = diff.Breadcrumb,
            Side = diff.Side == DeepEqual.MissingSide.Actual ? ChangeFeed.MissingSide.Actual : ChangeFeed.MissingSide.Expected,
            Key = SpanJson.JsonSerializer.Generic.Utf16.Serialize(diff.Key),
            Value = SpanJson.JsonSerializer.Generic.Utf16.Serialize(diff.Value)
        };
    }
}