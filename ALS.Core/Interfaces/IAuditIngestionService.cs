using ALS.Core.Contracts;
using ALS.Core.Enum;

namespace ALS.Core.Interfaces
{
    public interface IAuditIngestionService
    {
        Task<long> IngestAsync(AuditEventDto dto, AuditIngestSource source, string traceId, CancellationToken ct);
    }
}
