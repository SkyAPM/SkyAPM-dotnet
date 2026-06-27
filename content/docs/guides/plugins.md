---
title: "Plugins"
weight: 6
---

# Plugins

The SkyAPM .NET agent ships **16 diagnostic plugins** that auto-instrument common
frameworks and libraries. Each plugin observes a `DiagnosticListener` / `ActivitySource`
(or registers an interceptor) and produces SkyWalking **sw8 / v8** spans that the agent
reports to the SkyWalking OAP backend over gRPC (`localhost:11800` by default).

This guide covers every plugin: its NuGet package, what it instruments, whether it is
**default-on** or **opt-in**, and how to enable the opt-in ones.

For setup and activation, see [Installation](installation.md). For the full framework
matrix, see [Supported list](../Supported-list.md). For configuration keys, see
[Configuration](skyapm_config.md).

## How activation works

The agent is bootstrapped through `services.AddSkyAPM(...)`. For ASP.NET Core this is wired
zero-code by the host package, which you activate with the environment variable:

```bash
ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=SkyApm.Agent.AspNetCore
```

`AddSkyAPM` no-ops unless `SkyWalking:ServiceName` is set and `SkyWalking:Enable` is not
`false`. When it runs, it registers the **default-on** plugins automatically. The
**opt-in** plugins are added through the `extensionsSetup` lambda parameter — a
`SkyApmExtensions` builder:

```csharp
// Program.cs — add opt-in plugins through the AddSkyAPM setup lambda.
builder.Services.AddSkyAPM(ext =>
{
    ext.AddMongoDB();
    ext.AddCap();
    // ...other opt-in plugins
});
```

> If you rely on the zero-code `ASPNETCORE_HOSTINGSTARTUPASSEMBLIES` activation, the
> AspNetCore host calls `AddSkyAPM(ext => ext.AddAspNetCoreHosting())` for you. To pass
> your own opt-in plugins you call `AddSkyAPM(...)` explicitly in `Program.cs` (its
> registrations are idempotent for the defaults), or use a `GeneralHost` for non-web apps.

Each extension method below returns the same `SkyApmExtensions` instance, so calls are
chainable.

## Plugin matrix

| Plugin | Package | Default-on | Enable |
| --- | --- | --- | --- |
| ASP.NET Core | `SkyAPM.Diagnostics.AspNetCore` | Yes (added by AspNetCore host) | `ext.AddAspNetCoreHosting()` |
| HttpClient | `SkyAPM.Diagnostics.HttpClient` | Yes | `ext.AddHttpClient()` |
| SqlClient (ADO.NET) | `SkyAPM.Diagnostics.SqlClient` | Yes | `ext.AddSqlClient()` |
| Entity Framework Core | `SkyAPM.Diagnostics.EntityFrameworkCore` | Yes | `ext.AddEntityFrameworkCore(c => ...)` |
| EF Core — Pomelo MySQL | `SkyAPM.Diagnostics.EntityFrameworkCore.Pomelo.MySql` | Yes | `c.AddPomeloMysql()` |
| EF Core — Npgsql | `SkyAPM.Diagnostics.EntityFrameworkCore.Npgsql` | Yes | `c.AddNpgsql()` |
| EF Core — Sqlite | `SkyAPM.Diagnostics.EntityFrameworkCore.Sqlite` | Yes | `c.AddSqlite()` |
| gRPC server | `SkyAPM.Diagnostics.Grpc` | Yes | `ext.AddGrpc()` |
| Grpc.Net.Client | `SkyAPM.Diagnostics.Grpc.Net.Client` | Yes | `ext.AddGrpcClient()` |
| Microsoft.Extensions.Logging | `SkyAPM.Diagnostics.MSLogging` | Yes | `ext.AddMSLogging()` |
| SqlClient peer formatter | `SkyAPM.PeerFormatters.SqlClient` | Yes | `ext.AddSqlClientPeerFormatter()` |
| MySqlConnector peer formatter | `SkyAPM.PeerFormatters.MySqlConnector` | Yes | `ext.AddMySqlConnectorPeerFormatter()` |
| CAP | `SkyAPM.Diagnostics.CAP` | No | `ext.AddCap()` |
| MongoDB | `SkyAPM.Diagnostics.MongoDB` | No | `ext.AddMongoDB()` |
| SmartSql | `SkyAPM.Diagnostics.SmartSql` | No | `ext.AddSmartSql()` |
| FreeSql | `SkyAPM.Diagnostics.FreeSql` | No | `ext.AddFreeSql(IFreeSql)` |
| FreeRedis | `SkyAPM.Diagnostics.FreeRedis` | No | `ext.AddFreeRedis(RedisClient, includeAuth)` |
| MassTransit | `SkyAPM.Diagnostics.MassTransit` | No | `ext.AddMasstransit()` (or its own HostingStartup) |

The default-on plugins are wired automatically by `AddSkyAPM`:

```csharp
// Registered for you whenever AddSkyAPM runs.
ext.AddHttpClient()
   .AddGrpcClient()
   .AddSqlClient()
   .AddGrpc()
   .AddEntityFrameworkCore(c => c.AddPomeloMysql().AddNpgsql().AddSqlite())
   .AddMSLogging()
   .AddSqlClientPeerFormatter()
   .AddMySqlConnectorPeerFormatter();
```

