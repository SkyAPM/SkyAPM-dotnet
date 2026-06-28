# End-to-end test

Verifies that the SkyAPM .NET agent reports **traces, logs, and CLR metrics** to a real,
**version-pinned** Apache SkyWalking **OAP + BanyanDB** backend, for demo services running on
**.NET 8 and .NET 10**.

## Layout

| File | Purpose |
|---|---|
| `demo/` | Minimal ASP.NET Core app + `Dockerfile` (multi-target net8/net10); references the agent **from source** and is activated via `ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=SkyAPM.Agent.AspNetCore` |
| `docker-compose.yml` | Pinned `apache/skywalking-oap-server` + `apache/skywalking-banyandb` + `demo-net8` + `demo-net10`, all on one network |
| `run.sh` | Self-contained runner: build+up → traffic → assert trace/log/metric per service via `swctl` |
| `e2e.yaml` + `expected/` | [skywalking-infra-e2e](https://github.com/apache/skywalking-infra-e2e) config for CI (`e2e run -c test/e2e/e2e.yaml`) |

## Run it

```bash
bash test/e2e/run.sh            # builds images, boots the stack, verifies, tears down
# or, via the infra-e2e framework:
e2e run -c test/e2e/e2e.yaml
```

Pin a different SkyWalking/BanyanDB via env: `OAP_IMAGE=apache/skywalking-oap-server:<tag>`,
`BANYANDB_IMAGE=apache/skywalking-banyandb:<tag>`.

## What it asserts (per service, ×2 TFMs)

- **trace** — OAP learned the endpoint `GET:/work` (entry span) — the demo also makes an
  HttpClient call to `/downstream` (exit span + a second segment).
- **log** — application logs (via the MSLogging plugin) reached OAP.
- **metric** — a CLR instance metric (`instance_clr_available_worker_threads`) has values.

## Important: networking

The demos talk to OAP's **plaintext gRPC (`oap:11800`) over the Docker network**, not via a host
port. .NET cleartext HTTP/2 (h2c) does **not** survive Docker Desktop's macOS host port-forward
(you'll see `unable to establish HTTP/2 connection` / issue #571) — so the agent and OAP must share
a Docker network, which this compose does. Only OAP's HTTP query port `12800` is exposed to the
host (for `swctl`).
