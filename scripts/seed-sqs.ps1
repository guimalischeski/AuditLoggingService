$ErrorActionPreference = "Stop"

$region = $env:AWS_DEFAULT_REGION
if ([string]::IsNullOrWhiteSpace($region)) {
    $region = "us-east-1"
}

$scriptDir   = Split-Path -Parent $MyInvocation.MyCommand.Path
$payloadPath = Resolve-Path (Join-Path $scriptDir "..\payloads\payload.json")

$queueUrl = aws --endpoint-url=http://localhost:4566 --region $region `
  sqs get-queue-url `
  --queue-name als-audit-events `
  --query QueueUrl `
  --output text

Write-Host "QueueUrl = $queueUrl"

aws --endpoint-url=http://localhost:4566 --region $region `
  sqs send-message `
  --queue-url $queueUrl `
  --message-body ("file://{0}" -f $payloadPath.Path)
