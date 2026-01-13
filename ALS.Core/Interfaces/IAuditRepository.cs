using ALS.Core.Domain;

namespace ALS.Core.Interfaces
{
    public interface IAuditRepository
    {
        Task<long> AddAsync(AuditEvent ev, CancellationToken ct);
        IQueryable<AuditEvent> Query();
    }
}
