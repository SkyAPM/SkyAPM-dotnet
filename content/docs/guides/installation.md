---
title: "Installation & Activation"
weight: 2
---
This guide explains how to attach the SkyAPM .NET agent to your application and how
instrumentation plugins are enabled. SkyAPM-dotnet is a C#/.NET auto-instrumentation
agent for the .NET ecosystem; it reports traces and logs to an Apache SkyWalking OAP backend over gRPC
(default `localhost:11800`) using the `sw8` / protocol `v8` wire format.

## Prerequisites

- A supported runtime: `net8.0` or `net10.0` (LTS). Foundational libraries also target
  `netstandard2.0`.
- A running SkyWalking OAP backend reachable on its agent gRPC port (default `11800`).
  The query/UI ports (`12800`/`8080`) are not used by the agent.
- A `skyapm.json` configuration file (or environment variables) that at minimum sets the
  service name. See [Configuration](skyapm_config.md) for the full schema.

> The agent no-ops (registers nothing) when `SkyWalking:ServiceName` is empty or when
> `SkyWalking:Enable` is `false`. Always set a service name before expecting data in the UI.

There are **three ways to attach the agent**, depending on your application model. Pick
exactly one host package. After attaching, a fixed set of plugins is enabled by default,
and additional plugins can be opted in through a setup lambda.

---

## 1. ASP.NET Core (zero-code)

Package: **`SkyApm.Agent.AspNetCore`**

This is the recommended path for ASP.NET Core apps and requires **no source-code changes**.
The package ships an `[assembly: HostingStartup]` that runs
`services.AddSkyAPM(ext => ext.AddAspNetCoreHosting())` during host startup, wiring inbound
HTTP entry-span instrumentation plus all default plugins.

Install the package:

```bash
dotnet add package SkyApm.Agent.AspNetCore
```

Activate it by listing the assembly in the `ASPNETCORE_HOSTINGSTARTUPASSEMBLIES`
environment variable:

```bash
export ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=SkyApm.Agent.AspNetCore
```

On Windows (PowerShell):

```bash
$Env:ASPNETCORE_HOSTINGSTARTUPASSEMBLIES = "SkyApm.Agent.AspNetCore"
```

Then add a `skyapm.json` (or `skyapm.{Environment}.json`) next to your app with at least
a service name:

```json
{
  "SkyWalking": {
    "ServiceName": "your_aspnetcore_app",
    "Transport": {
      "gRPC": {
        "Servers": "localhost:11800"
      }
    }
  }
}
```

That is all — your `Program.cs` stays untouched. The agent attaches at startup and begins
reporting.

> To add opt-in plugins without writing code, see the MassTransit note in
> [Plugins: default vs opt-in](#plugins-default-vs-opt-in); for everything else, use the
> manual/custom model below.

---

## 2. Generic Host — console, Worker, BackgroundService

Package: **`SkyApm.Agent.GeneralHost`**

For non-ASP.NET hosts built on `Microsoft.Extensions.Hosting` (console apps, Worker
Services, `BackgroundService`), call `AddSkyAPM()` on the `IHostBuilder`:

```bash
dotnet add package SkyApm.Agent.GeneralHost
```

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SkyApm.Agent.GeneralHost;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices(services => services.AddHostedService<Worker>())
            .AddSkyAPM();
}
```

`IHostBuilder.AddSkyAPM()` is a thin wrapper over `UseSkyAPM()`, which in turn calls
`services.AddSkyAPM()`. All default plugins are enabled; the same `skyapm.json` rules
apply. (See `sample/SkyApm.Sample.GenericHost/Program.cs` in the repository for a runnable
example.)

---

## 3. Manual / custom setup

Package: **`SkyApm.Agent.Hosting`**

`SkyApm.Agent.Hosting` is the shared core used by both host packages above. Reference it
directly when you need to control the service collection yourself or to **enable opt-in
plugins** through the setup lambda.

```bash
dotnet add package SkyApm.Agent.Hosting
```

Attach via the service collection (the lambda receives a `SkyApmExtensions` you can extend):

```csharp
using Microsoft.Extensions.DependencyInjection;
using SkyApm.Diagnostics.MongoDB;

services.AddSkyAPM(ext =>
{
    ext.AddMongoDB();   // opt-in plugin
});
```

A bare call (`services.AddSkyAPM()`) loads `skyapm.json` / `skyapm.{Environment}.json`
itself. To bind against your own `IConfiguration` instead, use the overload that takes it:

```csharp
services.AddSkyAPM(Configuration, ext =>
{
    ext.AddMongoDB();
});
```

Or attach at the `IHostBuilder` level (no opt-in plugins, defaults only):

```csharp
using SkyApm.Agent.Hosting;

