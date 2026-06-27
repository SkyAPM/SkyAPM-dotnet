---
title: "配置参考（中文）"
weight: 4
---

# SkyAPM .NET agent 配置参考

本文档是 SkyAPM .NET agent（SkyAPM-dotnet，面向 Apache SkyWalking 的 C#/.NET 自动埋点探针）的权威配置参考。文档列出全部配置项及其类型、默认值与含义，说明配置的加载顺序与环境变量覆盖规则，并在结尾给出一份完整、正确的 `skyapm.json` 示例。英文版本见 [Configuration Reference](skyapm_config.md)。

## 概览

- SkyAPM-dotnet 是 Apache SkyWalking 的 C#/.NET 自动埋点探针，当前版本为 **2.3.0**。
- 目标框架为 `net8.0` 与 `net10.0`（LTS）；基础库同时兼容 `netstandard2.0`。
- 仅支持 SkyWalking **v8** 协议、跨进程传播头 **sw8**（不再支持已废弃的 sw6/v6）。
- 探针通过 gRPC 上报到 SkyWalking OAP 的 **11800** 端口（查询/UI 使用 12800/8080，与探针无关）。
- 协议默认值：`Transport.ProtocolVersion = "v8"`，`HeaderVersions = ["sw8"]`。

## 启用探针

ASP.NET Core 采用零代码的 `IHostingStartup` 启用方式，无需修改业务代码：

```bash
export ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=SkyApm.Agent.AspNetCore
export SKYWALKING__SERVICENAME=My_Service
export SKYWALKING__TRANSPORT__GRPC__SERVERS=localhost:11800
```

- 服务名通过 `SkyWalking:ServiceName` 配置，或使用环境变量 `SKYWALKING__SERVICENAME`。
- 当 `ServiceName` 为空，或 `SkyWalking:Enable` 为 `false` 时，探针不做任何操作（no-op）。

## 配置加载与环境变量覆盖

所有配置项均位于 `skyapm.json` 的 `SkyWalking` 根节点下。探针按以下来源读取配置：

1. `skyapm.json` 或 `skyapm.{Environment}.json`（如 `skyapm.Production.json`）。
2. `appsettings.json`。
3. `skywalking.json`。
4. 环境变量（使用双下划线 `__` 表示层级，例如 `SKYWALKING__TRANSPORT__GRPC__SERVERS`）。
5. 宿主的 `IConfiguration`。

**覆盖规则**：环境变量及后加载的来源会覆盖配置文件中的同名项。例如 `SKYWALKING__SERVICENAME` 覆盖 `skyapm.json` 中的 `SkyWalking:ServiceName`，`SKYWALKING__TRANSPORT__GRPC__SERVERS` 覆盖 `SkyWalking:Transport:gRPC:Servers`。

环境变量的命名规则：在配置路径前加 `SKYWALKING`，各级之间用 `__`（双下划线）连接，例如：

| 配置路径 | 对应环境变量 |
| --- | --- |
| `SkyWalking:ServiceName` | `SKYWALKING__SERVICENAME` |
| `SkyWalking:Transport:gRPC:Servers` | `SKYWALKING__TRANSPORT__GRPC__SERVERS` |
| `SkyWalking:Sampling:Percentage` | `SKYWALKING__SAMPLING__PERCENTAGE` |

## 全局配置（SkyWalking 根节点）

| 字段 | 类型 | 默认值 | 含义 |
| --- | --- | --- | --- |
| `Enable` | bool | `true` | 探针总开关，置为 `false` 时探针不做任何操作。 |
| `Namespace` | string | `""` | 逻辑命名空间/集群前缀。 |
| `ServiceName` | string | `"My_Service"` | 在 SkyWalking 中展示的服务名。 |
| `ApplicationCode` | string | _(无)_ | **已废弃**，`ServiceName` 的旧别名，仅为向后兼容保留，请改用 `ServiceName`。 |
| `ServiceInstanceName` | string | `{guid}@{ipv4}`（自动生成） | 服务实例的唯一标识。 |
| `HeaderVersions` | string[] | `["sw8"]` | 跨进程传播头格式，仅实现了 `sw8`。 |

### 各功能上报开关

以下开关分别控制各类数据的上报，默认均为 `true`：

