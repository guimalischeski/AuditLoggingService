using ALS.Core.Domain;
using ALS.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ALS.Infrastructure.Persistence
{
    public sealed class AuditRepository : IAuditRepository
    {
        private readonly AuditDbContext _db;

        public AuditRepository(AuditDbContext db) => _db = db;

        public async Task<long> AddAsync(AuditEvent ev, CancellationToken ct)
        {
            var entry = _db.AuditEvents.Add(ev);
            await _db.SaveChangesAsync(ct);

            return entry.Entity.Id;
        }

        public IQueryable<AuditEvent> Query() => _db.AuditEvents.AsNoTracking();
    }
}
