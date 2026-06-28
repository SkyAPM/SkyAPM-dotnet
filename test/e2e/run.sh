#!/usr/bin/env bash
# End-to-end test: boots a pinned SkyWalking OAP + BanyanDB and the .NET 8 / .NET 10
# demo services (agent from source), generates traffic, and verifies TRACES, LOGS
# and CLR METRICS for BOTH services reached OAP — via GraphQL (version-robust; no
# swctl version coupling). Requires docker + python3.
#
#   bash test/e2e/run.sh
set -euo pipefail
cd "$(dirname "$0")"

COMPOSE="docker compose -f docker-compose.yml"
OAP_GRAPHQL="http://localhost:12800/graphql"

# Set SKIP_CLEANUP=1 (e.g. in CI) to leave the stack up so logs can be collected.
cleanup() { [ -n "${SKIP_CLEANUP:-}" ] && return 0; $COMPOSE down -v >/dev/null 2>&1 || true; }
trap cleanup EXIT

echo "==> (1/4) build + start stack (OAP + BanyanDB + demo-net8 + demo-net10)"
$COMPOSE up -d --build
echo "    waiting for OAP (:12800) and demos…"
for _ in $(seq 1 60); do curl -sf http://localhost:12800/internal/l7check >/dev/null 2>&1 && break; sleep 3; done
for _ in $(seq 1 40); do curl -sf localhost:18801/healthz >/dev/null 2>&1 && curl -sf localhost:18802/healthz >/dev/null 2>&1 && break; sleep 2; done

echo "==> (2/4) generate traffic on both demos"
for _ in $(seq 1 25); do
  curl -sf http://localhost:18801/work >/dev/null || true   # net8
  curl -sf http://localhost:18802/work >/dev/null || true   # net10
  sleep 0.5
done

echo "==> (3/4) wait for OAP/BanyanDB to index (traces, logs, metrics)…"
sleep 55

echo "==> (4/4) verify trace / log / metric via GraphQL"
python3 verify.py "$OAP_GRAPHQL" e2e-demo-net8 e2e-demo-net10