| 字段 | 类型 | 默认值 | 含义 |
| --- | --- | --- | --- |
| `MeterActive` | bool | `true` | Meter（计量指标）上报开关。 |
| `MetricActive` | bool | `true` | CLR 指标上报开关。 |
| `SegmentActive` | bool | `true` | 追踪 Segment（链路片段）上报开关。 |
| `ProfilingActive` | bool | `true` | Profiling（性能剖析）上报开关。 |
| `ManagementActive` | bool | `true` | 实例管理（instance management）上报开关。 |
| `LogActive` | bool | `true` | 日志上报开关。 |

## 采样配置（Sampling）

| 字段 | 类型 | 默认值 | 含义 |
| --- | --- | --- | --- |
| `Sampling:SamplePer3Secs` | int | `-1` | 每 3 秒最多采样的追踪数；`-1` 表示不限制。 |
| `Sampling:Percentage` | double | `-1` | 百分比采样；`-1` 表示禁用（即全部采样）。 |
| `Sampling:IgnorePaths` | string[] | `null` | 不进行采样的请求路径；支持通配符 `*`、`**`、`?`。 |
| `Sampling:LogSqlParameterValue` | bool | `false` | 是否在数据库 Span 上记录 SQL 参数值。 |

通配符匹配示例：`a/b/c` => `a/b/c`，`a/*` => `a/b`，`a/**` => `a/b/c/d`，`a/?/c` => `a/b/c`。

## 探针自身日志（Logging）

该节点控制探针**自身**的诊断日志（基于 Serilog 的文件日志），与上报到 OAP 的应用日志无关。

| 字段 | 类型 | 默认值 | 含义 |
| --- | --- | --- | --- |
| `Logging:Level` | string | `"Information"` | 探针自诊断日志级别。 |
| `Logging:FilePath` | string | `"logs/skyapm-{Date}.log"` | 自诊断日志文件路径。 |
| `Logging:FileSizeLimitBytes` | long? | `268435456`（256MB） | 单个日志文件大小上限，达到后滚动。 |
| `Logging:FlushToDiskInterval` | long?（毫秒） | `1000` | 日志刷盘间隔。 |
| `Logging:RollingInterval` | string | `"Day"` | 日志滚动周期。 |
| `Logging:RollOnFileSizeLimit` | bool? | `false` | 是否同时按文件大小滚动。 |
| `Logging:RetainedFileCountLimit` | int? | `10` | 保留的日志文件个数。 |
| `Logging:RetainedFileTimeLimit` | long?（毫秒） | `864000000`（10 天） | 日志文件的保留时长。 |

## 传输配置（Transport）

| 字段 | 类型 | 默认值 | 含义 |
| --- | --- | --- | --- |
| `Transport:Reporter` | string | `"grpc"` | 上报方式，可选 `"grpc"` 或 `"kafka"`。 |
| `Transport:ProtocolVersion` | string | `"v8"` | OAP 通信协议，仅支持 `v8`。 |
| `Transport:QueueSize` | int | `10000` | 单通道缓冲区大小（总容量 = `QueueSize * Parallel`，满时丢弃数据）。 |
| `Transport:BatchSize` | int | `2000` | 单通道批量大小（总批量 = `BatchSize * Parallel`）。 |
| `Transport:Parallel` | int | `5` | 并行上报通道数。 |
| `Transport:Interval` | int（毫秒） | `50` | 两个批次之间的最大间隔；`-1` 表示等待上一批次完成。 |

### gRPC（Transport:gRPC）

| 字段 | 类型 | 默认值 | 含义 |
| --- | --- | --- | --- |
| `Transport:gRPC:Servers` | string | `"localhost:11800"` | OAP gRPC 地址，多个用英文逗号分隔；除非已带 `dns://` 或 `static://` 等 scheme，否则自动加 `http://` 前缀。 |
| `Transport:gRPC:ConnectTimeout` | int（毫秒） | `10000` | 通道连接超时时间。 |
| `Transport:gRPC:Timeout` | int（毫秒） | `10000` | 单次调用（注册/管理）的截止时间。 |
| `Transport:gRPC:ReportTimeout` | int（毫秒） | `600000` | 数据上报流式调用的截止时间。 |
| `Transport:gRPC:Authentication` | string | `null` | 可选的鉴权 token（通过 gRPC metadata 传递）。 |

### Kafka（Transport:Kafka）

当 `Transport:Reporter` 设为 `"kafka"` 时使用。

