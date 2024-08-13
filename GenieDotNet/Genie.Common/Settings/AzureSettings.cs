using MaxMind.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genie.Common.Settings
{
    public class AzureSettings(AzureStorage storage, AzureCosmos cosmosDB)
    {
        public AzureStorage Storage { get; init; } = storage;
        public AzureCosmos CosmosDB { get; init; } = cosmosDB;
    }

    public class AzureKeyPair(string key, string value)
    {
        public string Key { get; init; } = key;
        public string Value { get; init; } = value;
    }

    public class AzureCosmos(string id, string uri, string key)
    {
        public string Id { get; init; } = id;
        public string Uri { get; init; } = uri;
        public string Key { get; init; } = key;
    }

    public class AzureStorage(string connectionString, string server, string share)
    {
        public string ConnectionString { get; init; } = connectionString;
        public string Server { get; init; } = server;
        public string Share { get; init; } = share;
    }

    public class MaxMindSettings
    {
        public Reader Reader { get; init; }

        public MaxMindSettings(string mmdb)
        {
            Reader = new Reader(mmdb);
        }
    }
}
