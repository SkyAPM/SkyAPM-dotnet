---
title: "Configuration Reference"
weight: 3
---
This is the authoritative configuration reference for the SkyAPM .NET agent
(SkyAPM-dotnet) — a C#/.NET auto-instrumentation agent for the .NET ecosystem that reports to an Apache SkyWalking backend.
Every key lives under the top-level `SkyWalking` root and reports to the
SkyWalking OAP backend.

> **Protocol note:** SkyAPM-dotnet speaks the SkyWalking **v8** wire protocol and
> the **sw8** cross-process header **only**. The legacy `sw6`/`v6` protocol is no
> longer supported. The defaults are `Transport.ProtocolVersion = "v8"` and
> `HeaderVersions = ["sw8"]`; do not change them.

The agent targets `net8.0` and `net10.0` (LTS), with foundational libraries also
built for `netstandard2.0`. This reference applies to version 2.3.0.

## How configuration is loaded

The agent builds its configuration from several sources. Listed from lowest to
highest precedence (each source overrides the ones above it):

1. **Built-in defaults** — every key has a hard-coded default (the values in this
   document).
2. **`appsettings.json`** and `appsettings.{Environment}.json`.
3. **`skywalking.json`** and `skywalking.{Environment}.json`.
4. **`skyapm.json`** and `skyapm.{Environment}.json` — the canonical, dedicated
   agent config file (generate it with the [CLI](cli.md)).
5. **External config file** referenced by the `SKYAPM__CONFIG__PATH`
   (or legacy `SKYWALKING__CONFIG__PATH`) environment variable.
6. **Environment variables** — using `__` (double underscore) as the section
   separator, e.g. `SKYWALKING__TRANSPORT__GRPC__SERVERS` overrides
   `SkyWalking:Transport:gRPC:Servers`.
7. **Host `IConfiguration`** — the application's own configuration, applied last.

In short: **environment variables and the host configuration win over the JSON
files**, and `skyapm.json` wins over `appsettings.json`/`skywalking.json`. The
`{Environment}` token follows the ASP.NET Core environment
(`Development` / `Staging` / `Production`, etc.).

`{Environment}` is resolved from `ASPNETCORE_ENVIRONMENT` / `DOTNET_ENVIRONMENT`.

### Activation

Enable zero-code instrumentation for ASP.NET Core by adding the agent to the
hosting-startup assemblies — no code changes required:

```bash
export ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=SkyApm.Agent.AspNetCore
```

Set the service name through `SkyWalking:ServiceName` (or the
`SKYWALKING__SERVICENAME` environment variable). The agent **no-ops** if
`ServiceName` is empty or `SkyWalking:Enable` is `false`. See
[Getting Started](getting-started.md) for an end-to-end walkthrough.

## General

These keys sit directly under `SkyWalking`.

| Key | Type | Default | Description |
| --- | --- | --- | --- |
| `Enable` | bool | `true` | Master switch for the agent; `false` disables all instrumentation. |
| `Namespace` | string | `""` | Logical namespace/cluster prefix for the service. |
| `ServiceName` | string | `"My_Service"` | Service name shown in SkyWalking. The agent no-ops if this is empty. |
| `ApplicationCode` | string | _(none)_ | **Deprecated** alias for `ServiceName`, kept for backward compatibility. Use `ServiceName` instead. |
| `ServiceInstanceName` | string | `{guid}@{ipv4}` (auto) | Unique instance id; auto-generated from a GUID and the host IPv4 address. |
| `HeaderVersions` | string[] | `["sw8"]` | Cross-process correlation header format. Only `sw8` is implemented. |
| `MeterActive` | bool | `true` | Toggle meter (OpenTelemetry-style meter) reporting. |
| `MetricActive` | bool | `true` | Toggle CLR metric reporting (GC, threads, CPU, memory). |
| `SegmentActive` | bool | `true` | Toggle trace segment reporting. |
| `ProfilingActive` | bool | `true` | Toggle in-process profiling task support. |
| `ManagementActive` | bool | `true` | Toggle instance management (heartbeat/properties) reporting. |
| `LogActive` | bool | `true` | Toggle application log reporting to OAP. |

## Sampling

Keys under `SkyWalking:Sampling`.

| Key | Type | Default | Description |
| --- | --- | --- | --- |
| `SamplePer3Secs` | int | `-1` | Max number of sampled traces per 3 seconds; `-1` means unlimited. |
| `Percentage` | double | `-1` | Percentage sampling rate (0–100); `-1` disables percentage sampling (sample all). |
| `IgnorePaths` | string[] | `null` | Inbound request paths that are NOT sampled. Supports wildcards `*`, `**`, `?`. |
| `LogSqlParameterValue` | bool | `false` | Record SQL parameter values on database spans. |

