using ALS.Core.Constants;
using ALS.Core.Contracts;
using ALS.Core.Domain;
using ALS.Core.Extensions;
using ALS.Core.Interfaces;
using ALS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ALS.Infrastructure.Services
{
    public sealed class AuditQueryService : IAuditQueryService
    {
        private readonly AuditDbContext _db;
        private readonly ILogger<AuditQueryService> _logger;

        public AuditQueryService(ILogger<AuditQueryService> logger, AuditDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<PagedResult<AuditEvent>> SearchAsync(AuditSearchRequest req, CancellationToken ct)
        {
            try
            {
                req = req.Normalize();

                IQueryable<AuditEvent> q = _db.AuditEvents.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(req.UserId))
                    q = q.Where(x => x.UserId == req.UserId);

                if (req.From is not null)
                    q = q.Where(x => x.Timestamp >= req.From.Value);

                if (req.To is not null)
                    q = q.Where(x => x.Timestamp <= req.To.Value);

                if (!string.IsNullOrWhiteSpace(req.ActionType))
                    q = q.Where(x => x.ActionType == req.ActionType);

                q = req.SortBy switch
                {
                    Constants.SortOptions.TimestampAsc => q.OrderBy(x => x.Timestamp),
                    _ => q.OrderByDescending(x => x.Timestamp)
                };

                var total = await q.LongCountAsync(ct);

                var items = await q
                    .Skip((req.Page - 1) * req.PageSize)
                    .Take(req.PageSize)
                    .ToListAsync(ct);

                return new PagedResult<AuditEvent>(items, req.Page, req.PageSize, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(Constants.ErrorMessages.SearchAsyncQueryError, ex.Message);
                throw;
            }
        }
    }
}
