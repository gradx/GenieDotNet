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

        public bool Write(long i)
        {
            bool success = true;
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
            return success;
        }


        public bool Read(long i)
        {
            bool success = true;
            var lease = Pool.Get();
            using var session = lease.Store.OpenSession();
            var help = session.Load<PersistenceTest>($@"new{i}");

            Pool.Return(lease);
            return success;
        }

        public async Task<bool> WritePostal(CountryPostalCode message)
        {
            bool result = true;
            var lease = Pool.Get();

            try
            {
                using var session = lease.Store.OpenAsyncSession();

                await session.StoreAsync(new CountryPostalCodeString
                {
                    Id = message.Id.ToString(),
                    CountryCode = message.CountryCode,
                    PostalCode = message.PostalCode,
                    Latitude = message.Latitude,
                    Longitude = message.Longitude
                });
                await session.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = false;
            }

            Pool.Return(lease);
            return result;
        }

        public async Task<bool> ReadPostal(CountryPostalCode message)
        {
            bool result = true;
            var lease = Pool.Get();

            try
            {
                using var session = lease.Store.OpenAsyncSession();

                var match = await session.LoadAsync<CountryPostalCodeString>($@"{message.Id}");

                session.Dispose();
            }
            catch (Exception ex)
            {
                result = false;
            }

            Pool.Return(lease);
            return result;
        }

        public async Task<bool> QueryPostal(CountryPostalCode message)
        {
            bool result = true;
            var lease = Pool.Get();

            try
            {
                using var session = lease.Store.OpenSession();
                var results = session.Query<CountryPostalCodeString>().Where(e => e.PostalCode == message.PostalCode).ToList();

                session.Dispose();

            }
            catch (Exception ex)
            {
                result = false;
            }

            Pool.Return(lease);
            return result;
        }

        public async Task<bool> SelfJoinPostal(CountryPostalCode message)
        {
            bool result = true;
            var lease = Pool.Get();

            try
            {
                using var session = lease.Store.OpenSession();

                var match = session.Load<CountryPostalCodeString>($@"{message.Id}");

                var results = session.Query<CountryPostalCodeString>().Where(e => e.PostalCode == match.PostalCode).ToList();

                session.Dispose();
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
