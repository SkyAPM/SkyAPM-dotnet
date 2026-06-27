---
title: "Supported Components"
weight: 2
---
This page lists the libraries and frameworks that the SkyAPM .NET agent can
auto-instrument, what each one captures, and whether it is enabled by default or
must be opted in. SkyAPM-dotnet is the C#/.NET auto-instrumentation agent for
Apache SkyWalking; it reports to the SkyWalking OAP over gRPC using the **sw8 / v8**
protocol only.

For installation and wiring details, see the [plugins guide](guides/plugins.md) and
[Configuration](guides/skyapm_config.md).

## How instrumentation is enabled

The agent ships a set of diagnostic plugins. Some are registered automatically for
every instrumented host; others are opt-in and must be enabled through the
`AddSkyAPM` setup lambda.

- **On by default** — registered as soon as the agent is active on a host. For
  ASP.NET Core, activation is zero-code via
  `ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=SkyApm.Agent.AspNetCore`. The agent no-ops if
  `SkyWalking:ServiceName` is empty or `SkyWalking:Enable` is `false`.
- **Opt-in** — enabled by calling the matching extension method inside the
  `AddSkyAPM` lambda:

```csharp
services.AddSkyAPM(ext => ext
    .AddCap()
    .AddMongoDB()
    .AddSmartSql()
    .AddFreeSql(fsql)                       // requires your IFreeSql instance
    .AddFreeRedis(redisClient)              // requires your RedisClient instance
    .AddMasstransit());
```

## Default (on by default)

These plugins are registered automatically by the agent host and require no extra
code.

| Component | Instruments | Upstream project |
| --- | --- | --- |
| **ASP.NET Core** | Inbound HTTP entry spans; captures cookies/headers/body per config and honors `IgnorePaths`. Added by the ASP.NET Core host (`AddAspNetCoreHosting`). | [dotnet/aspnetcore](https://github.com/dotnet/aspnetcore) |
| **HttpClient** | Outbound HTTP exit spans with `sw8` header propagation (`HttpClient` / `IHttpClientFactory`). | [dotnet/runtime](https://github.com/dotnet/runtime) |
| **ADO.NET SqlClient** | Database exit spans for `System.Data.SqlClient` and `Microsoft.Data.SqlClient`. Includes peer formatters (`SkyApm.PeerFormatters.SqlClient`, `SkyApm.PeerFormatters.MySqlConnector`). | [System.Data.SqlClient](https://github.com/dotnet/runtime), [Microsoft.Data.SqlClient](https://github.com/dotnet/SqlClient) |
| **gRPC server** | Inbound gRPC entry spans for `Grpc.AspNetCore` / `Grpc.Core` (`AddGrpc`). | [grpc/grpc-dotnet](https://github.com/grpc/grpc-dotnet) |
| **gRPC client** | Outbound gRPC exit spans for `Grpc.Net.Client` (`AddGrpcClient`). | [grpc/grpc-dotnet](https://github.com/grpc/grpc-dotnet) |
| **EntityFrameworkCore** | EF Core database spans (`AddEntityFrameworkCore`). | [dotnet/efcore](https://github.com/dotnet/efcore) |
| &nbsp;&nbsp;↳ Pomelo MySQL provider | EF Core spans for `Pomelo.EntityFrameworkCore.MySql`. | [PomeloFoundation/Pomelo.EntityFrameworkCore.MySql](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql) |
| &nbsp;&nbsp;↳ Npgsql / PostgreSQL provider | EF Core spans for `Npgsql.EntityFrameworkCore.PostgreSQL`. | [npgsql/Npgsql.EntityFrameworkCore.PostgreSQL](https://github.com/npgsql/Npgsql.EntityFrameworkCore.PostgreSQL) |
| &nbsp;&nbsp;↳ Sqlite provider | EF Core spans for the SQLite provider. | [dotnet/efcore](https://github.com/dotnet/efcore) |
| **Microsoft.Extensions.Logging** | Ships `Microsoft.Extensions.Logging` records to the SkyWalking OAP with trace context (`AddMSLogging`). | [dotnet/runtime](https://github.com/dotnet/runtime) |

## Opt-in

Enable these by calling the corresponding method in the `AddSkyAPM` lambda.

| Component | Setup call | Instruments | Upstream project |
| --- | --- | --- | --- |
| **CAP** | `ext.AddCap()` | Message-bus spans with cross-message `sw8` propagation. | [dotnetcore/CAP](https://github.com/dotnetcore/CAP) |
| **MassTransit** | `ext.AddMasstransit()` | Message-bus spans with cross-message `sw8` propagation. Also ships its own `[assembly:HostingStartup]`: adding the package and listing it in `ASPNETCORE_HOSTINGSTARTUPASSEMBLIES` auto-wires it, switches `Activity` to W3C id format, and listens on the `MassTransit` `ActivitySource`. | [MassTransit/MassTransit](https://github.com/MassTransit/MassTransit) |
| **MongoDB** | `ext.AddMongoDB()` | `MongoDB.Driver` command spans. | [mongodb/mongo-csharp-driver](https://github.com/mongodb/mongo-csharp-driver) |
| **SmartSql** | `ext.AddSmartSql()` | ORM SQL spans. | [dotnetcore/SmartSql](https://github.com/dotnetcore/SmartSql) |
| **FreeSql** | `ext.AddFreeSql(fsql)` — requires your `IFreeSql` instance | ORM SQL spans. | [dotnetcore/FreeSql](https://github.com/dotnetcore/FreeSql) |
| **FreeRedis** | `ext.AddFreeRedis(redisClient, includeAuth: false)` — requires your `RedisClient` instance | Redis command spans. | [2881099/FreeRedis](https://github.com/2881099/FreeRedis) |

## Notes

- **Classic ASP.NET (System.Web) is not instrumented.** There is no dedicated plugin
  for the classic, non-Core ASP.NET pipeline; only ASP.NET Core is supported for
  inbound HTTP.
- **Protocol.** The agent supports the SkyWalking **sw8 / v8** protocol only. It
  reports to the OAP gRPC endpoint on port `11800` (the query/UI ports `12800`/`8080`
  are not used by the agent).
- **Configuration.** Plugin behavior (sampling, ignored paths, captured
  headers/body, transport servers, and so on) is driven by `skyapm.json` and the
  other configuration sources. See [Configuration](guides/skyapm_config.md) for the full
  reference.
