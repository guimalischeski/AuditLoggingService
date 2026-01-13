using Prometheus;

namespace ALS.Core.Observability
{
    public sealed class AuditMetrics
    {
        public static readonly CollectorRegistry Registry = new();
        public static readonly MetricFactory Factory = Metrics.WithCustomRegistry(Registry);

        public static AuditMetrics Instance { get; } = new();

        public Counter EventsIngested { get; }
        public Counter QueueConsumerErrors { get; }
        public Histogram ProcessingTimeSeconds { get; }

        private AuditMetrics()
        {
            EventsIngested = Factory.CreateCounter(
                Constants.Constants.AuditMetrics.IngestedTotalKey,
                Constants.Constants.AuditMetrics.IngestedTotalDescription);

            QueueConsumerErrors = Factory.CreateCounter(
                Constants.Constants.AuditMetrics.ConsumerErrorTotalKey,
                Constants.Constants.AuditMetrics.ConsumerErrorTotalDescription);

            ProcessingTimeSeconds = Factory.CreateHistogram(
                Constants.Constants.AuditMetrics.ProcessingTimeInSecondsKey,
                Constants.Constants.AuditMetrics.ProcessingTimeInSecondsDescription,
                new HistogramConfiguration
                {
                    Buckets = Histogram.ExponentialBuckets(start: 0.005, factor: 2, count: 10)
                });
        }
    }
}
