using Genie.Utils;
using Microsoft.Extensions.ObjectPool;
using Raven.Client.ServerWide.Operations;

namespace Genie.Adapters.Persistence.RavenDB
{


    public class RavenTest(int payload, ObjectPool<RavenPooledObject> pool) : IPersistenceTest
    {
        readonly ObjectPool<RavenPooledObject> Pool = pool;
        public int Payload { get; set; } = payload;


        public void CreateDB()
        {
            var lease = Pool.Get();
            lease.Store.Maintenance.Server.Send(new CreateDatabaseOperation(new Raven.Client.ServerWide.DatabaseRecord(RavenPooledObject.c_Database)));

            Pool.Return(lease);
        }

        public void Write(int i)
        {
            var test = new PersistenceTest
            {
                Id = $@"new{i}",
                Info = new('-', Payload)
            };

            var lease = Pool.Get();
            using var session = lease.Store.OpenSession();
            session.Store(test);

            session.SaveChanges();

            Pool.Return(lease);
        }


        public void Read(int i)
        {
            var lease = Pool.Get();
            using var session = lease.Store.OpenSession();
            var help = session.Load<PersistenceTest>($@"new{i}");

            Pool.Return(lease);
        }
    }
}
