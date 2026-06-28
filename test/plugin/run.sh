#!/usr/bin/env bash
#
# SkyAPM-dotnet plugin-test runner.
#
# For each scenario under scenarios/, and for each "<TFM> <version>" line in the
# scenario's support-version.list, this:
#   1. builds the scenario app image (agent built FROM SOURCE + the library at that version),
#   2. boots it together with the dedicated mock collector (a SkyWalking receiver, not a real
#      OAP+DB) so the agent reports its REAL emitted segments over gRPC,
#   3. drives the scenario's entry endpoint to generate a trace,
#   4. validates the captured segments against the scenario's config/expectedData.yaml by
#      POSTing it to the collector's /dataValidate (the SkyWalking validator).
#
# Only the FINAL EMITTED TRACES are asserted. Requires: docker, and (to build the collector)
# git + a JDK/Maven via the agent-test-tool's mvnw.
#
# Usage:
#   bash test/plugin/run.sh                  # all scenarios, all versions
#   bash test/plugin/run.sh cap-scenario     # one scenario
set -euo pipefail

HERE="$(cd "$(dirname "$0")" && pwd)"

# The mock collector is built from a PINNED apache/skywalking-agent-test-tool commit (the same
# one apache/skywalking-java's test/plugin pins today). The published skyapm/mock-collector:latest
# image is from 2019 and does not implement the V8 management protocol the current agent needs.
COLLECTOR_COMMIT="${COLLECTOR_COMMIT:-7220c715d280cf0d7421f17bbc8c8de57249914d}"
COLLECTOR_IMAGE="${COLLECTOR_IMAGE:-skyapm/mock-collector:${COLLECTOR_COMMIT:0:12}}"
export COLLECTOR_IMAGE

ENTRY_PORT="${ENTRY_PORT:-8080}"
COLLECTOR_HTTP="${COLLECTOR_HTTP:-http://localhost:12800}"
SCENARIO_FILTER="${1:-}"

log() { echo "[plugin-test] $*"; }

ensure_collector() {
  if docker image inspect "$COLLECTOR_IMAGE" >/dev/null 2>&1; then
    log "mock collector image present: $COLLECTOR_IMAGE"
    return
  fi
  log "building mock collector from apache/skywalking-agent-test-tool@${COLLECTOR_COMMIT}"
  local work; work="$(mktemp -d)"
  git clone -q --filter=blob:none https://github.com/apache/skywalking-agent-test-tool "$work/att"
  git -C "$work/att" checkout -q "$COLLECTOR_COMMIT"
  ( cd "$work/att" && ./mvnw -B -q -Dmaven.test.skip=true clean package )
  local tarball; tarball="$(find "$work/att" -name skywalking-mock-collector.tar.gz | head -1)"
  local ctx; ctx="$(mktemp -d)"
  cp "$tarball" "$ctx/"; cp "$work/att/docker/mock-collector/Dockerfile" "$ctx/"
  docker build -t "$COLLECTOR_IMAGE" "$ctx"
  rm -rf "$work" "$ctx"
}

# Wait until the collector has received at least one segment, then validate.
validate() {
  local expected="$1"
  for _ in $(seq 1 30); do
    if curl -sf --max-time 10 "$COLLECTOR_HTTP/receiveData" 2>/dev/null | grep -q "segmentId:"; then
      break
    fi
    sleep 2
  done
  local code; code="$(curl -s -o /tmp/plugin-validate.out -w '%{http_code}' --max-time 20 \
    -X POST --data-binary @"$expected" "$COLLECTOR_HTTP/dataValidate")"
  if [ "$code" = "200" ]; then
    return 0
  fi
  log "VALIDATION FAILED (HTTP $code):"
  sed -n '/cause by/,$p' /tmp/plugin-validate.out | head -40
  return 1
}

run_case() {
  local scen="$1" tfm="$2" ver="$3"
  local dotnet="${tfm#net}"           # net8.0 -> 8.0
  log "=== $(basename "$scen") :: TFM=$tfm CAP_VERSION=$ver ==="
  (
    cd "$scen"
    TFM="$tfm" CAP_VERSION="$ver" DOTNET_VERSION="$dotnet" \
      docker compose up -d --build --quiet-pull
  )
  local rc=0
  # health-gate the app, then drive the entry endpoint a few times
  for _ in $(seq 1 30); do
    curl -sf --max-time 5 "http://localhost:${ENTRY_PORT}/case/healthCheck" >/dev/null 2>&1 && break || sleep 2
  done
  for _ in $(seq 1 5); do curl -sf --max-time 5 "http://localhost:${ENTRY_PORT}/case/cap" >/dev/null 2>&1 || true; sleep 1; done
  validate "$scen/config/expectedData.yaml" || rc=1
  ( cd "$scen" && docker compose down -v >/dev/null 2>&1 || true )
  return $rc
}

main() {
  ensure_collector
  local failures=0 total=0
  for scen in "$HERE"/scenarios/*/; do
    scen="${scen%/}"
    [ -f "$scen/support-version.list" ] || continue
    [ -n "$SCENARIO_FILTER" ] && [ "$(basename "$scen")" != "$SCENARIO_FILTER" ] && continue
    while read -r raw; do
      local line; line="$(echo "${raw%%#*}" | xargs)"; [ -z "$line" ] && continue
      local tfm ver; tfm="$(echo "$line" | awk '{print $1}')"; ver="$(echo "$line" | awk '{print $2}')"
      total=$((total + 1))
      if run_case "$scen" "$tfm" "$ver"; then log "PASS $(basename "$scen") $tfm/$ver"
      else log "FAIL $(basename "$scen") $tfm/$ver"; failures=$((failures + 1)); fi
    done < "$scen/support-version.list"
  done
  log "done: $((total - failures))/$total passed"
  [ "$failures" -eq 0 ]
}

main "$@"
