using System.Text.Json;

namespace ALS.Core.Contracts
{
    public sealed class AuditEventDto
    {
        public DateTimeOffset Timestamp { get; init; }
        public string UserId { get; init; } = default!;
        public string ActionType { get; init; } = default!;
        public string EntityId { get; init; } = default!;
        public JsonElement? Metadata { get; init; }
    }
}
