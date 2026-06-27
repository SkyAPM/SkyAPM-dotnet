---
title: "Troubleshooting"
weight: 9
---

# Troubleshooting & FAQ

Common problems when running the SkyAPM .NET agent and how to fix them. Each
item follows a **symptom → cause → fix** shape. For configuration reference see
[Configuration](skyapm_config.md); the docs index is [docs/README.md](/docs/).

> The SkyAPM .NET agent (`SkyAPM-dotnet`, version 2.3.0) auto-instruments .NET
> apps and reports to the **SkyWalking OAP** backend. It speaks the SkyWalking
> **v8** protocol (header `sw8`) **only**, and targets `net8.0` and `net10.0`
> (the foundational libraries also build for `netstandard2.0`).

## No data shows up in the SkyWalking UI

This is the most common report. The agent is silent by design when it is not
fully activated, so work through these checks in order.

### Symptom: the service never appears in the UI

**Cause:** the agent no-ops when it is disabled or has no service name. The
activation gate in `AddSkyAPM` returns early — registering nothing — when
`SkyWalking:Enable` is `false`, or when `SkyWalking:ServiceName` is empty.

**Fix:** make sure both are set.

```json
{
  "SkyWalking": {
    "Enable": true,
    "ServiceName": "your_service_name"
  }
}
```

- `Enable` must not be `false` (it defaults to `true`).
- `ServiceName` must be a non-empty string. The internal default is
  `My_Service`, but if you explicitly set it to an empty string the agent
  silently does nothing.
- You can also set the service name with the environment variable
  `SKYWALKING__SERVICENAME` (double-underscore form).

### Symptom: the app starts but the agent is never loaded (ASP.NET Core)

**Cause:** zero-code activation relies on the `IHostingStartup` hook, which the
runtime only loads when its assembly is listed in
`ASPNETCORE_HOSTINGSTARTUPASSEMBLIES`. If that variable is missing, the agent
assembly is never started.

**Fix:** set the variable to the agent assembly name before the app starts.

```bash
# Linux / macOS
export ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=SkyAPM.Agent.AspNetCore
```

```bash
# Windows (cmd)
set ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=SkyAPM.Agent.AspNetCore
```

