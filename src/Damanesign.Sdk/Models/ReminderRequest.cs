namespace Damanesign.Sdk.Models;

public sealed class ReminderRequest
{
    public string? Id { get; set; }
    public bool? Enabled { get; set; }
    public int? Interval { get; set; }
    public int? Limit { get; set; }
    public int? Counter { get; set; }
}
