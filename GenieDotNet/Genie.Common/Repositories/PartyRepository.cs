using Genie.Common.Types;
using Genie.Common.Utils;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Genie.Common.Repositories
{
    public class PartyRepository
    {
        public static string PartitionKey(string channelId)
        {
            return channelId;
        }

        public static async Task<Party> InsertOrUpdate(PartyRequest request, GenieContext genieContext, CancellationToken cancellationToken)
        {
            var container = genieContext.CosmosClient.GetContainer(genieContext.Azure.CosmosDB.Id, "party");
            Party c;

            if (request.Party!.IsNew())
            {
                request.Party.Id = request.Party.Id.NullOrEmpty(Guid.NewGuid().ToString("N"));

                request.Party.PartitionKey = PartitionKey(request.Party.Id);
                request.Party.Created = DateTime.UtcNow;
                request.Party.Events.Add(new Event
                {
                    Code = "Registered",
                    EventDate = DateTime.UtcNow,
                    Info = "partyId: " + request.Party!.Id,
                    Origin = request.Origin,
                    DeviceId = request.DeviceId,
                    IpAddressDestination = request.IPAddressSource
                });

                c = await container.CreateItemAsync(request.Party, cancellationToken: cancellationToken);
            }
            else
                c = await container.ReplaceItemAsync(request.Party, request.Party.Id,
                    new PartitionKey(request.Party.PartitionKey),
                    new ItemRequestOptions { IfMatchEtag = request.Party._etag }, cancellationToken: cancellationToken);


            return c;
        }
    }
}