| 字段 | 类型 | 默认值 | 含义 |
| --- | --- | --- | --- |
| `Transport:Kafka:BootstrapServers` | string | `"localhost:9092"` | Kafka broker 地址。 |
| `Transport:Kafka:TopicTimeoutMs` | int | `3000` | 获取 topic 元数据的超时时间。 |
| `Transport:Kafka:MessageTimeoutMs` | int | `5000` | 生产者投递超时时间。 |
| `Transport:Kafka:TopicMeters` | string | `skywalking-meters` | Meter 数据的 topic 名。 |
| `Transport:Kafka:TopicCLRMetrics` | string | `skywalking-metrics` | CLR 指标的 topic 名。 |
| `Transport:Kafka:TopicSegments` | string | `skywalking-segments` | 追踪 Segment 的 topic 名。 |
| `Transport:Kafka:TopicProfilings` | string | `skywalking-profilings` | Profiling 数据的 topic 名。 |
| `Transport:Kafka:TopicManagements` | string | `skywalking-managements` | 实例管理数据的 topic 名。 |
| `Transport:Kafka:TopicLogs` | string | `skywalking-logs` | 日志数据的 topic 名。 |

## 追踪配置（Tracing）

| 字段 | 类型 | 默认值 | 含义 |
| --- | --- | --- | --- |
| `Tracing:ExceptionMaxDepth` | int | `3` | 捕获内部异常（inner exception）的最大深度。 |
| `Tracing:DbPeerSimpleFormat` | bool | `false` | 是否使用简化的数据库 peer（`host:port`）格式。 |

## 应用日志上报（Diagnostics:Logging）

| 字段 | 类型 | 默认值 | 含义 |
| --- | --- | --- | --- |
| `Diagnostics:Logging:CollectLevel` | enum | `Information` | 上报到 OAP 的最低应用日志级别（Microsoft.Extensions.Logging）。可选值：`Trace` / `Debug` / `Information` / `Warning` / `Error` / `Critical` / `None`。 |

## 组件配置（Component）

### ASP.NET Core（Component:AspNetCore）

控制入站请求数据的采集，采集到的内容会作为 Span 标签（tag）记录。

| 字段 | 类型 | 默认值 | 含义 |
| --- | --- | --- | --- |
| `Component:AspNetCore:CollectCookies` | string[] | `null` | 作为 Span 标签采集的入站请求 Cookie 名列表。 |
| `Component:AspNetCore:CollectHeaders` | string[] | `null` | 作为 Span 标签采集的入站请求 Header 名列表。 |
| `Component:AspNetCore:CollectBodyContentTypes` | string[] | `null` | 需要采集入站请求体的 Content-Type 列表。 |
| `Component:AspNetCore:CollectBodyLengthThreshold` | int（字节） | `2048` | 请求体长度超过该值时跳过采集。 |

### HttpClient（Component:HttpClient）

控制出站 HTTP 请求的追踪与数据采集，采集到的内容会作为 Span 标签记录。

| 字段 | 类型 | 默认值 | 含义 |
| --- | --- | --- | --- |
| `Component:HttpClient:IgnorePaths` | string[] | `null` | 不进行追踪的出站路径；支持通配符。 |
| `Component:HttpClient:StopHeaderPropagationPaths` | string[] | `null` | 对这些出站路径不注入 `sw8` 传播头。 |
| `Component:HttpClient:CollectRequestHeaders` | string[] | `null` | 作为 Span 标签采集的出站请求 Header 名列表。 |
| `Component:HttpClient:CollectRequestBodyContentTypes` | string[] | `null` | 需要采集出站请求体的 Content-Type 列表。 |
| `Component:HttpClient:CollectResponseBodyContentTypes` | string[] | `null` | 需要采集响应体的 Content-Type 列表。 |
| `Component:HttpClient:CollectBodyLengthThreshold` | int（字节） | `2048` | 请求体/响应体长度超过该值时跳过采集。 |

## 完整 `skyapm.json` 示例

```json
{
  "SkyWalking": {
    "Enable": true,
    "Namespace": "",
    "ServiceName": "My_Service",
    "HeaderVersions": [
      "sw8"
    ],
    "MeterActive": true,
    "MetricActive": true,
    "SegmentActive": true,
    "ProfilingActive": true,
    "ManagementActive": true,
    "LogActive": true,
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
        "TopicCLRMetrics": "skywalking-metrics",
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

## 相关文档

- [Configuration Reference（English）](skyapm_config.md)
- [How to Build](how-to-build.md)
