namespace Genie.Common.Settings;

public class KafkaSettings(string host, string ingress, string events, string mountPath)
{
    public string Host => host;
    public string Ingress => ingress;
    public string Events => events;
    public string MountPath => mountPath;
}