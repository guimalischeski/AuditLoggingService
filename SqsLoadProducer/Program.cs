using Amazon.SQS;
using Amazon.SQS.Model;
using System.Diagnostics;
using System.Text.Json;

namespace SqsLoadProducer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var queueUrl = "http://localhost:4566/000000000000/als-audit-events";

            var config = new AmazonSQSConfig
            {
                ServiceURL = "http://localhost:4566",
                UseHttp = true
            };

            var client = new AmazonSQSClient("test", "test", config);

            int total = 5000;
            int concurrency = 20;

            var semaphore = new SemaphoreSlim(concurrency);
            var tasks = new List<Task>();

            var sw = Stopwatch.StartNew();

            for (int i = 0; i < total; i++)
            {
                await semaphore.WaitAsync();

                int index = i;

                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var payload = new
                        {
                            timestamp = DateTime.UtcNow,
                            userId = $"user-{index % 50}",
                            actionType = $"action-{index % 10}",
                            entityId = $"entity-{index}",
                            metadata = new { foo = "bar", n = index }
                        };

                        var messageBody = JsonSerializer.Serialize(payload);

                        await client.SendMessageAsync(new SendMessageRequest
                        {
                            QueueUrl = queueUrl,
                            MessageBody = messageBody
                        });
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
            }

            await Task.WhenAll(tasks);

            sw.Stop();

            Console.WriteLine($"Sent {total} messages in {sw.Elapsed.TotalSeconds:F2}s");
            Console.WriteLine($"Throughput: {total / sw.Elapsed.TotalSeconds:F2} msg/sec");
        }
    }
}
