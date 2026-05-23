namespace Damanesign.Sdk.Models;

public sealed class TransactionFilter
{
    private readonly Dictionary<string, object?> _parameters = [];

    public static TransactionFilter Create() => new();

    public TransactionFilter Type(IEnumerable<string> type) => Parameter("type", type);
    public TransactionFilter Status(IEnumerable<string> status) => Parameter("status", status);
    public TransactionFilter Tags(IEnumerable<string> tags) => Parameter("tags", tags);
    public TransactionFilter Offset(int offset) => Parameter("offset", offset);
    public TransactionFilter Limit(int limit) => Parameter("limit", limit);
    public TransactionFilter Name(string name) => Parameter("name", name);
    public TransactionFilter MemberFirstname(string firstname) => Parameter("members.firstname", firstname);
    public TransactionFilter MemberLastname(string lastname) => Parameter("members.lastname", lastname);
    public TransactionFilter CreatorId(string creatorId) => Parameter("creatorId", creatorId);
    public TransactionFilter WorkspaceIds(IEnumerable<string> workspaceIds) => Parameter("workspaceIds", workspaceIds);
    public TransactionFilter CreatedAt(IEnumerable<DateOnly> createdAt) => Parameter("createdAt", createdAt);
    public TransactionFilter ExpiresAt(IEnumerable<DateOnly> expiresAt) => Parameter("expiresAt", expiresAt);
    public TransactionFilter Order(string order) => Parameter("order", order);
    public TransactionFilter Xlsx(bool xlsx) => Parameter("xlsx", xlsx);

    public TransactionFilter Parameter(string name, object? value)
    {
        _parameters[name] = value;
        return this;
    }

    public IReadOnlyDictionary<string, object?> ToQueryParameters() => new Dictionary<string, object?>(_parameters);
}
