using System.Net;
using System.Text;
using System.Text.Json;
using Damanesign.Sdk;
using Damanesign.Sdk.Models;

await TestRunner.RunAsync();

internal static class TestRunner
{
    public static async Task RunAsync()
    {
        await UploadFilePostsMultipartPayloadAsync();
        await CreateTransactionPostsJsonPayloadAsync();
        await CreateTransactionThrowsOnApiErrorAsync();
        await FiltersRepeatArrayQueryParametersAsync();
        Console.WriteLine("All tests passed.");
    }

    private static async Task UploadFilePostsMultipartPayloadAsync()
    {
        string? method = null;
        string? path = null;
        string? apiKey = null;
        string? contentType = null;
        string? body = null;

        await using TestServer server = await TestServer.StartAsync(async context =>
        {
            method = context.Request.HttpMethod;
            path = context.Request.RawUrl;
            apiKey = context.Request.Headers["x-api-key"];
            contentType = context.Request.Headers["Content-Type"];
            using StreamReader reader = new(context.Request.InputStream, context.Request.ContentEncoding);
            body = await reader.ReadToEndAsync();
            await RespondJsonAsync(context, 201, """{"id":"file_123","name":"contract.pdf","contentType":"application/pdf","type":"signable"}""");
        });

        string file = Path.Combine(Path.GetTempPath(), $"contract-{Guid.NewGuid():N}.pdf");
        await File.WriteAllTextAsync(file, "%PDF-1.4 test");
        try
        {
            using DamanesignClient client = DamanesignClient.Create(server.Url, "test-token");
            FileResponse? response = await client.UploadFileAsync(file);

            AssertEqual("POST", method);
            AssertEqual("/files/upload?contentType=application%2Fpdf&type=signable", path);
            AssertEqual("test-token", apiKey);
            AssertTrue(contentType?.StartsWith("multipart/form-data; boundary=", StringComparison.Ordinal) == true, "Expected multipart content type.");
            AssertTrue(body?.Contains("name=file", StringComparison.Ordinal) == true || body?.Contains("name=\"file\"", StringComparison.Ordinal) == true, "Expected multipart file field.");
            AssertTrue(body?.Contains("%PDF-1.4 test", StringComparison.Ordinal) == true, "Expected multipart file content.");
            AssertEqual("file_123", response?.Id);
        }
        finally
        {
            File.Delete(file);
        }
    }

    private static async Task CreateTransactionPostsJsonPayloadAsync()
    {
        string? apiKey = null;
        string? contentType = null;
        JsonDocument? requestBody = null;

        await using TestServer server = await TestServer.StartAsync(async context =>
        {
            apiKey = context.Request.Headers["x-api-key"];
            contentType = context.Request.Headers["Content-Type"];
            requestBody = await JsonDocument.ParseAsync(context.Request.InputStream);
            await RespondJsonAsync(context, 201, """{"id":"tx_123","name":"Contrat client","status":"draft"}""");
        });

        using DamanesignClient client = DamanesignClient.Create(server.Url, "test-token");
        TransactionResponse? response = await client.CreateTransactionAsync(new CreateTransactionRequest
        {
            Name = "Contrat client",
            Type = "simple",
            AuthenticationMode = "email",
            Ordered = false,
            Members =
            [
                new MemberRequest
                {
                    Type = MemberTypes.Signer,
                    Firstname = "Sara",
                    Lastname = "Amrani",
                    Email = "sara@example.com",
                    Phone = "+212600000000",
                    Fields =
                    [
                        new FieldRequest
                        {
                            File = "file_123",
                            Type = FieldTypes.Signature,
                            Page = 1,
                            Position = "141,268,151,101"
                        }
                    ]
                }
            ]
        });

        JsonElement root = requestBody!.RootElement;
        AssertEqual("test-token", apiKey);
        AssertEqual("application/json; charset=utf-8", contentType);
        AssertEqual("Contrat client", root.GetProperty("name").GetString());
        AssertEqual("simple", root.GetProperty("type").GetString());
        AssertEqual("sara@example.com", root.GetProperty("members")[0].GetProperty("email").GetString());
        AssertEqual("file_123", root.GetProperty("members")[0].GetProperty("fields")[0].GetProperty("file").GetString());
        AssertEqual("tx_123", response?.Id);
        AssertEqual("draft", response?.Status);
    }

    private static async Task CreateTransactionThrowsOnApiErrorAsync()
    {
        await using TestServer server = await TestServer.StartAsync(context => RespondJsonAsync(context, 400, """{"message":"Invalid payload"}"""));

        using DamanesignClient client = DamanesignClient.Create(server.Url, "test-token");
        try
        {
            await client.CreateTransactionAsync(new CreateTransactionRequest { Name = "Invalid" });
            throw new Exception("Expected DamanesignException.");
        }
        catch (DamanesignException exception)
        {
            AssertEqual(400, exception.StatusCode);
            AssertEqual("""{"message":"Invalid payload"}""", exception.ResponseBody);
        }
    }

    private static async Task FiltersRepeatArrayQueryParametersAsync()
    {
        string? path = null;

        await using TestServer server = await TestServer.StartAsync(context =>
        {
            path = context.Request.RawUrl;
            return RespondJsonAsync(context, 200, "[]");
        });

        using DamanesignClient client = DamanesignClient.Create(server.Url, "test-token");
        await client.ListTransactionsAsync(TransactionFilter.Create()
            .Status(["draft", "active"])
            .Type(["simple"])
            .Limit(20));

        AssertEqual("/transactions?status=draft&status=active&type=simple&limit=20", path);
    }

    private static async Task RespondJsonAsync(HttpListenerContext context, int status, string body)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(body);
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";
        context.Response.ContentLength64 = bytes.Length;
        await context.Response.OutputStream.WriteAsync(bytes);
        context.Response.Close();
    }

    private static void AssertEqual<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new Exception($"Expected '{expected}', got '{actual}'.");
        }
    }

    private static void AssertTrue(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }
}

internal sealed class TestServer : IAsyncDisposable
{
    private readonly HttpListener _listener = new();
    private readonly Func<HttpListenerContext, Task> _handler;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Task _listenTask;

    private TestServer(string url, Func<HttpListenerContext, Task> handler)
    {
        Url = url;
        _handler = handler;
        _listener.Prefixes.Add(url);
        _listener.Start();
        _listenTask = ListenAsync();
    }

    public string Url { get; }

    public static Task<TestServer> StartAsync(Func<HttpListenerContext, Task> handler)
    {
        int port = Random.Shared.Next(20000, 50000);
        return Task.FromResult(new TestServer($"http://127.0.0.1:{port}/", handler));
    }

    public async ValueTask DisposeAsync()
    {
        _cancellationTokenSource.Cancel();
        _listener.Stop();
        _listener.Close();
        try
        {
            await _listenTask.ConfigureAwait(false);
        }
        catch
        {
            // Listener shutdown throws while tests dispose the server.
        }
    }

    private async Task ListenAsync()
    {
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            HttpListenerContext context = await _listener.GetContextAsync().ConfigureAwait(false);
            _ = Task.Run(async () =>
            {
                try
                {
                    await _handler(context).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(ex.ToString());
                    context.Response.StatusCode = 500;
                    context.Response.ContentLength64 = bytes.Length;
                    await context.Response.OutputStream.WriteAsync(bytes).ConfigureAwait(false);
                    context.Response.Close();
                }
            });
        }
    }
}
