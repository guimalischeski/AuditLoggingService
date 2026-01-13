using ALS.Core.Enum;

namespace ALS.Core.Domain
{
    public sealed class AuditEvent
    {
        public long Id { get; set; }

        public DateTimeOffset Timestamp { get; set; }
        public string UserId { get; set; } = default!;
        public string ActionType { get; set; } = default!;
        public string EntityId { get; set; } = default!;

        public string MetadataJson { get; set; } = "{}";

        public AuditIngestSource IngestSource { get; set; }
        public string TraceId { get; set; } = default!;
    }
}
