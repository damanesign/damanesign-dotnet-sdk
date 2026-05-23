using System.Text.Json.Serialization;

namespace Damanesign.Sdk.Models;

public sealed class MemberRequest
{
    public string? Type { get; set; }
    public string? Firstname { get; set; }
    public string? Lastname { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? AuthenticationMode { get; set; }
    public int? Position { get; set; }
    public string? User { get; set; }
    public List<FieldRequest>? Fields { get; set; }
    public string? SignatureType { get; set; }
    public string? ConsentText { get; set; }

    [JsonExtensionData]
    public Dictionary<string, object?>? AdditionalProperties { get; set; }
}
