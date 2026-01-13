namespace ALS.Core.Contracts
{
    public sealed record PagedResult<T>(
        IReadOnlyList<T> Items,
        int Page,
        int PageSize,
        long Total
    );
}
