#!/usr/bin/env bash
set -e

awslocal sqs create-queue --queue-name als-audit-events