# AuditLoggingService – Basic Performance Check Report
## Test Objective
- Perform a basic performance validation of the AuditLoggingService under moderate load:
    - Simulate ingestion of ```5,000``` audit events;
    - Execute ```20``` GET requests filtering by userId and actionType;

- Collect:
    - Average GET response time;
    - API throughput;
    - CPU usage during load;
    - Memory usage before and after test;
    - Identify one inefficiency;
    - Apply one small improvement;

## Test Environment
- Tool: ```k6```
- Load Type: ```REST API ingestion```
- Concurrency: ```20 VUs```
- Iterations: ```5,000 POST requests```
- Database: ```SQL Server```
- Machine: ```Windows environment / docker containers```

## Ingestion Test Results (POST /api/audit)
- Command executed: 
    ```bash
    k6 run ingest.js
    ```
- Results
    - Total Events: ```5,000```
    - Total Duration: ```~2.8 seconds```
    - Throughput: ```~1,800 events/sec```
    - Average POST latency: ```10.97 ms```
    - p95 latency: ```19.58 ms```
    - Maximum latency: ```338.07 ms```
    - Failed Requests: ```0%```

### Metrics conclusions
- Throughput is strong for moderate load;
- Latency remained under ```20ms``` for ```95%``` of requests;
- One outlier spike ```~338ms``` observed under burst load;

## Query Test Results (GET /api/audit)
- Command executed:
    ```bash
    k6 run query.js
    ```
- Filter used:
    - ```userId=user-1```
    - ```actionType=action-1```
    - ```page=1```
    - ```pageSize=50```

### Run #1
- Average GET latency: ```4.23 ms```
- p95 latency: ```5.47 ms```
- Maximum latency: ```8.51 ms```
- Failed Requests: ```0%```

## System Resource Usage
- Tool: Windows Task Manager

| Metric    | Before Test   | During Ingest | During Query  | After Test    |
| ---       | ---           | ---           | ---           | ---           |
| CPU Usage | 0%            | 1.3%          | 2.7%          | 0%            |
| Memory    | 22.8 MB       | 95.2 MB       | 86.3 MB       | 80.5          |

---

## Identified Inefficiency
The GET search endpoint typically filters by:
- ```UserId```
- ```ActionType```

and sorts by:
- ```Timestamp (descending)```

Without a matching composite index, SQL Server may not be able to perform an efficient seek for this filter pattern and may require additional work (extra scans/lookups/sort cost) as the table grows.

### Why This Is Inefficient
The query filters on two columns and sorts by timestamp, but the database indexes are not aligned with the combined filter + sort shape. This becomes increasingly expensive as the audit table grows, especially under concurrent GET load and repeated queries.

## Applied Improvement
Added a composite index to match the common query pattern:
```
(UserId, ActionType, Timestamp)
```

This index supports:
- filtering by ```UserId + ActionType```;
- retrieving results already ordered by ```Timestamp```;
- faster paging on ```timestamp``` order;

---

### Run #1 after improvement
- Average GET latency: ``` 3.45 ms```
- p95 latency: ``` 6.62 ms```
- Maximum latency: ``` 7.69 ms```
- Failed Requests: ```0%```

### System Resource Usage after improvement
- Tool: Windows Task Manager

| Metric    | Before Test   | During Query  | After Test    |
| ---       | ---           | ---           | ---           |
| CPU Usage | 0%            | 1.8%          | 0%            |
| Memory    | 14.9 MB       | 38.7 MB       | 30.9          |

---

## Conclusion

### POST Endpoint - Before Improvements
- Throughput: ```~1,800 events/sec```
- Average POST latency: ```10.97 ms```
- p95 latency: ```19.58 ms```
- Maximum latency: ```338.07 ms```
- Failed requests: ```0%```

### POST Endpoint - After Improvements
- Throughput: ```~2,846 events/sec```
- Average POST latency: ```6.87 ms```
- p95 latency: ```12.19 ms```
- Maximum latency: ```36.75 ms```
- Failed requests: ```0%```

### GET Endpoint – Before Improvements
- Average latency: ```4.23 ms```
- p95 latency: ```5.47 ms```
- Max latency: ```8.51 ms```
- Failed requests: ```0%```

### GET Endpoint – After Improvements
- Average latency: ```3.45 ms```
- p95 latency: ```6.62 ms```
- Max latency: ```7.69 ms```
- Failed requests: ```0%```

---

## Improvement Changes - Summary

1. New DB Index: ```(UserId, ActionType, Timestamp)```;
2. Optimized DB Query: Executed ```LongCountAsync()``` before adding sorting to the query;
3. Removed Metadata serialization: Parsed incoming object from request as a ```JsonElement``` and removed Json Serialization unecessary step for each request;