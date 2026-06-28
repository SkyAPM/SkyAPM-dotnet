---
title: "Transports"
weight: 5
---
The SkyAPM .NET agent buffers the tracing, metrics, profiling, management, and
log data it collects, then ships it to the SkyWalking OAP backend through a
**transport** (also called a **reporter**). This guide explains how to pick a
transport and how to tune it. For the full configuration model, file locations,
and override precedence, see [Configuration](skyapm_config.md).

## Selecting a transport

The transport is selected by a single setting:

```json
{
  "SkyWalking": {
    "Transport": {
      "Reporter": "grpc"
    }
  }
}
```

| `Reporter` value | Transport library      | Description                                              |
| ---------------- | ---------------------- | ------------------------------------------------------- |
| `grpc` (default) | `SkyApm.Transport.Grpc`  | Streams data directly to the OAP gRPC receiver.        |
| `kafka`          | `SkyApm.Transport.Kafka` | Publishes data to Kafka topics that OAP fetches from.  |

When `Reporter` is omitted, the agent defaults to `grpc`. All transports use the
SkyWalking **v8 / `sw8`** protocol; this is the only protocol the agent speaks
(`Transport.ProtocolVersion` defaults to `v8`, `HeaderVersions` defaults to
`["sw8"]`).

## Common transport tuning

These options live directly under `SkyWalking:Transport` and apply regardless of
which reporter you choose. The agent maintains `Parallel` independent
queue/worker pairs; the totals below are the per-queue values multiplied by
`Parallel`.

| Option      | Default | Unit         | Description                                                                                         |
| ----------- | ------- | ------------ | --------------------------------------------------------------------------------------------------- |
| `QueueSize` | `10000` | items        | Capacity of each buffer queue (`TotalQueueSize = QueueSize * Parallel`). Items are dropped when a queue is full. |
| `BatchSize` | `2000`  | items        | Maximum items consumed per flush (`TotalBatchSize = BatchSize * Parallel`).                          |
| `Parallel`  | `5`     | count        | Number of parallel queues/workers shipping data to the backend.                                     |
| `Interval`  | `50`    | milliseconds | Maximum interval between flushes. Use `-1` to wait for the previous batch to complete before the next. |

```json
{
  "SkyWalking": {
    "Transport": {
      "Reporter": "grpc",
      "ProtocolVersion": "v8",
      "QueueSize": 10000,
      "BatchSize": 2000,
      "Parallel": 5,
      "Interval": 50
    }
  }
}
```

Raise `QueueSize` / `BatchSize` (or `Parallel`) for high-throughput services that
would otherwise drop data; lower `Interval` to reduce reporting latency at the
cost of more frequent backend round-trips.

## gRPC transport (default)

The gRPC transport streams data directly to the OAP gRPC receiver (default port
`11800`). Its options live under `SkyWalking:Transport:gRPC`.

