using ALS.Core.Constants;
using ALS.Core.Contracts;
using ALS.Core.Enum;
using ALS.Core.Interfaces;
using ALS.Core.Observability;
using Amazon.SQS;
using Amazon.SQS.Model;
using Prometheus;
using System.Text.Json;

namespace ALS.Consumer
{
    public sealed class SqsAuditConsumer : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly IAmazonSQS _sqs;
        private readonly IConfiguration _cfg;
        private readonly ILogger<SqsAuditConsumer> _logger;
        private readonly AuditMetrics _metrics = AuditMetrics.Instance;

        private string? _queueUrl;

        private const string TraceIdKey = "traceId";
        private const string MessageIdKey = "messageId";

        public SqsAuditConsumer(IServiceProvider sp, IAmazonSQS sqs, IConfiguration cfg, ILogger<SqsAuditConsumer> logger)
        {
            _sp = sp;
            _sqs = sqs;
            _cfg = cfg;
            _logger = logger;
        }

        public override async Task StartAsync(CancellationToken ct)
        {
            var queueName = _cfg[$"{Constants.ConfigurationKeys.Aws}:{Constants.ConfigurationKeys.QueueName}"]!;
            _queueUrl = (await _sqs.GetQueueUrlAsync(queueName, ct)).QueueUrl;

            await base.StartAsync(ct);
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            if (_queueUrl is null) throw new InvalidOperationException(Constants.ErrorMessages.QueueUrlNotSet);

            while (!ct.IsCancellationRequested)
            {
                using var timer = _metrics.ProcessingTimeSeconds.NewTimer();

                try
                {
                    var resp = await _sqs.ReceiveMessageAsync(new ReceiveMessageRequest
                    {
                        QueueUrl = _queueUrl,
                        MaxNumberOfMessages = 10,
                        WaitTimeSeconds = 20,
                        VisibilityTimeout = 30
                    }, ct);

                    if (resp.Messages == null || resp.Messages.Count == 0)
                        continue;

                    foreach (var m in resp.Messages)
                    {
                        try
                        {
                            var dto = JsonSerializer.Deserialize<AuditEventDto>(m.Body,
                                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                            if (dto is null) throw new InvalidOperationException(Constants.ErrorMessages.InvalidPayload);

                            var traceId = m.MessageId;

                            using var scope = _logger.BeginScope(new Dictionary<string, object?>
                            {
                                [TraceIdKey] = traceId,
                                [MessageIdKey] = m.MessageId
                            });

                            using var diScope = _sp.CreateScope();
                            var ingestion = diScope.ServiceProvider.GetRequiredService<IAuditIngestionService>();
                            var eventId = await ingestion.IngestAsync(dto, AuditIngestSource.Sqs, m.MessageId, ct);
                            
                            _metrics.EventsIngested.Inc();
                            _logger.LogInformation(Constants.LogMessages.IngestedAuditEventWithId, eventId);

                            await _sqs.DeleteMessageAsync(_queueUrl, m.ReceiptHandle, ct);
                        }
                        catch (Exception ex)
                        {
                            _metrics.QueueConsumerErrors.Inc();
                            _logger.LogError(ex, Constants.ErrorMessages.FailedProcessingSqsMessage);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _metrics.QueueConsumerErrors.Inc();
                    _logger.LogError(ex, Constants.ErrorMessages.SqsReceiveLoopError);
                    await Task.Delay(TimeSpan.FromSeconds(2), ct);
                }
            }
        }
    }
}
