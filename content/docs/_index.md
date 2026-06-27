---
title: "Documentation"
weight: 1
---
**SkyAPM-dotnet** is a community, open-source **.NET auto-instrumentation agent** for the .NET ecosystem. It provides distributed tracing, application topology, metrics, and log correlation for ASP.NET Core and .NET hosted applications, and reports the collected telemetry to an [Apache SkyWalking](https://skywalking.apache.org/) backend. It is an independent project, not an Apache Software Foundation or SkyWalking sub-project.

The agent targets `net8.0` and `net10.0` (the active .NET LTS releases); foundational libraries also build for `netstandard2.0`. Apps on newer runtimes (e.g. `net9.0`) are covered via the `net8.0` assemblies. The current version is **2.3.0**.

This agent speaks the SkyWalking **v8** protocol and propagates context with the **`sw8`** trace header only. By default the agent reports over gRPC to the OAP server on port `11800` (the SkyWalking query API and UI use `12800` / `8080`).

## Table of Contents

### Getting Started

- [Getting Started](guides/getting-started.md) — install the agent, point it at an OAP server, and see your first trace.

### Installation & Activation

- [Installation & Activation](guides/installation.md) — NuGet packages (`SkyAPM.Agent.AspNetCore`, `SkyAPM.Agent.GeneralHost`), zero-code activation via `ASPNETCORE_HOSTINGSTARTUPASSEMBLIES`, and how to set the service name.

### Configuration

- [Configuration Reference](guides/skyapm_config.md) — every option in `skyapm.json`, configuration sources and override order, and environment-variable mapping.
- [配置参考（中文）](guides/skyapm_config_cn.md) — Chinese translation of the configuration reference.

### Transports

- [Transports (gRPC & Kafka)](guides/transports.md) — choose and configure the gRPC reporter (default, port `11800`) or the Kafka reporter, including topic names and timeouts.

### Plugins / Supported Components

- [Plugins & Supported Components](guides/plugins.md) — how diagnostic plugins are wired in and which packages enable each integration.
- [Supported middlewares, frameworks and libraries](Supported-list.md) — the authoritative list of instrumented components.

### Logging

- [Logging](guides/logging.md) — the agent's internal log file, log level, rolling/retention options, and log-data reporting to OAP.

### CLI

- [CLI (`SkyAPM.DotNet.CLI`)](guides/cli.md) — the `dotnet skyapm config` tool that generates a `skyapm.json` for the gRPC or Kafka reporter.

### Troubleshooting

- [Troubleshooting](guides/troubleshooting.md) — diagnosing missing traces, no connection to OAP, the agent no-op conditions, and where to look in the logs.

### How to Build

- [How to Build](guides/how-to-build.md) — compile the agent from source, including the Git submodule and protocol-build steps.

## Quick Reference

| Topic | Default | Notes |
| --- | --- | --- |
| Protocol version | `v8` | `Transport.ProtocolVersion` |
| Trace header | `sw8` | `HeaderVersions` (`sw8` only) |
| Reporter | `grpc` | `grpc` or `kafka` — see [Transports](guides/transports.md) |
| OAP gRPC server | `localhost:11800` | `Transport.gRPC.Servers` |
| Activation | env var | `ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=SkyApm.Agent.AspNetCore` |
| Service name | _required_ | `SkyWalking:ServiceName` (env `SKYWALKING__SERVICENAME`) |
| Config file | `skyapm.json` | also `skyapm.{Environment}.json`; see [Configuration](guides/skyapm_config.md) |

> The agent no-ops if `SkyWalking:ServiceName` is empty or `SkyWalking:Enable` is `false`.
