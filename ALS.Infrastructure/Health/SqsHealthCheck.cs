using ALS.Core.Constants;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ALS.Infrastructure.Health
{
    public sealed class SqsHealthCheck : IHealthCheck
    {
        private readonly IAmazonSQS _sqs;
        private readonly IConfiguration _cfg;

        private const string DefaultQueueName = "als-audit-events";

        public SqsHealthCheck(IAmazonSQS sqs, IConfiguration cfg)
        {
            _sqs = sqs;
            _cfg = cfg;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken ct = default)
        {
            try
            {
                var queueName = _cfg[$"{Constants.ConfigurationKeys.Aws}:{Constants.ConfigurationKeys.QueueName}"] ?? DefaultQueueName;
                var url = (await _sqs.GetQueueUrlAsync(queueName, ct)).QueueUrl;

                await _sqs.GetQueueAttributesAsync(new GetQueueAttributesRequest
                {
                    QueueUrl = url,
                    AttributeNames = new List<string>
                {
                    QueueAttributeName.ApproximateNumberOfMessages
                }
                }, ct);

                return HealthCheckResult.Healthy(Constants.LogMessages.SQSReachable);
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(Constants.LogMessages.SQSUnreachable, ex);
            }
        }
    }
}
