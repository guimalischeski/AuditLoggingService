# Audit Logging Service (ALS)

Audit Logging Service (ALS) is a .NET-based backend system designed to ingest, persist, and query audit events from multiple sources (HTTP and SQS queue), with strong observability and local development support.

## Architecture Overview
### AuditLoggingService
- ALS.Api
- ALS.Consumer
- ALS.Core
- ALS.Infrastructure

### Ingestion paths

1. HTTP API (POST /api/audit)
2. AWS SQS (LocalStack)

### Technologies Used

- .NET 8
- Entity Framework Core
- SQL Server
- AWS SQS
- Prometheus (metrics)
- ASP.NET Health Checks
- Docker / Docker Compose

---

## Local Development Guide
### Prerequisites
- Docker Desktop
- .NET 8 SDK

### Start everything locally
1. Execute 
    ```bash
    .\scripts\run-local.ps1
    ```
    Script will:
    - Start SQL Server container (or reuse existing)
    - Start LocalStack (SQS)
    - Start Prometheus
    - Wait for SQL Server to be ready
    - Apply EF migrations
    - Start ALS.Api
    - Start ALS.Consumer

### Environment config file example
```env
SQL_SA_PASSWORD={{DB_PASSWORD}}
Database__Provider=SqlServer
ConnectionStrings__AuditDb=Server=localhost,{{DB_PORT}};Database={{DB_NAME}};User Id={{DB_USER}};Password={{DB_PASSWORD}};TrustServerCertificate=True

Aws__Region={{AWS_REGION}}
Aws__ServiceUrl={{AWS_SERVICE_URL}}
Aws__QueueName={{AWS_QUEUE_NAME}}

AWS_ACCESS_KEY_ID={{AWS_KEY_ID}}
AWS_SECRET_ACCESS_KEY={{AWS_SECRET_KEY}}
AWS_DEFAULT_REGION={{AWS_DEFAULT_REGION}}
```

## Available Endpoints
1. ```POST``` ```/api/audit``` – Ingest audit event
2. ```GET``` ```/api/audit``` – Query audit events
3. ```GET``` ```/health``` – Health checks
4. ```GET``` ```/metrics``` – Full Prometheus metrics
5. ```GET``` ```/metrics/als``` – ALS-only metrics

### Metrics exposed
- sqsauditconsumer_ingested_events
- sqsauditconsumer_consumer_errors
- sqsauditconsumer_processing_time

## Scripts
### scripts/run-local.ps1
- Loads .env;
- Starts Docker containers (SQL Server + LocalStack + Prometheus);
- Applies EF migrations;
- Starts API and Consumer processes;

### scripts/seed-sqs.ps1
- Used to validate the consumer without calling the API.
- Example:
    - .\scripts\seed-sqs.ps1
- Payloads are read from:
    - payloads/payload.json

## Docker Setup
1. SQL Server
    - Defined in:
        - docker/sqlserver/docker-compose.yml
    - Runs SQL Server 2022
        - Password injected via .env
        - Exposed on port: 14333

2. LocalStack (SQS)
    - Defined in:
        - docker/localstack/docker-compose.yml
        - SQS only
    - Exposed on port: 4566

3. Prometheus
    - Defined in:
        - docker/prometheus/docker-compose.yml
    - Exposed on port: 9090

## Migrations
- Under ALS.Infrastructure project;
- Migrations are applied automatically by run-local.ps1 script;

---

## Notes
- Ingestion source is stored as an enum (int) for better clarity;
- API and Consumer share the same ingestion logic;