(That table counts 18 packages but **16 diagnostic plugins** — the two
`SkyAPM.PeerFormatters.*` packages are helpers, not standalone instrumentation; see
[Peer formatters](#peer-formatters).)

---

## Default-on plugins

These are registered automatically by `AddSkyAPM`. Add the corresponding NuGet package to
your project (or it comes transitively with the host package) and the spans appear with no
code change.

### ASP.NET Core

- **Package:** `SkyAPM.Diagnostics.AspNetCore`
- **Instruments:** inbound HTTP entry spans for ASP.NET Core requests. Captures
  cookies / headers / request body according to configuration, and honors the configured
  `IgnorePaths` so excluded routes are not traced.
- **Enable:** `ext.AddAspNetCoreHosting()` — added for you by the `SkyApm.Agent.AspNetCore`
  host when you set `ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=SkyApm.Agent.AspNetCore`.

### HttpClient

- **Package:** `SkyAPM.Diagnostics.HttpClient`
- **Instruments:** outbound HTTP exit spans for `HttpClient` calls, and injects the
  **sw8** propagation header so the downstream service continues the same trace.
- **Enable:** `ext.AddHttpClient()` (default).

### SqlClient (ADO.NET)

- **Package:** `SkyAPM.Diagnostics.SqlClient`
- **Instruments:** ADO.NET database spans (`System.Data.SqlClient` /
  `Microsoft.Data.SqlClient`) for SQL command execution.
- **Enable:** `ext.AddSqlClient()` (default).

### Entity Framework Core

- **Package:** `SkyAPM.Diagnostics.EntityFrameworkCore`
- **Instruments:** EF Core database spans for queries and command execution.
- **Enable:** `ext.AddEntityFrameworkCore(c => ...)` (default). The provider sub-plugins
  resolve the database **peer** so spans target the right logical DB. They are all
  registered by default:

```csharp
ext.AddEntityFrameworkCore(c => c
    .AddPomeloMysql()   // SkyAPM.Diagnostics.EntityFrameworkCore.Pomelo.MySql
    .AddNpgsql()        // SkyAPM.Diagnostics.EntityFrameworkCore.Npgsql
    .AddSqlite());      // SkyAPM.Diagnostics.EntityFrameworkCore.Sqlite
```

Add only the provider you use if you trim the default set; each lives in its own package
(`SkyAPM.Diagnostics.EntityFrameworkCore.{Pomelo.MySql|Npgsql|Sqlite}`).

### gRPC server

- **Package:** `SkyAPM.Diagnostics.Grpc`
- **Instruments:** inbound and outbound gRPC calls handled via the `Grpc.Core`
  client/server interceptors.
- **Enable:** `ext.AddGrpc()` (default).

### Grpc.Net.Client

- **Package:** `SkyAPM.Diagnostics.Grpc.Net.Client`
- **Instruments:** outbound gRPC exit spans for the modern `Grpc.Net.Client` HTTP/2 client.
- **Enable:** `ext.AddGrpcClient()` (default).

### Microsoft.Extensions.Logging

- **Package:** `SkyAPM.Diagnostics.MSLogging`
- **Instruments:** ships `Microsoft.Extensions.Logging` records to the SkyWalking OAP as
  logs, correlated with the active trace context (trace / segment IDs) so logs link to
  spans in the UI.
- **Enable:** `ext.AddMSLogging()` (default).

### Peer formatters

The two `SkyAPM.PeerFormatters.*` packages are **helpers** for the SqlClient and EF Core
plugins, not standalone instrumentation. They normalize a database connection into the
SkyWalking **peer** (host:port) shown on DB spans and the topology map:

- **`SkyAPM.PeerFormatters.SqlClient`** — `ext.AddSqlClientPeerFormatter()` (default).
- **`SkyAPM.PeerFormatters.MySqlConnector`** — `ext.AddMySqlConnectorPeerFormatter()`
  (default). Use this when your MySQL access goes through the `MySqlConnector` driver.

Both are registered by `AddSkyAPM` automatically.

---

## Opt-in plugins

These are **not** registered by default. Add the NuGet package and enable the plugin in
the `AddSkyAPM` setup lambda.

### CAP

- **Package:** `SkyAPM.Diagnostics.CAP`
- **Instruments:** [CAP](https://github.com/dotnetcore/CAP) message-bus spans for publish
  and consume, propagating **sw8** across messages so producer and consumer share a trace.
- **Enable:**

```csharp
builder.Services.AddSkyAPM(ext => ext.AddCap());
```

### MongoDB

- **Package:** `SkyAPM.Diagnostics.MongoDB`
- **Instruments:** `MongoDB.Driver` command execution spans.
- **Enable:**

```csharp
builder.Services.AddSkyAPM(ext => ext.AddMongoDB());
```

### SmartSql

- **Package:** `SkyAPM.Diagnostics.SmartSql`
- **Instruments:** SmartSql ORM SQL execution spans.
- **Enable:**

```csharp
builder.Services.AddSkyAPM(ext => ext.AddSmartSql());
```

### FreeSql

- **Package:** `SkyAPM.Diagnostics.FreeSql`
- **Instruments:** [FreeSql](https://github.com/dotnetcore/FreeSql) ORM SQL execution
  spans.
- **Requires a live instance:** `AddFreeSql` takes your configured `IFreeSql` so the plugin
  can attach its AOP/diagnostic hooks to that exact instance. Build the `IFreeSql` first,
  then pass it in.
- **Enable:**

```csharp
using FreeSql;

// 1. Build your IFreeSql instance.
IFreeSql fsql = new FreeSqlBuilder()
    .UseConnectionString(DataType.MySql, builder.Configuration.GetConnectionString("Default"))
    .UseAutoSyncStructure(true)
    .Build();

// 2. Register it for your app's own use.
builder.Services.AddSingleton(fsql);

// 3. Hand the SAME instance to the SkyAPM plugin so it can instrument it.
builder.Services.AddSkyAPM(ext => ext.AddFreeSql(fsql));
```

> The plugin instruments only the `IFreeSql` instance you pass. If you create multiple
> `IFreeSql` instances, call `AddFreeSql` for the ones you want traced.

### FreeRedis

- **Package:** `SkyAPM.Diagnostics.FreeRedis`
- **Instruments:** [FreeRedis](https://github.com/2881099/FreeRedis) Redis command spans.
- **Requires a live instance:** `AddFreeRedis(RedisClient redisClient, bool includeAuth = false)`
  takes your configured `RedisClient`. Set `includeAuth: true` only if you want credential
  details from the connection string included on the span's peer (off by default to avoid
  leaking secrets).
- **Enable:**

```csharp
using FreeRedis;

// 1. Build your RedisClient instance.
var redisClient = new RedisClient("127.0.0.1:6379,password=,defaultDatabase=0");

// 2. Register it for your app's own use.
builder.Services.AddSingleton(redisClient);

// 3. Hand the SAME instance to the SkyAPM plugin (includeAuth defaults to false).
builder.Services.AddSkyAPM(ext => ext.AddFreeRedis(redisClient));

// Include auth info on the peer only if you understand the exposure:
// builder.Services.AddSkyAPM(ext => ext.AddFreeRedis(redisClient, includeAuth: true));
```

### MassTransit

- **Package:** `SkyAPM.Diagnostics.MassTransit`
- **Instruments:** [MassTransit](https://masstransit.io/) message-bus spans (publish, send,
  consume, receive) by listening on the `MassTransit` `ActivitySource`, propagating **sw8**
  across messages.
- **Dual activation.** This plugin can be enabled two ways:

  1. **Its own HostingStartup (zero-code).** The package ships
     `[assembly: HostingStartup(typeof(MasstransitSkyApmHostingStartup))]`. Just add the
     package and list it in the activation assemblies:

     ```bash
     ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=SkyApm.Agent.AspNetCore;SkyApm.Diagnostics.MassTransit
     ```

     Its `HostingStartup` sets the process `Activity` ID format to **W3C**
     (`Activity.DefaultIdFormat = W3C`, `ForceDefaultIdFormat = true`), registers an
     `ActivityListener` for the `"MassTransit"` source, and calls
     `AddSkyAPM(ext => { ext.AddAspNetCoreHosting(); ext.AddMasstransit(); })` for you.

  2. **Explicit registration.** Call it yourself in the setup lambda:

     ```csharp
     builder.Services.AddSkyAPM(ext => ext.AddMasstransit());
     ```

> Because the HostingStartup forces W3C activity IDs, prefer the zero-code path for
> MassTransit so trace IDs line up across the bus. If you register explicitly, the bus
> still needs W3C activity IDs to correlate correctly.

---

## Quick reference: full opt-in example

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

IFreeSql fsql = new FreeSql.FreeSqlBuilder()
    .UseConnectionString(FreeSql.DataType.Sqlite, "Data Source=app.db")
    .Build();
builder.Services.AddSingleton(fsql);

var redisClient = new FreeRedis.RedisClient("127.0.0.1:6379");
builder.Services.AddSingleton(redisClient);

builder.Services.AddSkyAPM(ext =>
{
    ext.AddCap();
    ext.AddMongoDB();
    ext.AddSmartSql();
    ext.AddFreeSql(fsql);
    ext.AddFreeRedis(redisClient);          // includeAuth: false by default
    ext.AddMasstransit();                    // or use the MassTransit HostingStartup
});

var app = builder.Build();
app.Run();
```

The default-on plugins (ASP.NET Core, HttpClient, SqlClient, EF Core + providers, gRPC,
Grpc.Net.Client, MSLogging, peer formatters) are added by `AddSkyAPM` regardless of the
lambda above.

## See also

- [Installation](installation.md) — install the agent and activate it.
- [Configuration](skyapm_config.md) — `skyapm.json` keys (`ServiceName`, `Transport.gRPC`,
  `Sampling`, `IgnorePaths`, ...).
- [Supported list](../Supported-list.md) — the full supported-framework matrix.
