namespace Damanesign.Sdk;

public sealed class DamanesignException : Exception
{
    public DamanesignException(string message)
        : base(message)
    {
    }

    public DamanesignException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public DamanesignException(string message, int statusCode, string? responseBody)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    public int StatusCode { get; } = -1;

    public string? ResponseBody { get; }
}
