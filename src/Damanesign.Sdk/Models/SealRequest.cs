using System.Text.Json.Serialization;

namespace Damanesign.Sdk.Models;

public sealed class SealRequest
{
    public string? File { get; set; }
    public string? Certificate { get; set; }
    public string? Code { get; set; }
    public string? Image { get; set; }
    public List<SealFieldRequest>? Fields { get; set; }

    [JsonExtensionData]
    public Dictionary<string, object?>? AdditionalProperties { get; set; }
}
