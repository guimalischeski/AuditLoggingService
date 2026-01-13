using ALS.Core.Contracts;
using ALS.Core.Domain;

namespace ALS.Core.Interfaces
{
    public interface IAuditQueryService
    {
        Task<PagedResult<AuditEvent>> SearchAsync(AuditSearchRequest req, CancellationToken ct);
    }
}
