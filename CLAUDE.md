# SkyAPM-dotnet — repository guide

C#/.NET auto-instrumentation agent for the .NET ecosystem that reports tracing, metrics, and logs
to an Apache SkyWalking backend over the **sw8 / v8** gRPC protocol. Targets **net8.0** and
**net10.0**; foundational libraries also build for **netstandard2.0**. Version in `build/version.props`.

## Layout

```
src/                         The agent (≈30 NuGet packages)
  SkyApm.Abstractions/         interfaces + Config classes (telemetry contracts)
  SkyApm.Core/                 tracing engine, sampling, context
  SkyApm.Agent.AspNetCore/     ASP.NET Core host (zero-code via HostingStartup); assembly = SkyAPM.Agent.AspNetCore
  SkyApm.Agent.GeneralHost/    generic-host (console/Worker) entry
  SkyApm.Agent.Hosting/        shared composition root (AddSkyAPM); wires default plugins + transport
  SkyApm.Diagnostics.*/         16 plugins (AspNetCore, HttpClient, SqlClient, EntityFrameworkCore[+providers],
                               Grpc, Grpc.Net.Client, MSLogging, CAP, MassTransit, MongoDB, SmartSql,
                               FreeSql, FreeRedis) — thin DiagnosticSource adapters
  SkyApm.Transport.Grpc/        default reporter: Segment(traces)/CLRStats(metrics)/Log/Register/Ping (V8/)
  SkyApm.Transport.Kafka/       alternative Kafka reporter
  SkyApm.Transport.Protocol/    gRPC stubs codegen'd from the protocol-v3 git submodule
  SkyApm.PeerFormatters.*/      DB peer-address resolvers
  SkyApm.DotNet.CLI/            `dotnet skyapm config` global tool
test/
  SkyApm.Core.Tests/            xUnit unit tests (the only existing tests)
  e2e/                          end-to-end demo + setup (in progress)
sample/                        runnable example apps (Frontend→Backend, FreeSql, GenericHost, grpc, …)
build/                         common/version/sign .props, version.cake, SkyAPM.snk (strong-name key)
content/                       Hugo (Hextra theme) documentation site → GitHub Pages
themes/hugo-... , hugo.toml, i18n/   docs site theme + config (.github/workflows/docs.yml deploys)
.github/workflows/            net-ci-it.yml (build+test net8/net10), docs.yml (Pages)
```

## Build / test

- `git submodule update --init` (populates `src/SkyApm.Transport.Protocol/protocol-v3`).
- `dotnet restore` → `dotnet build src/SkyApm.Transport.Protocol` (codegen first) → `dotnet build`.
- `dotnet test --framework net8.0` (and `net10.0`). Only the net8/net10 *runtimes* are tested.

## Conventions / gotchas

- Activation: env `ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=SkyAPM.Agent.AspNetCore` (note casing `APM`).
- Config: `skyapm.json` (or env `SKYWALKING__...`, double-underscore). Agent no-ops if `ServiceName`
  empty or `SkyWalking:Enable=false`. Full reference: `content/docs/guides/skyapm_config.md`.
- Plugins: a fixed default set is auto-registered; others are opt-in via the `AddSkyAPM(ext => …)` lambda.
- The repo also pins per-TFM package versions via `Condition="'$(TargetFramework)' == 'netX'"` blocks.
- Project is **independent** — reports to Apache SkyWalking but is not an ASF/SkyWalking sub-project.
