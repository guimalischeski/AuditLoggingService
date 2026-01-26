# Architecture Trade-offs & Risk Assessment
Describes the main architectural decisions taken in the Audit Logging Service solution, the trade-offs involved, and the associated risks.

## Trade-off Analysis

### SQL vs NoSQL
Used ```SQL Server``` instead of a ```NoSQL``` database.

### Why SQL
Relational databases are more in accordance with the following requirements:
- Strong consistency;
- Complex querying;
    - ```Time-range``` queries;
    - ```User-based``` filtering;
    - ```Action-based``` filtering;
    - ```Sorting```;
    - ```Pagination```;
- Clear schema; 
    - Helps enforce data integrity;

### Why not NoSQL
- Usually NoSQL is good for the following topics:
    - Large scaling;
    - Simple key-value or document retrieval;

- While the audit logs solution required:
    - Filtering;
    - Index-based queries;
    - Ordering;

### Conclusion
```SQL``` fits the read/query requirements of an audit system better than ```NoSQL```.

---

## Queue System Choice (AWS SQS)
Used ```AWS SQS (LocalStack)```.

### Why SQS
From the solution requirements, ```SQS with LocalStack``` presented a free and easier implementation approach, while ```Azure Service Bus``` would require a real subcription and hosting to properly work.

---

## Design Patterns Used
1. Repository Pattern
    - Abstracts database access;
    - Enables easier testing implementations;
    - Enables easier future DB changes;

2. Service Layer
    - Encapsulates business logic:
        - Ingestion rules;
        - Validation;
        - Source mapping;
    - Keeps controllers thin and focused on HTTP concerns;

3. Background Worker (Consumer)
    - Separates async processing from synchronous API calls;
    - Improves resilience and throughput;

4. Dependency Injection
    - Standard .NET practice;
    - Enables loose coupling and testability;

---

## Complexity vs Maintainability
The solution adds architectural structures (services, repository, worker...) to keep the system maintainable as it grows.

### Complexity
1. Multiple projects
    - Complexity cost: 
        - more folders;
        - more ```Dependency Injection```;
    - Maintainability gain:
        - API remains ```HTTP-only```;
        - Consumer remains ```queue-only```;
        - Core is reusable;
        - Infrastructure isolates ```persistence``` and ```AWS SDK``` details;
        - Prevents high coupling and makes future changes safer;

2. Service layer
    - Complexity cost: 
        - more complex when compared to calling repository directly from controller;
    - Maintainability gain:
        - Reusable ingestion logic:
            - HTTP ingestion;
            - Queue ingestion;
        - Removes logic from controllers;
            - Avoids duplicate code for validation, mappings, etc;

3. Repository abstraction
    - Complexity cost:
        - extra interfaces and classes;
    - Maintainability gain: 
        - Centralized persistence logic;
        - Easier test implementation through mocking;
        - Isolated database changes;

4. Observability
    - Complexity cost: 
        - more middleware;
    - Maintainability gain:
        - reduces debugging time;
        - failures can be tracked down;
        - early visibility into performance information;

### Maintainability
- Easy extensions
    - Add a new ingestion source (e.g., another queue / file input) by calling the ingestion service;
    - Add fields to audit event (migration + model update);
    - Replace ```LocalStack``` with real ```AWS``` (config only);

- Risk areas
    - Query endpoint growth
        - Adding new optional filters without indexes might degrade performance;
    - JSON metadata querying
        - Querying inside MetadataJson impacts SQL performance;
    - Retry strategy
        - Bad retry strategy can overload DB/queue;

---

## Risk Assessment
### Potential Bottlenecks
- Database
    - Heavy write load during spikes;
    - Index maintenance cost;
    - Possible mitigation:
        - Batch inserts;
        - Partitioning by time;

- Queue Consumer
    - Single consumer instance could be slow during spikes;
    - Possible mitigation:
        - Horizontal scaling;

- Serialization and Deserialization
    - JSON parsing errors;
    - Possible mitigation:
        - Strict schema;
        - Validation at ingestion;

---

### Failure Modes
- API Failures
    - DB down -> API ingestion fails;
    - Possible mitigation:
        - Health checks;

- Queue Failures
    - Message processing crashes;
    - Possible mitigation:
        - Message not deleted -> auto retry;

- Poison Messages
    - Malformed payloads;
    - Possible mitigation:
        - Validation;
        - Dead-letter queue;

---

### Retry & Backoff Strategy
- Queue Processing
    - At-least-once delivery model;
    - Failures result in:
        - Message becoming visible again;
    - Strategy:
        - Limit max retries;
        - Log and increment error metrics;

- Database Operations
    - Avoid infinite retries;
    - Fail fast + logging for persistent DB failures;