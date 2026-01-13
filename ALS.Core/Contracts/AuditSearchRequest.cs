namespace ALS.Core.Contracts
{
    public sealed record AuditSearchRequest(
        string? UserId,
        DateTimeOffset? From,
        DateTimeOffset? To,
        string? ActionType,
        string SortBy = Constants.Constants.SortOptions.TimestampAsc,
        int Page = 1,
        int PageSize = 50
    );
}
