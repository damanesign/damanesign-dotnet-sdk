using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Damanesign.Sdk.Models;

namespace Damanesign.Sdk;

public sealed class DamanesignClient : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;
    private readonly bool _disposeHttpClient;

    public DamanesignClient(DamanesignClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.BaseUrl)) throw new ArgumentException("BaseUrl is required.", nameof(options));
        if (string.IsNullOrWhiteSpace(options.ApiKey)) throw new ArgumentException("ApiKey is required.", nameof(options));

        _httpClient = options.HttpClient ?? new HttpClient();
        _disposeHttpClient = options.HttpClient is null;
        _httpClient.BaseAddress = new Uri(EnsureTrailingSlash(options.BaseUrl), UriKind.Absolute);
        _httpClient.Timeout = options.Timeout;
        _httpClient.DefaultRequestHeaders.Accept.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.Remove("x-api-key");
        _httpClient.DefaultRequestHeaders.Add("x-api-key", options.ApiKey);
    }

    public static DamanesignClient Create(string baseUrl, string apiKey)
    {
        return new DamanesignClient(new DamanesignClientOptions { BaseUrl = baseUrl, ApiKey = apiKey });
    }

    public Task<TransactionResponse?> CreateTransactionAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default)
        => SendJsonAsync<TransactionResponse>(HttpMethod.Post, "transactions", request, cancellationToken);

    public Task<TransactionResponse?> GetTransactionAsync(string transactionId, CancellationToken cancellationToken = default)
        => SendJsonAsync<TransactionResponse>(HttpMethod.Get, $"transactions/{EncodePath(transactionId)}", null, cancellationToken);

    public Task<TransactionResponse?> UpdateTransactionAsync(string transactionId, CreateTransactionRequest request, CancellationToken cancellationToken = default)
        => SendJsonAsync<TransactionResponse>(HttpMethod.Put, $"transactions/{EncodePath(transactionId)}", request, cancellationToken);

    public Task DeleteTransactionAsync(string transactionId, CancellationToken cancellationToken = default)
        => SendNoContentAsync(HttpMethod.Delete, $"transactions/{EncodePath(transactionId)}", null, cancellationToken);

    public Task<IReadOnlyList<TransactionResponse>?> ListTransactionsAsync(TransactionFilter? filter = null, CancellationToken cancellationToken = default)
        => SendJsonAsync<IReadOnlyList<TransactionResponse>>(HttpMethod.Get, "transactions" + QueryString(filter?.ToQueryParameters()), null, cancellationToken);

    public Task<IReadOnlyList<TransactionResponse>?> ListAssignedTransactionsAsync(TransactionFilter? filter = null, CancellationToken cancellationToken = default)
        => SendJsonAsync<IReadOnlyList<TransactionResponse>>(HttpMethod.Get, "transactions/assigned" + QueryString(filter?.ToQueryParameters()), null, cancellationToken);

    public Task<TransactionResponse?> UpdateMemberAsync(string transactionId, string memberId, MemberRequest request, CancellationToken cancellationToken = default)
        => SendJsonAsync<TransactionResponse>(HttpMethod.Put, $"transactions/{EncodePath(transactionId)}/member/{EncodePath(memberId)}", request, cancellationToken);

    public Task<TransactionResponse?> UpdateMemberAuthenticationAsync(string transactionId, string memberId, string mode, CancellationToken cancellationToken = default)
        => SendJsonAsync<TransactionResponse>(HttpMethod.Put, $"transactions/{EncodePath(transactionId)}/member/{EncodePath(memberId)}/authentication/{EncodePath(mode)}", null, cancellationToken);

    public async Task<FileResponse?> UploadFileAsync(string filePath, string contentType = "application/pdf", string type = "signable", CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        await using FileStream stream = File.OpenRead(filePath);
        using MultipartFormDataContent content = new();
        using StreamContent fileContent = new(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(fileContent, "file", Path.GetFileName(filePath));

        string path = "files/upload" + QueryString(new Dictionary<string, object?>
        {
            ["contentType"] = contentType,
            ["type"] = type
        });

        using HttpRequestMessage request = new(HttpMethod.Post, path) { Content = content };
        using HttpResponseMessage response = await SendAsync(request, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        return await ReadJsonAsync<FileResponse>(response, cancellationToken).ConfigureAwait(false);
    }

    public Task StartTransactionAsync(string transactionId, CancellationToken cancellationToken = default)
        => SendNoContentAsync(HttpMethod.Post, $"transactions/{EncodePath(transactionId)}/start", null, cancellationToken);

    public Task SendReminderAsync(string transactionId, CancellationToken cancellationToken = default)
        => SendNoContentAsync(HttpMethod.Post, $"transactions/{EncodePath(transactionId)}/reminders", null, cancellationToken);

    public Task ProlongTransactionAsync(string transactionId, DateOnly expiresAt, CancellationToken cancellationToken = default)
        => SendNoContentAsync(HttpMethod.Post, $"transactions/{EncodePath(transactionId)}/prolong", expiresAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), cancellationToken);

    public Task CancelTransactionAsync(string transactionId, CancellationToken cancellationToken = default)
        => SendNoContentAsync(HttpMethod.Post, $"transactions/{EncodePath(transactionId)}/cancel", null, cancellationToken);

    public async Task<string> GetSignatureUrlAsync(string transactionId, string memberId, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, $"transactions/{EncodePath(transactionId)}/member/{EncodePath(memberId)}/url");
        using HttpResponseMessage response = await SendAsync(request, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    public Task<SealResponse?> SealDocumentAsync(SealRequest request, CancellationToken cancellationToken = default)
        => SendJsonAsync<SealResponse>(HttpMethod.Post, "seal", request, cancellationToken);

    public Task<IReadOnlyList<SealResponse>?> ListSealsAsync(SealFilter? filter = null, CancellationToken cancellationToken = default)
        => SendJsonAsync<IReadOnlyList<SealResponse>>(HttpMethod.Get, "seals" + QueryString(filter?.ToQueryParameters()), null, cancellationToken);

    public Task<FileResponse?> GetFileAsync(string fileId, CancellationToken cancellationToken = default)
        => SendJsonAsync<FileResponse>(HttpMethod.Get, $"files/{EncodePath(fileId)}", null, cancellationToken);

    public async Task<byte[]> DownloadFileAsync(string fileId, CancellationToken cancellationToken = default)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, $"files/{EncodePath(fileId)}/download");
        request.Headers.Accept.Clear();
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
        using HttpResponseMessage response = await SendAsync(request, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        return await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
    }

    public void Dispose()
    {
        if (_disposeHttpClient) _httpClient.Dispose();
    }

    private async Task<T?> SendJsonAsync<T>(HttpMethod method, string path, object? body, CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = new(method, path);
        if (body is not null)
        {
            request.Content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");
        }

        using HttpResponseMessage response = await SendAsync(request, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
        return await ReadJsonAsync<T>(response, cancellationToken).ConfigureAwait(false);
    }

    private async Task SendNoContentAsync(HttpMethod method, string path, object? body, CancellationToken cancellationToken)
    {
        using HttpRequestMessage request = new(method, path);
        if (body is not null)
        {
            request.Content = new StringContent(JsonSerializer.Serialize(body, JsonOptions), Encoding.UTF8, "application/json");
        }

        using HttpResponseMessage response = await SendAsync(request, cancellationToken).ConfigureAwait(false);
        await EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);
    }

    private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            return await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new DamanesignException("Unable to call Damanesign API.", ex);
        }
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode) return;
        string responseBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        throw new DamanesignException($"Damanesign API request failed with status {(int)response.StatusCode}.", (int)response.StatusCode, responseBody);
    }

    private static async Task<T?> ReadJsonAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        string body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return string.IsNullOrWhiteSpace(body) ? default : JsonSerializer.Deserialize<T>(body, JsonOptions);
    }

    private static string QueryString(IReadOnlyDictionary<string, object?>? parameters)
    {
        if (parameters is null || parameters.Count == 0) return string.Empty;

        List<string> parts = [];
        foreach ((string key, object? value) in parameters)
        {
            if (value is null) continue;
            if (value is System.Collections.IEnumerable values && value is not string)
            {
                foreach (object? item in values) AddQueryPart(parts, key, item);
            }
            else
            {
                AddQueryPart(parts, key, value);
            }
        }

        return parts.Count == 0 ? string.Empty : "?" + string.Join("&", parts);
    }

    private static void AddQueryPart(ICollection<string> parts, string key, object? value)
    {
        if (value is null) return;

        string text = value switch
        {
            DateOnly date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            DateTime dateTime => dateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            bool boolean => boolean.ToString().ToLowerInvariant(),
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty
        };
        parts.Add($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(text)}");
    }

    private static string EncodePath(string value) => Uri.EscapeDataString(value);

    private static string EnsureTrailingSlash(string value) => value.EndsWith("/", StringComparison.Ordinal) ? value : value + "/";
}
