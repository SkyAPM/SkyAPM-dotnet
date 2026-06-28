---
title: "End-to-End Testing"
weight: 11
---

The repository ships a containerized end-to-end test under [`test/e2e/`](https://github.com/SkyAPM/SkyAPM-dotnet/tree/main/test/e2e)
that proves the agent reports **traces, logs, and CLR metrics** to a real, version-pinned Apache
SkyWalking **OAP + BanyanDB** backend — for demo services running on **.NET 8 and .NET 10**.

## Run it

```bash
bash test/e2e/run.sh
```

This builds the demo images, boots the stack via Docker Compose, drives traffic, verifies the data
in OAP, and tears everything down. Requires **docker** and **python3**. You can also run it through
the [skywalking-infra-e2e](https://github.com/apache/skywalking-infra-e2e) framework:

```bash
e2e run -c test/e2e/e2e.yaml
```

## What it asserts (per service, ×2 TFMs)

| Telemetry | How it's produced | How it's verified |
| --- | --- | --- |
| **trace** | inbound `GET /work` + an outbound HttpClient call to `/downstream` | OAP `service_cpm` has a value |
| **log** | `ILogger` records via the MSLogging plugin | OAP `queryLogs` returns records |
| **metric** | CLR runtime metrics | OAP `instance_clr_available_worker_threads` has a value |

Verification uses the OAP **GraphQL API** directly (`verify.py`), so it is not coupled to a specific
`swctl` version.

## Layout

| File | Purpose |
| --- | --- |
| `demo/` | minimal ASP.NET Core app + multi-stage `Dockerfile` (net8/net10); references the agent **from source**, activated via `ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=SkyAPM.Agent.AspNetCore` |
| `docker-compose.yml` | pinned OAP + BanyanDB + `demo-net8` + `demo-net10`, all on one network |
| `verify.py` | GraphQL verifier (services / traces / logs / CLR metrics) |
| `run.sh` | self-contained runner | 
| `e2e.yaml` + `expected/` | skywalking-infra-e2e config |

Pin a different backend with env: `OAP_IMAGE=...`, `BANYANDB_IMAGE=...`.

## CI

The `.github/workflows/e2e.yml` workflow runs this on changes to `src/**` or `test/e2e/**` (and on
manual dispatch), building the net8/net10 images and asserting trace/log/metric.

## Networking note (cleartext gRPC / h2c)

The agent reports to OAP's **plaintext gRPC (`oap:11800`) over the Docker network**, not a host
port. .NET cleartext HTTP/2 (h2c) does **not** reliably survive Docker Desktop's macOS host
port-forward — a host-run agent against a forwarded OAP port can fail with
`unable to establish HTTP/2 connection` ([#571](https://github.com/SkyAPM/SkyAPM-dotnet/issues/571)).
Keeping the agent and OAP on the same Docker network (as this compose does) avoids it.
