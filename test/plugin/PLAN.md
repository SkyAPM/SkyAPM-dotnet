# Plugin Test Plan (cross-version)

Status: **proposed** · Scope: a plugin/integration test framework for SkyAPM-dotnet that verifies
every instrumentation plugin against **multiple versions** of the library it instruments, and
asserts the agent's **traces, metrics, logs, and (future) public APIs**.

Modeled on the Apache SkyWalking **Java agent plugin-test** framework
(`skywalking-java/test/plugin`) and reusing the existing, language-agnostic
**`skywalking-agent-test-tool`** (mock OAP collector + YAML validator).

---

## 1. Goals

1. **Catch regressions per plugin, per library version.** Each plugin (EF Core, MongoDB, CAP,
   FreeSql, FreeRedis, MassTransit, SmartSql, SqlClient, gRPC, …) must keep producing correct
   spans as the instrumented library evolves across its supported version range.
2. **Cover all four telemetry paths**, including the planned additions:
   - **Traces** — `SegmentReporter` → `TraceSegmentReportService`.
   - **Metrics** — today `CLRStatsReporter` → `CLRMetricReportService`; **planned OTel-style
     `Meter`** → `MeterReportService`.
   - **Logs** — `LogReporter` → `LogReportService` (today fed only by MSLogging; **planned: more
     log producers**).
   - **Public APIs** — the **planned manual tracing/metric/log facade** (`ITracingContext` and a
     public API surface).
3. **Be runnable locally and in CI** with a clear, low-friction developer loop.

## 2. How the Java framework works (reference)

A Java *scenario* is a self-contained webapp (a JVM jar or a Tomcat war) plus three declarative
files:

| File | Purpose |
|---|---|
| `configuration.yml` | case metadata: `type` (jvm/tomcat), `entryService` (GET URL to drive traffic), `healthCheck` (HEAD URL), `startScript`, `runningMode`, and `dependencies` (side-car services → docker-compose) |
| `support-version.list` | a **literal enumeration** of versions, one per line (`version[,key=value…]`). **No range DSL** — the runner loops each line |
| `config/expectedData.yaml` | the expected segments/spans/refs with **operator-based** field assertions |

