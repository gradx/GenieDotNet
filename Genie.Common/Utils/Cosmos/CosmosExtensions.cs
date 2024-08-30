using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Genie.Common.Utils.Cosmos;

public static class CosmosExtensions
{
    public static async Task DeleteRecursive(this BlobContainerClient cont, string path)
    {
        await foreach (var blob in cont.GetBlobsAsync(prefix: path).AsPages())
            foreach (BlobItem item in blob.Values)
                await cont.DeleteBlobIfExistsAsync(item.Name);
    }
}