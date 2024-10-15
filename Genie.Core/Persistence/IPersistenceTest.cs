
namespace Genie.Core.Persistence;

public interface IPersistenceTest
{
    public int Payload { get; set; }
    public void CreateDB();
    public void CreateIndex();
    public bool WriteJson(long i);
    public bool ReadJson(long i);

    public Task<bool> WritePostal(CountryPostalCode message);
    public Task<bool> ReadPostal(CountryPostalCode message);

    public Task<bool> QueryPostal(CountryPostalCode message);
    public Task<bool> SelfJoinPostal(CountryPostalCode message);
}