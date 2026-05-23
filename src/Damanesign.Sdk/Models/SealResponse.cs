using System.Text.Json.Serialization;

namespace Damanesign.Sdk.Models;

public sealed class SealResponse
{
    public string? Id { get; set; }
    public string? Creator { get; set; }
    public string? Type { get; set; }
    public FileResponse? File { get; set; }
    public string? Certificate { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonExtensionData]
    public Dictionary<string, object?>? AdditionalProperties { get; set; }
}
