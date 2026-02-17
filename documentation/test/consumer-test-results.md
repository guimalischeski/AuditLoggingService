# AuditLoggingService – Basic Performance Check Report
## Test Objective
- Perform a basic performance validation of the AuditLoggingService under moderate load:
    - Simulate ingestion of ```5,000``` audit events;
    - Monitor consumer performance;

- Collect:
    - CPU usage before, during and after load;
    - Memory usage before, during and after load;
    - Identify one inefficiency;
    - Apply one improvement;

## Test Environment
- Tool: ```.NET Console App / Prometheus```
- Load Type: ```Consumer ingestion```
- Iterations: ```5,000 POST requests```
- Database: ```SQL Server```
- Machine: ```Windows environment / docker containers```

### PromQL Queries
#### Average Processing Time (ms / message)
```promql
rate(sqsauditconsumer_processing_time_milliseconds_sum[5m])
/
rate(sqsauditconsumer_processing_time_milliseconds_count[5m])
```

#### Processing throughput (messages/sec)
```promql
rate(sqsauditconsumer_processing_time_milliseconds_count[5m])
```

#### Ingested throughput (events/sec)
```promql
rate(sqsauditconsumer_ingested_events_total[1m])
```

#### p95 processing time (ms)
```promql
histogram_quantile(
  0.95,
  sum(rate(sqsauditconsumer_processing_time_milliseconds_bucket[5m])) by (le)
)
```

## Ingestion Test Results (Localstack SQS queue)
- Command executed: 
    ```bash
    cd .\SqsLoadProducer
    dotnet run
    ```

### Results
- Average Processing Time: ```69.07 ms```
- Processing Throughput: ```1.47 msg/s```
- Ingested Throughput: ```71.26 events/s```
- p95 Processing Time: ```73.76 ms```

## System Resource Usage
- Tool: Windows Task Manager

| Metric    | Before Test   | During Ingest | After Test    |
| ---       | ---           | ---           | ---           |
| CPU Usage | 0%            | 1.0%          | 0%            |
| Memory    | 14.8 MB       | 40.8 MB       | 39.3          |

---

## Identified Inefficiency
During ingestion, the metadata field received from the API was first deserialized by ASP.NET Core into an object and then re-serialized back into JSON before being stored in the database. This introduced an unnecessary JSON serialization step on every ingested event.

For high-volume ingestion scenarios (e.g., thousands of events), this repeated serialization added avoidable CPU usage, memory allocations, and garbage collection pressure.

### Why This Is Inefficient
Each ingestion request was performing:
- JSON parsing (HTTP request → DTO)
- JSON re-serialization (JsonSerializer.Serialize(metadata))
- String allocation for storage

This double-handling of the same JSON payload created unnecessary overhead per event. Under burst load (5,000 events), this overhead accumulates and directly impacts:
- CPU usage
- Memory allocations
- Garbage collection frequency
- Maximum latency spikes
- Overall ingestion throughput

## Applied Improvement
The ingestion path was optimized by accepting the metadata field directly as raw JSON (JsonElement) in the DTO and storing it using ```GetRawText()``` without re-serializing.

### System Resource Usage after improvement
- Tool: Windows Task Manager

| Metric    | Before Test   | During Ingest | After Test    |
| ---       | ---           | ---           | ---           |
| CPU Usage | 0%            | 2.1%          | 0%            |
| Memory    | 13.2 MB       | 40.3 MB       | 39.5          |

---

## Conclusion

### Consumer - Before Improvements
- Average Processing Time: ```69.07 ms```
- Processing Throughput: ```1.47 msg/s```
- Ingested Throughput: ```71.26 events/s```
- p95 Processing Time: ```73.76 ms```

### Consumer - After Improvements
- Average Processing Time: ```61.89 ms```
- Processing Throughput: ```1.69 msg/s```
- Ingested Throughput: ```83.95 events/s```
- p95 Processing Time: ```72.81 ms```

---

## Improvement Changes - Summary
1. Removed Metadata serialization: Parsed incoming object from request as a ```JsonElement``` and removed Json Serialization unecessary step for each request;