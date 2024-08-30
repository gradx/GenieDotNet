using Microsoft.Extensions.ObjectPool;

namespace Genie.Common.Performance
{
    public class GeniePooledObject
    {
        public string EventChannel { get; set; } = Guid.NewGuid().ToString("N");
        public int Counter { get; set; }
    }


    public class LimitedPooledObjectPolicy<T> : PooledObjectPolicy<T> where T : class, new()
    {
        private static int Limit = 10;
        private int count;
        private ManualResetEvent waitHandle = new(false);


        public LimitedPooledObjectPolicy(int limit)
        {
            Limit = limit;
        }

        public override T Create()
        {
            Mutex iso = new Mutex(false, "LimitedPooledObjectPolicy");
            iso.WaitOne();

            if (count + 1 > Limit)
                waitHandle.WaitOne();

            count++;

            iso.ReleaseMutex();

            return new T();
        }

        public override bool Return(T obj)
        {
            count--;
            waitHandle.Set();

            return true;
        }
    }
}
