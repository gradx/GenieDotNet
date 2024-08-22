namespace Genie.Common.Settings;

public class ZloggerSettings(string path)
{
    public string Path { get; set; } = path;
}