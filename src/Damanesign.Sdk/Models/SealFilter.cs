namespace Damanesign.Sdk.Models;

public sealed class SealFilter
{
    private readonly Dictionary<string, object?> _parameters = [];

    public static SealFilter Create() => new();

    public SealFilter Name(string name) => Parameter("name", name);
    public SealFilter CertificateId(string certificateId) => Parameter("certificateId", certificateId);
    public SealFilter Offset(int offset) => Parameter("offset", offset);
    public SealFilter Limit(int limit) => Parameter("limit", limit);

    public SealFilter Parameter(string name, object? value)
    {
        _parameters[name] = value;
        return this;
    }

    public IReadOnlyDictionary<string, object?> ToQueryParameters() => new Dictionary<string, object?>(_parameters);
}
