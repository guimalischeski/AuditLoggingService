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
        private readonly ILogger<AuditQueryService> _logger;
        private readonly IAuditRepository _auditRepository;

        public AuditQueryService(ILogger<AuditQueryService> logger, IAuditRepository auditRepository)
        {
            _logger = logger;
            _auditRepository = auditRepository;
        }

        public async Task<PagedResult<AuditEvent>> SearchAsync(AuditSearchRequest req, CancellationToken ct)
        {
            try
            {
                req = req.Normalize();

                var query = _auditRepository.Query();

                if (!string.IsNullOrWhiteSpace(req.UserId))
                    query = query.Where(x => x.UserId == req.UserId);

                if (req.From is not null)
                    query = query.Where(x => x.Timestamp >= req.From.Value);

                if (req.To is not null)
                    query = query.Where(x => x.Timestamp <= req.To.Value);

                if (!string.IsNullOrWhiteSpace(req.ActionType))
                    query = query.Where(x => x.ActionType == req.ActionType);

                var total = await query.LongCountAsync(ct);

                query = req.SortBy switch
                {
                    Constants.SortOptions.TimestampAsc => query.OrderBy(x => x.Timestamp),
                    _ => query.OrderByDescending(x => x.Timestamp)
                };

                var items = await query
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
