using System.Text.Json.Serialization;

namespace Damanesign.Sdk.Models;

public sealed class FieldRequest
{
    public string? File { get; set; }
    public int? Page { get; set; }
    public string? Position { get; set; }
    public string? Type { get; set; }
    public string? Value { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }

    [JsonExtensionData]
    public Dictionary<string, object?>? AdditionalProperties { get; set; }
}
