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

            //Worker
            public const string WorkerDelayMillisecondsNotConfigured = "Worker:DelayMilliseconds is not configured.";
        }

        public static class LogMessages
        {
            //Event
            public const string IngestingAuditEvent = "Ingesting audit event. source={Source} userId={UserId} actionType={ActionType} entityId={EntityId} traceId={TraceId}.";
            public const string IngestedAuditEventWithId = "Ingested AuditEvent with id: {eventId}";
            
            //SQS
            public const string SqsReachable = "SQS reachable.";
            public const string SqsUnreachable = "SQS unreachable.";

            //Worker
            public const string WorkerRunningAt = "Worker running at: {time}";
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

            //Worker
            public const string Worker = "Worker";
            public const string DelayMilliseconds = "DelayMilliseconds";
        }

        public static class SortOptions
        {
            public const string TimestampAsc = "timestamp";
            public const string TimestampDesc = "timestamp_desc";
        }

        public static class AuditMetrics
        {
            public const string IngestedTotalKey = "als_events_ingested_total";
            public const string IngestedTotalDescription = "Total ingested events count.";
            public const string ConsumerErrorTotalKey = "als_queue_consumer_errors_total";
            public const string ConsumerErrorTotalDescription = "Consumer error count.";
            public const string ProcessingTimeInSecondsKey = "als_processing_time_seconds";
            public const string ProcessingTimeInSecondsDescription = "Processing time in seconds.";
        }
    }
}
