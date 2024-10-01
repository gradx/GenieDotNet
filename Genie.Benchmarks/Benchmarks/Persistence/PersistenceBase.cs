

using BenchmarkDotNet.Attributes;
using Genie.Utils;

namespace Genie.Benchmarks.Benchmarks.Persistence;

public abstract class PersistenceBase
{
    protected readonly int threads = 1;
    protected readonly int payload = 4000;

    public IPersistenceTest persistenceTest;

    public PersistenceBase()
    {

    }

    [Benchmark]
    public void Write()
    {
        Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
        {
            persistenceTest.Write(iter);
        });
    }

    [Benchmark]
    public void Read()
    {
        Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
        {
            persistenceTest.Read(iter);
        });
    }

    //[Benchmark]
    //public void Query()
    //{
    //    Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
    //    {
    //        persistenceTest.QueryPostal(iter);
    //    });
    //}

    //[Benchmark]
    //public void SelfJoin()
    //{
    //    Parallel.For(0, threads, new ParallelOptions { MaxDegreeOfParallelism = -1 }, iter =>
    //    {
    //        persistenceTest.SelfJoinPostal(iter);
    //    });
    //}
}