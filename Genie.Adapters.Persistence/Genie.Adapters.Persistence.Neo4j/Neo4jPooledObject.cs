using Neo4j.Driver;

namespace Genie.Adapters.Persistence.Neo4j;

public class Neo4jPooledObject
{
    // localhost:7444 neo4j/neo4j
    public readonly IDriver Driver = GraphDatabase.Driver("neo4j://localhost:7687", AuthTokens.Basic("neo4j", "genieinabottle"));
    public readonly IAsyncSession Session;
    public Neo4jPooledObject()
    {
        Session = Driver.AsyncSession();
    }
}