The value is the assembly name only (no path, no `.dll`). The variable must be
visible to the process that actually runs your app (your shell, the systemd
unit, the container `ENV`, or the IIS site environment — see
[IIS / multiple instances](#iis-and-multiple-instances-notes) below). See
[Getting Started](getting-started.md) for the full setup walkthrough.

### Symptom: agent is active but no traces reach the OAP

**Cause:** the OAP gRPC receiver is unreachable from the app, or the wrong
address/port is configured.

**Fix:** confirm the agent points at the OAP **gRPC** port `11800` and that it
is reachable from the host running your app.

```json
{
  "SkyWalking": {
    "Transport": {
      "gRPC": {
        "Servers": "your-oap-host:11800"
      }
    }
  }
}
```

Quick reachability check from the app host:

```bash
# should connect; a refused/timeout means a firewall, wrong host, or OAP down
nc -zv your-oap-host 11800
```

If you see no errors but still no data, raise the agent log level to inspect the
reporters — see [Reading the agent self-log](#reading-the-agent-self-log).

## Connection and port problems

### Symptom: "connection refused" / timeouts, or you configured the UI port and nothing works

**Cause:** the SkyWalking ports are mixed up. The agent talks to the OAP over
**gRPC on 11800**. Ports `12800` (OAP HTTP/REST + GraphQL query) and `8080`
(the UI) are for the web/query side and must **not** be used by the agent.

**Fix:** use the right port for the right job.

| Port    | Purpose                                  | Used by              |
| ------- | ---------------------------------------- | -------------------- |
| `11800` | OAP gRPC data-collection receiver        | **The agent** (this) |
| `12800` | OAP HTTP/REST + GraphQL query            | UI / `swctl`         |
| `8080`  | SkyWalking UI                            | Browsers             |

So `SkyWalking:Transport:gRPC:Servers` should be `host:11800`. If you put
`12800` or `8080` there, the gRPC handshake fails and no data is reported.

You can override the server list with an environment variable (note the
double-underscore separator):

```bash
export SKYWALKING__TRANSPORT__GRPC__SERVERS=your-oap-host:11800
```

`Servers` accepts multiple comma-separated addresses for load balancing, e.g.
`oap-1:11800,oap-2:11800`.

## Wrong or old config copied from elsewhere (sw6 / v6)

### Symptom: copied a config from an old project/blog and traces never link, or the agent behaves oddly

**Cause:** the config came from an old SkyAPM-dotnet / SkyWalking 6 setup that
uses the legacy `sw6` header and `v6` protocol. **This agent supports `sw8` /
`v8` ONLY.** Older protocol versions are not supported and will not interop with
a current OAP.

**Fix:** ensure the header and protocol version are `sw8` / `v8`.

```json
{
  "SkyWalking": {
    "HeaderVersions": [
      "sw8"
    ],
    "Transport": {
      "ProtocolVersion": "v8"
    }
  }
}
```

These are also the built-in defaults (`HeaderVersions: ["sw8"]`,
`Transport.ProtocolVersion: "v8"`), so the simplest fix is to delete the stale
`HeaderVersions` / `ProtocolVersion` entries entirely and let the defaults
apply. Better still, regenerate a clean file with the CLI, which always emits
`sw8` / `v8`:

```bash
dotnet skyapm config your_service_name your-oap-host:11800
```

> Tip: search any inherited config for the strings `sw6` or `v6`. If you find
> them, the config predates this agent.

## Reading the agent self-log

The agent writes its own diagnostics to a rolling file, separate from your
application logs. This is the fastest way to see what the reporters are doing.

### Symptom: you need to see why the agent is (not) sending data

**Cause:** at the default `Information` level the self-log is quiet and won't
show per-segment / per-report detail.

**Fix:** find the log, then raise the level to `Debug`.

**Location** — by default `logs/skyapm-{Date}.log`, relative to the app's
working directory (`{Date}` is the rolling day stamp, e.g.
`logs/skyapm-20260627.log`). The path is configurable:

```json
{
  "SkyWalking": {
    "Logging": {
      "Level": "Debug",
      "FilePath": "logs/skyapm-{Date}.log"
    }
  }
}
```

**Raise verbosity** — set `SkyWalking:Logging:Level` to `Debug`. Valid values
are `Verbose`, `Debug`, `Information`, `Warning`, `Error`, and `Fatal` (they map
to Serilog levels). An unrecognized value falls back to `Error`, which makes the
log nearly empty — so spell the level exactly as listed above.

You can also set it via environment variable:

```bash
export SKYWALKING__LOGGING__LEVEL=Debug
```

After restarting, look in the log for connection attempts to `:11800` and report
results. gRPC connection errors, auth failures, or "queue full" messages point
directly at the underlying problem.

## Traces are sparse or missing intermittently (sampling)

### Symptom: only some requests produce traces, or counts look far lower than your real traffic

**Cause:** sampling is enabled. The agent supports a per-3-second rate cap
(`Sampling:SamplePer3Secs`) and a percentage sampler (`Sampling:Percentage`).
When `SamplePer3Secs` is set to a positive number, only that many traces are
kept every 3 seconds and the rest are dropped — so high-traffic services will
look "thinned out".

**Fix:** to capture everything, disable both samplers by setting them to their
"off" sentinel value `-1` (the default):

```json
{
  "SkyWalking": {
    "Sampling": {
      "SamplePer3Secs": -1,
      "Percentage": -1.0
    }
  }
}
```

- `SamplePer3Secs: -1` → no per-3-second cap (sampler off; keep all).
  A value like `100` keeps at most 100 traces per 3 seconds.
- `Percentage: -1.0` → percentage sampler off. A value such as `50` keeps
  roughly 50% of traces.

If you intentionally sample in production to control volume, this is expected
behavior, not a bug — raise the cap if you need more traces. See the sampling
notes in [Configuration](skyapm_config.md).

## IIS and multiple instances notes

### Symptom: works under Kestrel/`dotnet run` but not when hosted in IIS

**Cause:** under IIS the agent process does not inherit your interactive shell's
environment, so `ASPNETCORE_HOSTINGSTARTUPASSEMBLIES` (and any
`SKYWALKING__*` overrides) may be unset for the worker process.

**Fix:** set the environment variables where the IIS-hosted process can see
them — for example in `web.config` under the ASP.NET Core Module, so they are
present for the worker:

```xml
<aspNetCore processPath="dotnet" arguments=".\YourApp.dll">
  <environmentVariables>
    <environmentVariable name="ASPNETCORE_HOSTINGSTARTUPASSEMBLIES" value="SkyAPM.Agent.AspNetCore" />
  </environmentVariables>
</aspNetCore>
```

### Symptom: multiple sites/instances report as one service, or instances are hard to tell apart

**Cause:** every process that shares the same `ServiceName` is grouped as one
service. By default the agent builds a per-instance name
(`{guid}@{ipAddress}`), but if you pin `ServiceInstanceName` to a constant, or
run many app-pool instances that you want labeled distinctly, they can be hard
to separate.

**Fix:**

- Give each logically distinct app a unique `SkyWalking:ServiceName`.
- For multiple instances of the *same* service (e.g. several IIS app-pool worker
  processes, a web farm, or scaled-out replicas), either let the agent generate
  the instance name automatically (recommended) or set a distinct
  `SkyWalking:ServiceInstanceName` per instance so they don't collide in the UI.

## Where to ask for help

- **SkyAPM .NET agent (this project) issues** — file a GitHub issue at
  <https://github.com/SkyAPM/SkyAPM-dotnet/issues>. Include your agent version
  (2.3.0), target framework, the relevant part of your config (with secrets
  redacted), and an excerpt of the `Debug`-level agent self-log.
- **SkyWalking OAP backend or UI questions** (storage, OAP startup, UI panels,
  alarms, query) — those belong to **Apache SkyWalking**, not this repo. Ask via
  the Apache SkyWalking community at <https://github.com/apache/skywalking>.

## See also

- [Configuration](skyapm_config.md) — full config reference.
- [Getting Started](getting-started.md) — install and activate the agent.
- [docs/README.md](/docs/) — documentation index.
