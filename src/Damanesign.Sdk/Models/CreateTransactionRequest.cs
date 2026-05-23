using System.Text.Json.Serialization;

namespace Damanesign.Sdk.Models;

public sealed class CreateTransactionRequest
{
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? DeliveryMode { get; set; }
    public string? AuthenticationMode { get; set; }
    public string? Description { get; set; }
    public string? ExpiresAt { get; set; }
    public bool? Ordered { get; set; }
    public List<MemberRequest>? Members { get; set; }
    public List<QrCodeFieldRequest>? QrCode { get; set; }
    public ReminderRequest? Reminder { get; set; }
    public string? Workspace { get; set; }
    public string? TemplateId { get; set; }
    public string[]? TagIds { get; set; }

    [JsonExtensionData]
    public Dictionary<string, object?>? AdditionalProperties { get; set; }
}