| Option           | Default            | Unit         | Description                                                                 |
| ---------------- | ------------------ | ------------ | --------------------------------------------------------------------------- |
| `Servers`        | `localhost:11800`  | address(es)  | One or more OAP gRPC addresses (see below).                                 |
| `ConnectTimeout` | `10000`            | milliseconds | Timeout for establishing the gRPC connection.                               |
| `Timeout`        | `10000`            | milliseconds | Timeout for short unary calls (e.g. instance registration / properties).    |
| `ReportTimeout`  | `600000`           | milliseconds | Timeout for the long-lived reporting streams (10 minutes by default).       |
| `Authentication` | _(empty)_          | token        | Optional auth token; see [Authentication](#authentication).                 |

### Servers

`Servers` accepts one or more addresses separated by commas (`,`). Each address
is normalized as follows: if it does **not** already contain a `://` scheme, the
agent prefixes it with `http://`. This lets you point at a single backend, a
fixed set of backends, or a gRPC name-resolver / load-balancing scheme.

```json
{
  "SkyWalking": {
    "Transport": {
      "Reporter": "grpc",
      "gRPC": {
        "Servers": "oap-host:11800",
        "ConnectTimeout": 10000,
        "Timeout": 10000,
        "ReportTimeout": 600000
      }
    }
  }
}
```

Multiple explicit addresses (each gets an `http://` prefix automatically):

```json
{
  "SkyWalking:Transport:gRPC:Servers": "oap-1:11800,oap-2:11800,oap-3:11800"
}
```

Using a gRPC load-balancing scheme — because these include a `://`, the agent
leaves them untouched:

- `dns://` — resolve a DNS name to the live set of OAP backends, e.g.
  `dns:///oap.skywalking.svc:11800` (lets the gRPC client load-balance across all
  resolved addresses, ideal for Kubernetes headless services).
- `static://` — a fixed, comma-style list handled by the gRPC client's static
  resolver, e.g. `static://oap-1:11800,oap-2:11800`.

```json
{
  "SkyWalking:Transport:gRPC:Servers": "dns:///oap.skywalking.svc:11800"
}
```

### TLS / SSL

Because the `http://` prefix is only added when the address has no scheme, giving an
explicit **`https://`** address makes the agent connect to OAP over **TLS** — point it
at the OAP's TLS gRPC port:

```json
{
  "SkyWalking:Transport:gRPC:Servers": "https://your-oap-host:443"
}
```

The server certificate is validated against the host's trust store (system CA roots),
so this works out of the box for certificates issued by a trusted CA. There is
currently **no** option for a custom / self-signed CA or for mutual (client) TLS —
those would require a custom `HttpClientHandler`, which the agent does not expose
today. (`Authentication`, below, is unrelated to TLS — it is an application token
sent as gRPC metadata.)

### Authentication

If the OAP backend has token authentication enabled, set `Authentication` to the
shared token. The agent sends it on every gRPC call as the metadata header
`Authentication`. Leave it empty to disable.

```json
{
  "SkyWalking": {
    "Transport": {
      "gRPC": {
        "Servers": "oap-host:11800",
        "Authentication": "your-oap-token"
      }
    }
  }
}
```

## Kafka transport

The Kafka transport publishes data to Kafka topics instead of calling the OAP
gRPC receiver directly. OAP then **fetches** the data from those topics.

### When to use it

Choose Kafka when you want to decouple agents from the OAP backend with a
durable buffer — for example to absorb large traffic spikes, to survive
temporary OAP outages without dropping data, or to fan data into a Kafka-centric
ingestion pipeline. For most deployments the default gRPC transport is simpler
and is the recommended starting point.

### OAP-side requirement

The Kafka transport only works if **SkyWalking's Kafka fetcher is enabled on the
OAP side** (the `kafka-fetcher` module in the backend). The agent's topic names
must match the topics the OAP fetcher consumes. Configure and enable the fetcher
on the backend before switching the agent to `kafka`; otherwise the published
data will never be ingested.

### Configuration

Set `Reporter` to `kafka` and configure the broker list and topic names under
`SkyWalking:Transport:Kafka`.

| Option             | Default                   | Unit         | Description                                                  |
| ------------------ | ------------------------- | ------------ | ------------------------------------------------------------ |
| `BootstrapServers` | `localhost:9092`          | address(es)  | Kafka brokers, e.g. `address1:port1[,address2:port2...]`.    |
| `TopicTimeoutMs`   | `3000`                    | milliseconds | Timeout for topic metadata / readiness operations.           |
| `MessageTimeoutMs` | `5000`                    | milliseconds | Per-message delivery timeout.                                 |
| `TopicMeters`      | `skywalking-meters`       | topic        | Topic for service meter data.                                |
| `TopicCLRMetrics`  | `skywalking-clr-metrics`  | topic        | Topic for .NET CLR (runtime) metrics.                        |
| `TopicSegments`    | `skywalking-segments`     | topic        | Topic for trace segments.                                    |
| `TopicProfilings`  | `skywalking-profilings`   | topic        | Topic for profiling task data.                               |
| `TopicManagements` | `skywalking-managements`  | topic        | Topic for service-instance management (registration/heartbeat). |
| `TopicLogs`        | `skywalking-logs`         | topic        | Topic for log data.                                          |

The default topic names match SkyWalking's defaults, so you normally only need to
set `Reporter` and `BootstrapServers`. Override the topic names only if your OAP
Kafka fetcher is configured to consume from different topics.

### Example

```json
{
  "SkyWalking": {
    "ServiceName": "your_service_name",
    "Transport": {
      "Reporter": "kafka",
      "Interval": 50,
      "QueueSize": 10000,
      "BatchSize": 2000,
      "Parallel": 5,
      "Kafka": {
        "BootstrapServers": "kafka-1:9092,kafka-2:9092",
        "TopicTimeoutMs": 3000,
        "MessageTimeoutMs": 5000,
        "TopicMeters": "skywalking-meters",
        "TopicCLRMetrics": "skywalking-clr-metrics",
        "TopicSegments": "skywalking-segments",
        "TopicProfilings": "skywalking-profilings",
        "TopicManagements": "skywalking-managements",
        "TopicLogs": "skywalking-logs"
      }
    }
  }
}
```

## Overriding via environment variables

Every transport setting can be supplied through environment variables using the
double-underscore (`__`) separator, which is convenient for containers and CI.
Environment variables override values from the config file. For example:

```bash
export SKYWALKING__TRANSPORT__REPORTER=grpc
export SKYWALKING__TRANSPORT__GRPC__SERVERS=oap-host:11800
export SKYWALKING__TRANSPORT__GRPC__AUTHENTICATION=your-oap-token
```

See [Configuration](skyapm_config.md) for the complete list of settings and the
configuration source precedence.
