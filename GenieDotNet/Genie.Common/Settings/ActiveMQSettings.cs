namespace Genie.Common.Settings;

public class ActiveMQSettings(string connectionString, string ingress, string username, string password)
{
    public string ConnectionString { get; set; } = connectionString;
    public string Ingress { get; set; } = ingress;
    public string Username { get; set; } = username;
    public string Password { get; set; } = password;
}