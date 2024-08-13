using System.Text.Json.Serialization;

namespace Genie.Common.Types;

public record Artifact
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? ContentType { get; set; }
    public string? FileName { get; set; }
    public string? Container { get; set; }
    public string? PublicUri { get; set; }
    public string? ServerUri { get; set; }
    public long Length { get; set; }
    public SealedEnvelope? SealedEnvelope { get; set; }
    public string? Hash { get; set; }
    [JsonIgnore]
    public bool Touched { get; set; }
    public List<KeyValuePair> Properties { get; set; } = [];
    public enum ArtifactType
    {
        Empty = 0,
        Profile = 1, // Person or Channel
        Header = 2,
        Resource = 3,
        Message = 4,
        DirectMessage = 5,
        Kiosk = 6
    }
    public ArtifactType Category { get; set; }
    public void AddUri(GenieContext context, string container, string fileName, string contentType)
    {
        Container = container;
        FileName = fileName;
        ContentType = contentType;
        ServerUri = context.Azure.Storage.Server + "/" + context.Azure.Storage.Share;
        Touched = true;
    }
}