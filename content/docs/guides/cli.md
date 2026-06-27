---
title: "CLI"
weight: 8
---
`SkyApm.DotNet.CLI` is a [.NET global tool](https://learn.microsoft.com/dotnet/core/tools/global-tools) that scaffolds the configuration file for the SkyAPM .NET agent. It generates a ready-to-edit `skyapm.json` so you do not have to author the configuration by hand.

## Install

Install the tool globally from NuGet:

```bash
dotnet tool install -g SkyApm.DotNet.CLI
```

Once installed, the `dotnet skyapm` command is available on your `PATH`. To update or remove it later:

```bash
dotnet tool update -g SkyApm.DotNet.CLI
dotnet tool uninstall -g SkyApm.DotNet.CLI
```

## The `config` command

The tool exposes a single command, `config`, which writes a configuration file into the current working directory.

```bash
dotnet skyapm config <serviceName> [options]
```

### Argument

| Argument        | Description                                                                 |
| --------------- | --------------------------------------------------------------------------- |
| `<serviceName>` | **Required.** The `ServiceName` reported to SkyWalking OAP. If empty, the command prints `Invalid ServiceName.` and exits without writing a file. |

### Options

| Option                       | Description                                                                                          | Default            |
| ---------------------------- | -------------------------------------------------------------------------------------------------- | ------------------ |
| `--reporter <grpc\|kafka>`   | Selects the transport reporter written into the `Transport.Reporter` field.                        | `grpc`             |
| `--grpcservers <host:port>`  | Address of the SkyWalking OAP gRPC endpoint, written to `Transport.gRPC.Servers`.                   | `localhost:11800`  |
| `--kafkaservers <host:port>` | Address of the Kafka bootstrap servers, written to `Transport.Kafka.BootstrapServers`.             | `localhost:9092`   |
| `-e\|--Environment <env>`    | Generates an environment-specific file. Follows the app's environment (e.g. `Development`, `Staging`, `Production`). | _(none)_ |

> The gRPC and Kafka sections are **both** written to the file regardless of which reporter you choose, so you can switch reporters later by editing `Transport.Reporter`. The `--reporter` flag only sets which one is active at startup.

### Behavior notes

- **File name.** Without `-e`, the tool writes `skyapm.json`. With `-e <env>`, it writes `skyapm.{Environment}.json` (for example, `skyapm.Development.json`). Both are placed in the current working directory.
- **No overwrite.** If a config file with the target name already exists, the tool prints `Already exist config file in <path>` and exits without changing anything. Delete or rename the existing file first if you want to regenerate it.
- **Invalid reporter falls back to gRPC.** If `--reporter` is anything other than `grpc` or `kafka` (case-insensitive), the tool prints `Invalid reporter type <value>. Use default type.` and uses `grpc`.

## Examples

Generate a default config (gRPC reporter, local OAP) for a service named `sample-service`:

```bash
dotnet skyapm config sample-service
```

Point the agent at a remote OAP gRPC endpoint:

```bash
dotnet skyapm config sample-service --grpcservers oap.example.com:11800
```

Generate a Kafka-reporter config:

```bash
dotnet skyapm config sample-service --reporter kafka --kafkaservers kafka.example.com:9092
```

Generate an environment-specific file (writes `skyapm.Development.json`):

```bash
dotnet skyapm config sample-service -e Development
```

## Generated file

The command emits a `SkyWalking` configuration root. The example below shows the default output (gRPC reporter); the values mirror what the tool writes, with placeholders substituted from your arguments.

```json
{
  "SkyWalking": {
    "Enable": "true",
    "ServiceName": "sample-service",
    "Namespace": "",
    "HeaderVersions": [
      "sw8"
    ],
    "Sampling": {
      "SamplePer3Secs": -1,
      "Percentage": -1.0
    },
    "Logging": {
      "Level": "Information",
      "FilePath": "logs/skyapm-{Date}.log"
    },
    "MeterActive": true,
    "MetricActive": true,
    "SegmentActive": true,
    "ProfilingActive": true,
    "ManagementActive": true,
    "LogActive": true,
    "Transport": {
      "ProtocolVersion": "v8",
      "QueueSize": 10000,
      "BatchSize": 2000,
      "Parallel": 5,
      "Interval": 50,
      "Reporter": "grpc",
      "gRPC": {
        "Servers": "localhost:11800",
        "Timeout": 10000,
        "ConnectTimeout": 10000,
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
    }
  }
}
```

### What the fields mean

- **Protocol.** `Transport.ProtocolVersion` is `v8` and `HeaderVersions` is `["sw8"]`. SkyAPM-dotnet speaks the SkyWalking v8 protocol with the `sw8` propagation header only.
- **`*Active` flags.** `MeterActive`, `MetricActive`, `SegmentActive`, `ProfilingActive`, `ManagementActive`, and `LogActive` toggle which categories of telemetry the agent reports. They are all `true` by default. Set one to `false` to stop reporting that category.
- **gRPC section.** Used when `Reporter` is `grpc`. `Servers` is the OAP gRPC endpoint (port `11800` by default). `Authentication` is the optional OAP auth token.
- **Kafka section.** Used when `Reporter` is `kafka`. `BootstrapServers` is the Kafka cluster address, and the `Topic*` entries map each telemetry category to a Kafka topic.

For a full reference of every configuration key (including `Sampling`, `Logging`, namespaces, and environment-variable overrides), see [Configuration](skyapm_config.md).

## Next steps

After generating the file, edit any values you need and run your application with the agent enabled. The agent reads `skyapm.json` (or `skyapm.{Environment}.json`), and values can be overridden by `appsettings.json`, environment variables (double-underscore form, e.g. `SKYWALKING__TRANSPORT__GRPC__SERVERS`), and the host `IConfiguration`.

- [Configuration](skyapm_config.md) — full configuration reference.
- [Documentation index](/docs/) — all SkyAPM-dotnet guides.
