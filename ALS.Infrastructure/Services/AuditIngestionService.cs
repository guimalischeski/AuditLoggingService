using System.Text.Json;
using ALS.Core.Constants;
using ALS.Core.Contracts;
using ALS.Core.Domain;
using ALS.Core.Enum;
using ALS.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ALS.Infrastructure.Services
{
    public sealed class AuditIngestionService : IAuditIngestionService
    {
        private readonly IAuditRepository _repo;
        private readonly ILogger<AuditIngestionService> _logger;

        public AuditIngestionService(IAuditRepository repo, ILogger<AuditIngestionService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<long> IngestAsync(AuditEventDto dto, AuditIngestSource source, string traceId, CancellationToken ct)
        {
            try
            {
                if (dto.Timestamp == default)
                    throw new ArgumentException(Constants.ErrorMessages.TimestampIsRequired, nameof(dto.Timestamp));
                if (string.IsNullOrWhiteSpace(dto.UserId))
                    throw new ArgumentException(Constants.ErrorMessages.UserIdIsRequired, nameof(dto.UserId));
                if (string.IsNullOrWhiteSpace(dto.ActionType))
                    throw new ArgumentException(Constants.ErrorMessages.ActionTypeIsRequired, nameof(dto.ActionType));
                if (string.IsNullOrWhiteSpace(dto.EntityId))
                    throw new ArgumentException(Constants.ErrorMessages.EntityIdIsRequired, nameof(dto.EntityId));

                var ev = new AuditEvent
                {
                    Timestamp = dto.Timestamp,
                    UserId = dto.UserId,
                    ActionType = dto.ActionType,
                    EntityId = dto.EntityId,
                    MetadataJson = dto.Metadata is null ? "{}" : JsonSerializer.Serialize(dto.Metadata),
                    IngestSource = source,
                    TraceId = traceId
                };

                _logger.LogInformation(Constants.LogMessages.IngestingAuditEvent, source, ev.UserId, ev.ActionType, ev.EntityId, traceId);

                var entityId = await _repo.AddAsync(ev, ct);
                return entityId;
            }
            catch (Exception ex)
            {
                _logger.LogError(Constants.ErrorMessages.IngestAsyncErrorWithMessage, ex.Message);
                throw;
            }
        }
    }
}
