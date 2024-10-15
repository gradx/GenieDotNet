using Genie.Core.Persistence;
using Microsoft.Extensions.ObjectPool;
using Raven.Client.Documents.Indexes;
using Raven.Client.ServerWide.Operations;

namespace Genie.Adapters.Persistence.RavenDB
{

    public class CountryPostalIndex : AbstractIndexCreationTask<CountryPostalCodeString>
    {
        public CountryPostalIndex()
        {
            // ...
            Configuration["Indexing.MapTimeoutInSec"] = "30";

            Map = postal_codes => from postal in postal_codes
                select new
                {
                    postal.PostalCode
                };
        }
    }

    public class RavenTest(int payload, ObjectPool<RavenPooledObject> pool) : PersistenceTestBase, IPersistenceTest
    {
        readonly ObjectPool<RavenPooledObject> Pool = pool;
        public int Payload { get; set; } = payload;

        public RavenTest() : this(JsonSize,new DefaultObjectPool<RavenPooledObject>(new DefaultPooledObjectPolicy<RavenPooledObject>()))
        {

        }

        public RavenTest(bool create) : this(JsonSize,new DefaultObjectPool<RavenPooledObject>(new DefaultPooledObjectPolicy<RavenPooledObject>()))
        {
            if (create)
                CreateDB();
        }

        public override void CreateDB()
        {
            var lease = Pool.Get();

            lease.JsonStore.Maintenance.Server.Send(new CreateDatabaseOperation(new Raven.Client.ServerWide.DatabaseRecord(RavenPooledObject.Json)));
            lease.PostalStore.Maintenance.Server.Send(new CreateDatabaseOperation(new Raven.Client.ServerWide.DatabaseRecord(RavenPooledObject.Postal)));

            Pool.Return(lease);
        }

        public override void CreateIndex()
        {
            var lease = Pool.Get();

            lease.PostalStore.ExecuteIndex(new CountryPostalIndex());

            Pool.Return(lease);
        }

        public override bool WriteJson(long i)
        {
            bool result = true;
            var lease = Pool.Get();

            try
            {

                var test = new PersistenceTestModel
                {
                    Id = i.ToString(),
                    Info = new('-', Payload)
                };


                using var session = lease.JsonStore.OpenSession();
                session.Store(test);

                session.SaveChanges();
            }
            catch(Exception ex)
            {
                result = false;
            }


            Pool.Return(lease);
            return result;
        }


        public override bool ReadJson(long i)
        {
            bool result = true;
            var lease = Pool.Get();

            try
            {
                using var session = lease.JsonStore.OpenSession();
                var match = session.Load<PersistenceTestModel>(i.ToString());
            }
            catch (Exception ex)
            {
                result = false;
            }

            Pool.Return(lease);
            return result;
        }


        public override async Task<bool> WritePostal(CountryPostalCode message)
        {
            bool result = true;
            var lease = Pool.Get();

            try
            {
                using var session = lease.PostalStore.OpenSession();

                session.Store(new CountryPostalCodeString
                {
                    Id = message.Id.ToString(),
                    CountryCode = message.CountryCode,
                    PostalCode = message.PostalCode,
                    Latitude = message.Latitude,
                    Longitude = message.Longitude
                });

                session.SaveChanges();
            }
            catch (Exception ex)
            {
                result = false;
            }

            Pool.Return(lease);
            return result;
        }

        public override async Task<bool> UpdatePostal(CountryPostalCode message)
        {
            return await WritePostal(message);
        }

        public override async Task<bool> ReadPostal(CountryPostalCode message)
        {
            bool result = true;
            var lease = Pool.Get();

            try
            {
                using var session = lease.PostalStore.OpenSession();
                var match = session.Load<CountryPostalCodeString>(message.Id.ToString());
            }
            catch (Exception ex)
            {
                result = false;
            }

            Pool.Return(lease);
            return result;
        }

        public override async Task<bool> QueryPostal(CountryPostalCode message)
        {
            bool result = true;
            var lease = Pool.Get();

            try
            {
                using var session = lease.PostalStore.OpenSession();
                var results = session.Query<CountryPostalCodeString>().Where(e => e.PostalCode == message.PostalCode).ToList();
            }
            catch (Exception ex)
            {
                result = false;
            }

            Pool.Return(lease);
            return result;
        }

        public override async Task<bool> SelfJoinPostal(CountryPostalCode message)
        {
            bool result = true;
            var lease = Pool.Get();

            try
            {
                using var session = lease.PostalStore.OpenSession();

                var match = session.Load<CountryPostalCodeString>(message.Id.ToString());
                var results = session.Query<CountryPostalCodeString>().Where(e => e.PostalCode == match.PostalCode && e.Id != message.Id.ToString()).ToList();
            }
            catch (Exception ex)
            {
                result = false;
            }

            Pool.Return(lease);
            return result;
        }
    }
}
