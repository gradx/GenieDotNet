
namespace Genie.Core.Settings;

public class PulsarSettings(string connectionString)
{
    public string ConnectionString { get; set; } = connectionString;
}