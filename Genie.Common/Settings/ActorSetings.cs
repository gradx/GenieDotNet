namespace Genie.Common.Settings;

public class ActorSettings(string clusterName, string consulProvider)
{
    public string ClusterName { get; set; } = clusterName;
    public string ConsulProvider { get; set; } = consulProvider;
}