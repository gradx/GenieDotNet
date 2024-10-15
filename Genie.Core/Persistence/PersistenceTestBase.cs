namespace Genie.Core.Persistence;

public abstract class PersistenceTestBase : IPersistenceTest
{
    public const int JsonSize = 4000;
    public int Payload { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public void RunTests(int i)
    {
        WriteJson(i);
        ReadJson(i);
        WritePostal(i).GetAwaiter().GetResult();
        UpdatePostal(i).GetAwaiter().GetResult();
        ReadPostal(i).GetAwaiter().GetResult();
        QueryPostal(i).GetAwaiter().GetResult();
        SelfJoinPostal(i).GetAwaiter().GetResult();
    }
    public abstract void CreateDB();

    public abstract void CreateIndex();

    public abstract bool WriteJson(long i);

    public abstract bool ReadJson(long i);

    public abstract Task<bool> WritePostal(CountryPostalCode message);

    public abstract Task<bool> UpdatePostal(CountryPostalCode message);

    public abstract Task<bool> ReadPostal(CountryPostalCode message);

    public abstract Task<bool> QueryPostal(CountryPostalCode message);

    public abstract Task<bool> SelfJoinPostal(CountryPostalCode message);

    public Task<bool> WritePostal(int i) 
    {
        return WritePostal(CountryPostalCode.GetCode(i));
    }

    public Task<bool> UpdatePostal(int i)
    {
        return UpdatePostal(CountryPostalCode.GetCode(i));
    }

    public Task<bool> ReadPostal(int i)
    {
        return ReadPostal(CountryPostalCode.GetCode(i));
    }

    public Task<bool> QueryPostal(int i)
    {
        return QueryPostal(CountryPostalCode.GetCode(i));
    }

    public Task<bool> SelfJoinPostal(int i)
    {
        return SelfJoinPostal(CountryPostalCode.GetCode(i));
    }
}