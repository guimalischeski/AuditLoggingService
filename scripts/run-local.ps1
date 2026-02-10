$ErrorActionPreference = "Stop"

function Import-DotEnv {
    param([string]$Path)

    if (!(Test-Path $Path)) { throw ".env not found at: $Path" }

    Get-Content $Path | ForEach-Object {
        $line = $_.Trim()
        if (!$line -or $line.StartsWith("#")) { return }

        $idx = $line.IndexOf("=")
        if ($idx -lt 1) { return }

        $name = $line.Substring(0, $idx).Trim()
        $value = $line.Substring($idx + 1).Trim()

        if (($value.StartsWith('"') -and $value.EndsWith('"')) -or ($value.StartsWith("'") -and $value.EndsWith("'"))) {
            $value = $value.Substring(1, $value.Length - 2)
        }

        [Environment]::SetEnvironmentVariable($name, $value, "Process")
    }
}

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$envPath   = Join-Path $scriptDir "..\.env"
Import-DotEnv $envPath

function Ensure-ComposeServiceRunning {
    param(
        [Parameter(Mandatory=$true)][string]$ComposeFile,
        [Parameter(Mandatory=$true)][string]$ContainerName
    )

    $exists = docker ps -a --format "{{.Names}}" | Select-String -SimpleMatch $ContainerName
    if ($exists) {
        Write-Host "Container '$ContainerName' exists. Starting it..."
        docker start $ContainerName | Out-Null
    }
    else {
        Write-Host "Container '$ContainerName' does not exist. Creating via compose..."
        docker compose -f $ComposeFile up -d
    }
}

function Wait-SqlServerReady {
    param(
        [Parameter(Mandatory=$true)][string]$ContainerName,
        [int]$MaxRetries = 40
    )

    Write-Host "Waiting for SQL Server to accept connections..."

    for ($i = 1; $i -le $MaxRetries; $i++) {
        docker exec $ContainerName /opt/mssql-tools18/bin/sqlcmd `
            -S localhost -U sa -P "$env:SQL_SA_PASSWORD" -C -Q "SELECT 1" 2>$null | Out-Null

        if ($LASTEXITCODE -eq 0) {
            Write-Host "SQL Server is ready"
            return
        }

        Start-Sleep -Seconds 3
    }

    throw "SQL Server did not become ready in time"
}

Write-Host "Starting SQL Server..."
Ensure-ComposeServiceRunning -ComposeFile "docker/sqlserver/docker-compose.yml" -ContainerName "als-sql"

Write-Host "Starting LocalStack (SQS)..."
Ensure-ComposeServiceRunning -ComposeFile "docker/localstack/docker-compose.yml" -ContainerName "als-localstack"

Write-Host "Starting Prometheus..."
Ensure-ComposeServiceRunning -ComposeFile "docker/prometheus/docker-compose.yml" -ContainerName "als-prometheus"

Wait-SqlServerReady -ContainerName "als-sql"

Write-Host "Running EF migrations..."
dotnet ef database update `
  --project ALS.Infrastructure `
  --startup-project ALS.Api

Write-Host "Starting API..."
Start-Process powershell -ArgumentList "dotnet run --project ALS.Api"

Write-Host "Starting Consumer..."
Start-Process powershell -ArgumentList "dotnet run --project ALS.Consumer"

Write-Host "Local environment is up"
Write-Host " - API Swagger:      $apiUrl/swagger"
Write-Host " - Prometheus UI:    http://localhost:9090"