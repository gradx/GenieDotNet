using Neo4j.Driver;

namespace Genie.Adapters.Persistence.Neo4j;

public class Neo4jPooledObject
{
    public readonly IDriver Driver = GraphDatabase.Driver("neo4j://localhost:7687", AuthTokens.Basic("neo4j", "genieinabottle"));
    public Neo4jPooledObject()
    {
        
    }
}