Wildcard usage: `a/b/c` matches `a/b/c`, `a/*` matches `a/b`, `a/**` matches
`a/b/c/d`, and `a/?/c` matches `a/b/c`.

## Logging (agent self-log)

Keys under `SkyWalking:Logging`. These control the agent's **own diagnostic log**
(a Serilog rolling file), not the application logs that are shipped to OAP (for
that, see [Diagnostics.Logging](#diagnosticslogging)).

| Key | Type | Default | Description |
| --- | --- | --- | --- |
| `Level` | string | `"Information"` | Self-diagnostic log level. |
| `FilePath` | string | `"logs/skyapm-{Date}.log"` | Self-log file path; `{Date}` is replaced at runtime. |
| `FileSizeLimitBytes` | long? | `268435456` (256 MB) | Roll size for the self-log file. |
| `FlushToDiskInterval` | long? (ms) | `1000` | Flush interval for the self-log, in milliseconds. |
| `RollingInterval` | string | `"Day"` | Self-log rolling interval (`Day`, `Hour`, etc.). |
| `RollOnFileSizeLimit` | bool? | `false` | Also roll the self-log when `FileSizeLimitBytes` is reached. |
| `RetainedFileCountLimit` | int? | `10` | Number of rolled self-log files to retain. |
| `RetainedFileTimeLimit` | long? (ms) | `864000000` (10 days) | Maximum age of retained self-log files, in milliseconds. |

## Transport

Keys under `SkyWalking:Transport`. Controls how telemetry is buffered, batched,
and reported. See [Transports](transports.md) for a deeper discussion of the
gRPC and Kafka reporters.

| Key | Type | Default | Description |
| --- | --- | --- | --- |
| `Reporter` | string | `"grpc"` | Reporter implementation: `"grpc"` or `"kafka"`. |
| `ProtocolVersion` | string | `"v8"` | OAP wire protocol version. Only `v8` is supported. |
| `QueueSize` | int | `10000` | Per-channel buffer size. Total = `QueueSize × Parallel`; elements drop when full. |
| `BatchSize` | int | `2000` | Per-channel batch size. Total = `BatchSize × Parallel`. |
| `Parallel` | int | `5` | Number of parallel reporting channels. |
| `Interval` | int (ms) | `50` | Max interval between batches, in milliseconds; `-1` waits for the previous batch to complete. |

### Transport.gRPC

Keys under `SkyWalking:Transport:gRPC`. Used when `Reporter = "grpc"`.

| Key | Type | Default | Description |
| --- | --- | --- | --- |
| `Servers` | string | `"localhost:11800"` | OAP gRPC address(es), comma-separated. `http://` is auto-prefixed unless a `dns://` or `static://` scheme is present. |
| `ConnectTimeout` | int (ms) | `10000` | Channel connect timeout, in milliseconds. |
| `Timeout` | int (ms) | `10000` | Per-call (register / management) deadline, in milliseconds. |
| `ReportTimeout` | int (ms) | `600000` | Data-report streaming deadline, in milliseconds. |
| `Authentication` | string | `null` | Optional auth token sent as gRPC metadata. |

> The agent reports to the OAP **gRPC receiver on port 11800**. Ports 12800 / 8080
> are the OAP query / UI endpoints and are not used by the agent.

### Transport.Kafka

Keys under `SkyWalking:Transport:Kafka`. Used when `Reporter = "kafka"`.

| Key | Type | Default | Description |
| --- | --- | --- | --- |
| `BootstrapServers` | string | `"localhost:9092"` | Kafka broker address(es), comma-separated. |
| `TopicTimeoutMs` | int | `3000` | Topic metadata timeout, in milliseconds. |
| `MessageTimeoutMs` | int | `5000` | Producer message delivery timeout, in milliseconds. |
| `TopicMeters` | string | `"skywalking-meters"` | Topic for meter data. |
| `TopicCLRMetrics` | string | `"skywalking-clr-metrics"` | Topic for CLR metrics. |
| `TopicSegments` | string | `"skywalking-segments"` | Topic for trace segments. |
| `TopicProfilings` | string | `"skywalking-profilings"` | Topic for profiling data. |
| `TopicManagements` | string | `"skywalking-managements"` | Topic for instance management data. |
| `TopicLogs` | string | `"skywalking-logs"` | Topic for application logs. |

## Tracing

Keys under `SkyWalking:Tracing`.

| Key | Type | Default | Description |
| --- | --- | --- | --- |
| `ExceptionMaxDepth` | int | `3` | Max inner-exception depth captured on error spans. |
| `DbPeerSimpleFormat` | bool | `false` | Use the simplified `host:port` form for the database peer. |

## Diagnostics.Logging

Keys under `SkyWalking:Diagnostics:Logging`. Controls which **application** logs
(via `Microsoft.Extensions.Logging`) are shipped to OAP.

| Key | Type | Default | Description |
| --- | --- | --- | --- |
| `CollectLevel` | enum | `Information` | Minimum application log level shipped to OAP. One of `Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical`, `None`. |

## Component.AspNetCore

Keys under `SkyWalking:Component:AspNetCore`. Controls capture of **inbound**
ASP.NET Core request data as span tags (implemented by the hosting diagnostic
handler).

| Key | Type | Default | Description |
| --- | --- | --- | --- |
| `CollectCookies` | string[] | `null` | Names of inbound request cookies captured as span tags. |
| `CollectHeaders` | string[] | `null` | Names of inbound request headers captured as span tags. |
| `CollectBodyContentTypes` | string[] | `null` | Content-types whose inbound request body is captured as a span tag. |
| `CollectBodyLengthThreshold` | int (bytes) | `2048` | Skip body capture when `Content-Length` exceeds this value. |

## Component.HttpClient

Keys under `SkyWalking:Component:HttpClient`. Controls tracing and data capture
for **outbound** `HttpClient` calls (implemented by the request diagnostic
handler).

| Key | Type | Default | Description |
| --- | --- | --- | --- |
| `IgnorePaths` | string[] | `null` | Outbound paths that are NOT traced. Supports wildcards `*`, `**`, `?`. |
| `StopHeaderPropagationPaths` | string[] | `null` | Outbound paths where the `sw8` headers are NOT injected. Supports wildcards. |
| `CollectRequestHeaders` | string[] | `null` | Outbound request headers captured as span tags. |
| `CollectRequestBodyContentTypes` | string[] | `null` | Content-types whose outbound request body is captured as a span tag. |
| `CollectResponseBodyContentTypes` | string[] | `null` | Content-types whose response body is captured as a span tag. |
| `CollectBodyLengthThreshold` | int (bytes) | `2048` | Skip body capture when the content length exceeds this value. |

## Example `skyapm.json`

A complete, correct example using `sw8` / `v8` and realistic defaults. The
fastest way to produce this file is the [CLI generator](cli.md); the structure
below matches its output.

```json
{
  "SkyWalking": {
    "Enable": true,
    "ServiceName": "My_Service",
    "Namespace": "",
    "HeaderVersions": [
      "sw8"
    ],
    "Sampling": {
      "SamplePer3Secs": -1,
      "Percentage": -1,
      "IgnorePaths": [
        "/health",
        "/metrics"
      ],
      "LogSqlParameterValue": false
    },
    "Logging": {
      "Level": "Information",
      "FilePath": "logs/skyapm-{Date}.log",
      "FileSizeLimitBytes": 268435456,
      "FlushToDiskInterval": 1000,
      "RollingInterval": "Day",
      "RollOnFileSizeLimit": false,
      "RetainedFileCountLimit": 10,
      "RetainedFileTimeLimit": 864000000
    },
    "Transport": {
      "Reporter": "grpc",
      "ProtocolVersion": "v8",
      "QueueSize": 10000,
      "BatchSize": 2000,
      "Parallel": 5,
      "Interval": 50,
      "gRPC": {
        "Servers": "localhost:11800",
        "ConnectTimeout": 10000,
        "Timeout": 10000,
        "ReportTimeout": 600000,
        "Authentication": ""
      },
      "Kafka": {
        "BootstrapServers": "localhost:9092",
        "TopicTimeoutMs": 3000,
        "MessageTimeoutMs": 5000,
        "TopicMeters": "skywalking-meters",
        "TopicCLRMetrics": "skywalking-clr-metrics",
        "TopicSegments": "skywalking-segments",
        "TopicProfilings": "skywalking-profilings",
        "TopicManagements": "skywalking-managements",
        "TopicLogs": "skywalking-logs"
      }
    },
    "Tracing": {
      "ExceptionMaxDepth": 3,
      "DbPeerSimpleFormat": false
    },
    "Diagnostics": {
      "Logging": {
        "CollectLevel": "Information"
      }
    },
    "Component": {
      "AspNetCore": {
        "CollectCookies": null,
        "CollectHeaders": null,
        "CollectBodyContentTypes": null,
        "CollectBodyLengthThreshold": 2048
      },
      "HttpClient": {
        "IgnorePaths": null,
        "StopHeaderPropagationPaths": null,
        "CollectRequestHeaders": null,
        "CollectRequestBodyContentTypes": null,
        "CollectResponseBodyContentTypes": null,
        "CollectBodyLengthThreshold": 2048
      }
    }
  }
}
```

## See also

- [CLI generator](cli.md) — generate `skyapm.json` from the command line.
- [Transports](transports.md) — gRPC vs. Kafka reporter details.
- [Getting Started](getting-started.md) — install and activate the agent.
- [Documentation index](/docs/)
