using System.Text.Json.Serialization;

namespace Damanesign.Sdk.Models;

public sealed class TransactionResponse
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? ExpiresAt { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
    public string? DeliveryMode { get; set; }
    public string? AuthenticationMode { get; set; }
    public string? Creator { get; set; }
    public string? CreatorName { get; set; }
    public bool? Ordered { get; set; }
    public List<Dictionary<string, object?>>? Members { get; set; }
    public List<Dictionary<string, object?>>? Files { get; set; }
    public List<Dictionary<string, object?>>? QrCode { get; set; }
    public List<Dictionary<string, object?>>? Tags { get; set; }
    public string? TemplateId { get; set; }
    public string? Link { get; set; }

    [JsonExtensionData]
    public Dictionary<string, object?>? AdditionalProperties { get; set; }
}
