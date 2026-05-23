namespace Damanesign.Sdk;

public sealed class DamanesignClientOptions
{
    public required string BaseUrl { get; init; }

    public required string ApiKey { get; init; }

    public HttpClient? HttpClient { get; init; }

    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
}