`run.sh <scenario>` loops each version, rebuilds the same webapp with `-Dtest.framework.version=<v>`
(injected as the library's dependency version), boots a Docker container with the agent + a
**mock collector** mounted, waits on the health URL, sends one request to the entry URL, pulls the
collected `actualData.yaml` from the mock collector's REST port, and runs the **validator** to
compare actual vs expected. Container exit code = test result.

**We adopt the same shape, idiomatically for .NET** — but split into two layers (below) because
.NET's `DiagnosticSource` seam lets most assertions run **in-process**, far faster than
container-per-version.

## 3. Architecture — two layers

### Layer 1 — In-process integration tests (the bulk; fast)

One xUnit project per plugin: `test/plugin/integration/SkyApm.Diagnostics.<Plugin>.Tests`.

- **Drive the real library against a real backend** spun up with **[Testcontainers for .NET]**
  (Postgres, MySQL/MariaDB, MongoDB, Redis, RabbitMQ/Kafka, SQL Server). The library emits to its
  real `DiagnosticListener`; `TracingDiagnosticProcessorObserver` dispatches to the plugin.
- **Assert via capture-fakes.** Build the DI container with `AddSkyAPM(...)`, then **replace the
  reporter/dispatcher interfaces** with in-memory capture implementations:
  - `ISegmentDispatcher` / `ISegmentReporter` → captured **spans** (operationName, component,
    layer, tags, peer, refs, error).
  - `ILogReporter` → captured **logs** (+ trace correlation).
  - `IMeterReporter` *(new, see §6)* / `ICLRStatsReporter` → captured **metrics**.

  These are the single swap point — all live in `src/SkyApm.Abstractions/Transport/`.
- **Why two styles per plugin:**
  - *Handler-level* (fastest): call the `[DiagnosticName("…")]` methods directly with hand-built
    `EventData` — no backend needed. Good for tag/edge-case coverage.
  - *Library-level* (realistic): run the real library against the Testcontainers backend — proves
    the listener name + event payloads still match the library version.

### Layer 2 — End-to-end wire validation (smoke; reuses the SkyWalking tool)

For a handful of representative scenarios per release, validate the **actual gRPC wire payload**
end-to-end by reusing **`skywalking-agent-test-tool`** unchanged:

- Run the mock collector as a container (see [`docker-compose.mock-collector.yml`](docker-compose.mock-collector.yml)):
  `ghcr.io/apache/skywalking-agent-test-tool/mock-collector` — gRPC **:19876**, REST **:12800**.
- Point the agent at it (`SkyWalking:Transport:gRPC:Servers = mock-collector:19876`), run a small
  sample app, send one request, then `GET http://mock-collector:12800/receiveData` to dump
  `actualData.yaml` and validate against an `expectedData.yaml`.
- **Validator operators** (plain string = exact-equals): `null` · `not null` · `not blank` · `eq` ·
  `nq` · `gt` · `ge` · `start with` · `end with`.
- **Persistence caveat (important):** the mock collector **persists and returns only segments,
  meters, and logs**. CLR/JVM-metrics, management (register/keepalive), and events are **ACKed but
  not retrievable**. → Use Layer-1 in-process fakes to assert CLR metrics & management; use Layer 2
  for traces, **Meter** metrics, and logs.

## 4. Cross-version matrix

- The library-under-test version is an **overridable MSBuild property**, e.g.
  `<PackageReference Include="MongoDB.Driver" Version="$(LibVersion)" />` with a sensible default.
- CI runs a matrix `dotnet test -p:LibVersion=<v>` over an explicit list per plugin — the .NET
  equivalent of `support-version.list`. Keep one csproj per plugin and **float the version**, rather
  than per-TFM `Condition` pinning.
- Adopt **Central Package Management** (`Directory.Packages.props`) for the *shared/agent*
  dependencies only (stop drift); keep each **library-under-test** version floated via the property.

**Per-plugin version targets** (`support-version.list` equivalent — fill the concrete list per
plugin; one representative version per minor, lowest-supported → latest):

| Plugin | Library (NuGet) | Backend (Testcontainers) |
|---|---|---|
| EntityFrameworkCore | `Microsoft.EntityFrameworkCore` (+ `.Relational`) | per provider below |
| …EntityFrameworkCore.Npgsql | `Npgsql.EntityFrameworkCore.PostgreSQL` | PostgreSQL |
| …EntityFrameworkCore.Pomelo.MySql | `Pomelo.EntityFrameworkCore.MySql` | MySQL/MariaDB |
| …EntityFrameworkCore.Sqlite | `Microsoft.EntityFrameworkCore.Sqlite` | (file/in-memory) |
| MongoDB | `MongoDB.Driver` / `.Core` | MongoDB |
| CAP | `DotNetCore.CAP` | RabbitMQ/Kafka + storage |
| FreeSql | `FreeSql` | per provider |
| FreeRedis | `FreeRedis` | Redis |
| MassTransit | `MassTransit` | RabbitMQ |
| SmartSql | `SmartSql` | SQLite/MySQL |
| SqlClient | `System.Data.SqlClient` + `Microsoft.Data.SqlClient` | SQL Server |
| Grpc / Grpc.Net.Client | `Grpc` / `Grpc.Net.Client` | in-process gRPC server |
| HttpClient / AspNetCore / MSLogging | BCL (no external lib) | in-process |

## 5. Directory layout

```
test/plugin/
  PLAN.md                              ← this file
  docker-compose.mock-collector.yml    ← Layer-2 mock collector (reused SkyWalking tool)
  Directory.Packages.props             ← (to add) CPM for shared/agent deps
  shared/
    SkyApm.Testing.Harness/            ← capture-fakes + AddSkyAPM test host + assertion helpers
  integration/
    SkyApm.Diagnostics.EntityFrameworkCore.Tests/   ← support-versions.txt + Tests.cs
    SkyApm.Diagnostics.MongoDB.Tests/
    SkyApm.Diagnostics.Cap.Tests/
    … (one per plugin)
  e2e/
    scenarios/
      efcore-sqlite/{expectedData.yaml, app/, README.md}    ← Layer-2 scenarios
```

## 6. Covering the planned metrics / logs / public APIs

The agent's reporters are interfaces in `src/SkyApm.Abstractions/Transport/`, each driven by an
`ExecutionService` and bound in `ServiceCollectionExtensions`. The new features plug in cleanly and
are testable with the same seams:

- **Meter metrics** *(new)*: add `IMeterReporter` (mirror `ICLRStatsReporter`), a V8 `MeterReporter`
  over the already-generated `MeterReportService` stub, and a `MeterStatsService` execution service
  bridging `System.Diagnostics.Metrics`. **Tests:** capture-fake `IMeterReporter` (Layer 1) **and**
  Layer-2 (meters *are* persisted by the mock collector → assertable end-to-end).
- **More log collecting** *(new)*: new producers build a `LogRequest` and call
  `ISkyApmLogDispatcher.Dispatch` (the existing `→ LogReporter` path is unchanged). **Tests:** fake
  `ISkyApmLogDispatcher`, assert `LogData` fields + trace correlation; Layer-2 (logs persisted).
- **Public APIs** *(new)*: the manual span/metric/log facade (around `ITracingContext`). **Tests:**
  a dedicated `SkyApm.Api.Tests` contract suite — create spans/metrics/logs via the public API,
  assert captured output and trace-context propagation. (Keep separate from per-plugin tests.)

## 7. CI integration

Add an **integration job** (the workflow is already named `net-ci-it` but has no IT job today) —
either a new `plugin-it.yml` or a job in the existing CI:

- Matrix: `{plugin} × {libVersion} × {tfm: net8.0}`. Keep unit tests on net8.0 + net10.0; run the
  heavier container-backed IT on **net8.0** first (add net10.0 once its tooling stabilizes).
- Steps: checkout `submodules: recursive` → setup .NET 8/10 → `dotnet restore -p:LibVersion=<v>` →
  run the plugin's IT project (Testcontainers pulls backends) → (optional) Layer-2 scenario via the
  mock-collector compose.
- Mark the IT job **non-blocking/slower** initially; promote to required once stable. Pin any
  third-party action to a reviewed commit SHA (ASF allow-list); `actions/*` are always allowed.

## 8. Known hazards (from the code audit)

- **MassTransit plugin csproj** references `SkyAPM.Agent.Hosting` / `.Diagnostics.AspNetCore` /
  `.Utilities.DependencyInjection` as **published 2.0.0 NuGet packages**, not ProjectReferences.
  Its test build must override these to ProjectReferences (a `Directory.Build.targets`) so the test
  exercises *current* source.
- **EF Core provider sub-plugins** (Npgsql/Pomelo/Sqlite) carry **no** library `PackageReference` —
  they add peer formatters and rely on the consuming app to bring the provider; their tests must
  supply the provider explicitly.
- **net10.0 container tooling** is newer — start IT on net8.0 to reduce flakiness.

## 9. Phased rollout

1. **Harness** — `SkyApm.Testing.Harness` (capture-fakes + `AddSkyAPM` test host + span/log/meter
   assertion helpers). Add `Directory.Packages.props`.
2. **Pilot (2 plugins)** — EF Core (Sqlite, no backend) + MongoDB (Testcontainers) as the template;
   wire the `-p:LibVersion=` matrix in CI.
3. **Roll out** the remaining plugins one per PR, each with its `support-versions.txt`.
4. **Layer 2** — add the mock-collector compose + 1–2 `expectedData.yaml` e2e scenarios (traces,
   then meters + logs once those features land).
5. **New features** — add Meter/log/API tests alongside the features as they ship.

## 10. Decisions taken (no-ask defaults) & open questions

**Defaults chosen:** in-process Layer-1 is primary; Layer-2 reuses the SkyWalking mock-collector
unchanged (no .NET re-implementation); cross-version via MSBuild `-p:LibVersion` matrix +
Testcontainers; IT on net8.0 first.

**Open questions for review:** exact version list per plugin (lowest-supported → latest); whether
the new public APIs extend `ITracingContext` or a separate facade; whether `Transport.Kafka` gets
its own e2e leg; and whether to keep the myget `-vnext` prerelease feed.

[Testcontainers for .NET]: https://dotnet.testcontainers.org/
