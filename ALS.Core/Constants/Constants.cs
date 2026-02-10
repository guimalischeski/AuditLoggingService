namespace ALS.Core.Constants
{
    public static class Constants
    {
        public static class ErrorMessages
        {
            //API
            public const string TimestampIsRequired = "Timestamp parameter is required.";
            public const string UserIdIsRequired = "UserId parameter is required.";
            public const string ActionTypeIsRequired = "ActionType parameter is required.";
            public const string EntityIdIsRequired = "EntityId parameter is required.";

            //SQS
            public const string FailedProcessingSqsMessage = "Failed processing SQS message.";
            public const string QueueUrlNotSet = "QueueUrl not set.";
            public const string InvalidPayload = "Invalid payload.";
            public const string SqsReceiveLoopError = "SQS receive loop error.";

            //QueryService
            public const string SearchAsyncQueryError = "Error while querying database. Exception message: {message}";

            //IngestionService
            public const string IngestAsyncErrorWithMessage = "Error while ingesting audit to database. Exception message: {message}";

            //API
            public const string GetError = "Error while querying database.";
            public const string GetErrorWithMessage = "Error while querying database. Exception message: {message}.";
            public const string PostError = "Error while ingesting audit instance.";
            public const string PostErrorWithMessage = "Error while ingesting audit instance. Exception message: {message}.";
        }

        public static class LogMessages
        {
            //Event
            public const string IngestingAuditEvent = "Ingesting audit event. source={Source} userId={UserId} actionType={ActionType} entityId={EntityId} traceId={TraceId}.";
            public const string IngestedAuditEventWithId = "Ingested AuditEvent with id: {eventId}";
            
            //SQS
            public const string SqsReachable = "SQS reachable.";
            public const string SqsUnreachable = "SQS unreachable.";
        }

        public static class ConfigurationKeys
        {
            //DB
            public const string AuditDb = "AuditDb";
            
            //SQS
            public const string Aws = "Aws";
            public const string ServiceUrl = "ServiceUrl";
            public const string Region = "Region";
            public const string QueueName = "QueueName";
        }

        public static class SortOptions
        {
            public const string TimestampAsc = "timestamp";
            public const string TimestampDesc = "timestamp_desc";
        }
    }
}
