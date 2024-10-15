namespace Genie.Core.Settings;

public class ZloggerSettings(string path)
{
    public string Path { get; set; } = path;
}