hostBuilder.UseSkyAPM();
```

> The opt-in setup lambda is only available through `services.AddSkyAPM(...)`. The ASP.NET
> Core zero-code host always uses `ext => ext.AddAspNetCoreHosting()` internally, so to add
> opt-in plugins in an ASP.NET Core app, call `services.AddSkyAPM(ext => { ... })` yourself
> from `SkyApm.Agent.Hosting` instead of relying solely on the env-var activation.

---

## Plugins: default vs opt-in

When any host attaches, a fixed set of **default plugins** is registered automatically — you
do not list them. The setup lambda is then invoked so you can add **opt-in plugins** on top.

### Default plugins (always on)

These are wired by `AddSkyAPM` for every host:

| Plugin | Instruments |
| --- | --- |
| HttpClient | Outbound HTTP exit spans (with `sw8` propagation) |
| SqlClient | ADO.NET database spans |
| Grpc (server) | Inbound gRPC spans |
| Grpc.Net.Client | Outbound gRPC spans |
| EntityFrameworkCore | EF Core database spans (providers: Pomelo MySQL, Npgsql, SQLite) |
| MSLogging | Ships `Microsoft.Extensions.Logging` records to OAP with trace context |
| SqlClient / MySqlConnector peer formatters | Resolve DB peer addresses for SQL spans |

The ASP.NET Core host additionally registers **AspNetCore** (`AddAspNetCoreHosting`), which
produces inbound HTTP entry spans, captures cookies/headers/body per configuration, and
honors `IgnorePaths`.

### Opt-in plugins (enable in the setup lambda)

Add these inside `services.AddSkyAPM(ext => { ... })`. Each requires its own NuGet package.

```csharp
using Microsoft.Extensions.DependencyInjection;
using SkyApm.Diagnostics.CAP;
using SkyApm.Diagnostics.MongoDB;
using SkyApm.Diagnostics.MassTransit;
using SkyApm.Diagnostics.SmartSql;
using SkyApm.Diagnostics.FreeSql;
using SkyApm.Diagnostics.FreeRedis;

services.AddSkyAPM(ext =>
{
    ext.AddCap();              // CAP message-bus spans + cross-message sw8
    ext.AddMongoDB();          // MongoDB.Driver command spans
    ext.AddMasstransit();      // MassTransit message-bus spans + cross-message sw8
    ext.AddSmartSql();         // SmartSql ORM SQL spans
    ext.AddFreeSql(fsql);      // FreeSql ORM SQL spans — pass your IFreeSql instance
    ext.AddFreeRedis(client);  // FreeRedis Redis command spans — pass your RedisClient
});
```

Two opt-in plugins take an instance you already own:

- **FreeSql** — `ext.AddFreeSql(IFreeSql fsql)` requires the `IFreeSql` instance you build
  for your application, so the agent can hook into the same connection.
- **FreeRedis** — `ext.AddFreeRedis(RedisClient redisClient, bool includeAuth = false)`
  requires your `RedisClient`. Set `includeAuth: true` only if you want the (sensitive) auth
  command included in spans.

```csharp
IFreeSql fsql = new FreeSql.FreeSqlBuilder()
    .UseConnectionString(FreeSql.DataType.MySql, connectionString)
    .Build();

var redisClient = new FreeRedis.RedisClient("127.0.0.1:6379");

services.AddSkyAPM(ext =>
{
    ext.AddFreeSql(fsql);
    ext.AddFreeRedis(redisClient, includeAuth: false);
});
```

### MassTransit special case

The MassTransit plugin (`SkyApm.Diagnostics.MassTransit`) ships its **own**
`[assembly: HostingStartup]`. In an ASP.NET Core app you can therefore enable it with
**zero code**: install the package and add it to the activation env var alongside the host —

```bash
export ASPNETCORE_HOSTINGSTARTUPASSEMBLIES="SkyApm.Agent.AspNetCore;SkyApm.Diagnostics.MassTransit"
```

Its hosting startup sets `Activity.DefaultIdFormat` to W3C, listens on the `MassTransit`
`ActivitySource`, and calls `ext.AddAspNetCoreHosting()` + `ext.AddMasstransit()` for you.
Outside ASP.NET Core, call `ext.AddMasstransit()` in the setup lambda as shown above.

---

## Next steps

- [Plugins](plugins.md) — the full plugin catalog and what each one captures.
- [Configuration](skyapm_config.md) — `skyapm.json` schema, sampling, transport, and the
  environment-variable overrides (e.g. `SKYWALKING__TRANSPORT__GRPC__SERVERS`).
