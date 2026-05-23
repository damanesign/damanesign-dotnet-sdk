using System.Text.Json.Serialization;

namespace Damanesign.Sdk.Models;

public sealed class FileResponse
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public long? Pages { get; set; }
    public string? ContentType { get; set; }
    public string? Type { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? Creator { get; set; }
    public string? Hash { get; set; }
    public string? PreHash { get; set; }
    public string? HashAlgorithm { get; set; }
    public long? Size { get; set; }
    public double? Width { get; set; }
    public double? Height { get; set; }
    public string? OwnerId { get; set; }
    public bool? IsSignedBeforeUpload { get; set; }
    public string? Link { get; set; }

    [JsonExtensionData]
    public Dictionary<string, object?>? AdditionalProperties { get; set; }
}
