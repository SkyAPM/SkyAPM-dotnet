---
title: "Getting Started"
weight: 1
---

# Getting Started

This quickstart walks you through instrumenting an ASP.NET Core application with the
SkyAPM .NET agent end to end: stand up a SkyWalking OAP backend, install the package,
generate a config file, activate the agent with a single environment variable, then send
traffic and watch your service appear in the SkyWalking UI.

The SkyAPM .NET agent is the C#/.NET auto-instrumentation agent for
[Apache SkyWalking](https://skywalking.apache.org/). It targets `net8.0` and `net10.0`
(the active .NET LTS releases) and reports over the SkyWalking `v8` protocol (`sw8`
headers) only.

## Prerequisites

- The .NET SDK (8.0 or 10.0).
- Docker, or another way to run the SkyWalking OAP backend.

## 1. Run a SkyWalking OAP backend

The agent reports trace and metric data to a SkyWalking OAP server over gRPC on port
`11800`. The OAP also exposes the GraphQL query port (`12800`) that backs the web UI on
port `8080`.

The quickest way to get a backend and UI running locally is Docker Compose with the
official `apache/skywalking-oap-server` and `apache/skywalking-ui` images:

```bash
git clone https://github.com/apache/skywalking.git
cd skywalking/docker
docker compose up
```

Once it is up:

- OAP gRPC (agent reporting): `localhost:11800`
- SkyWalking UI: <http://localhost:8080>

Backend setup is intentionally brief here. For production deployments, storage options,
and configuration details, see the official
[SkyWalking documentation](https://skywalking.apache.org/docs/).

> Note: The SkyAPM .NET agent supports SkyWalking 8.0 or higher only.

## 2. Install the agent package

Create (or open) an ASP.NET Core project and add the agent package:

```bash
dotnet new mvc -n sampleapp
cd sampleapp
dotnet add package SkyApm.Agent.AspNetCore
```

The `SkyApm.Agent.AspNetCore` package wires the agent into the ASP.NET Core host through
a zero-code `IHostingStartup`, so no code changes are required. For other host types and
package options, see [Installation](installation.md).

## 3. Generate the config file

Install the SkyAPM CLI tool (once per machine):

```bash
dotnet tool install -g SkyApm.DotNet.CLI
```

Then generate a `skyapm.json` in the project directory, passing your service name and the
OAP gRPC address:

```bash
dotnet skyapm config sample_app --grpcservers localhost:11800
```

This creates `skyapm.json` next to your project. A few defaults worth knowing:

- `ServiceName` is set to the name you passed (`sample_app`).
- `HeaderVersions` is `["sw8"]` and `Transport.ProtocolVersion` is `"v8"`.
- `Transport.gRPC.Servers` is the address you passed (multiple addresses are
  comma-separated).

```json
{
  "SkyWalking": {
    "Enable": "true",
    "ServiceName": "sample_app",
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
    "Transport": {
      "ProtocolVersion": "v8",
      "QueueSize": 10000,
      "BatchSize": 2000,
      "Reporter": "grpc",
      "gRPC": {
        "Servers": "localhost:11800",
        "Timeout": 10000,
        "ConnectTimeout": 10000,
        "ReportTimeout": 600000,
        "Authentication": ""
      }
    }
  }
}
```

For all CLI options (including the Kafka reporter and per-environment files), see
[CLI](cli.md). For the meaning of every config field, see
[Configuration](skyapm_config.md).

## 4. Activate the agent

The agent is activated by setting the standard ASP.NET Core hosting-startup environment
variable so the runtime loads the agent assembly:

```
ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=SkyApm.Agent.AspNetCore
```

The service name must also be set. The CLI already wrote it into `skyapm.json`, but you
can override it (or set it without a config file) via the `SKYWALKING__SERVICENAME`
environment variable. The agent no-ops if the service name is empty or `SkyWalking:Enable`
is `false`.

On macOS/Linux:

```bash
export ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=SkyApm.Agent.AspNetCore
export SKYWALKING__SERVICENAME=sample_app
```

On Windows (Command Prompt):

```bash
set ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=SkyApm.Agent.AspNetCore
set SKYWALKING__SERVICENAME=sample_app
```

> Configuration is layered. The agent reads `skyapm.json` (and
> `skyapm.{Environment}.json`), as well as `appsettings.json`, environment variables
> (double-underscore syntax, e.g. `SKYWALKING__TRANSPORT__GRPC__SERVERS`), and the host
> `IConfiguration`. Environment variables and later sources override the file, so the
> `SKYWALKING__*` variables above take precedence over `skyapm.json`.

## 5. Run, generate traffic, and view your service

Start the app:

```bash
dotnet run
```

Generate some traffic by hitting an endpoint a few times (open it in a browser or use
`curl`):

```bash
curl http://localhost:5000/
```

Then open the SkyWalking UI at <http://localhost:8080>. Within a minute or two, your
service (`sample_app`) appears under **General Service**, where you can see its topology,
traces, and metrics.

> Tip: SkyWalking aggregates metrics on a fixed interval, so the dashboard may take up to
> a minute to populate after the first requests arrive. Make sure the UI time range covers
> "now".

## A minimal working example

A default `dotnet new mvc` app already serves traffic, but here is a minimal ASP.NET Core
app that exposes a single endpoint to instrument. No SkyAPM API calls are needed — the
agent instruments the incoming HTTP request automatically once activated.

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello from a SkyAPM-instrumented service!");

app.Run();
```

Run it with the agent activated:

```bash
export ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=SkyApm.Agent.AspNetCore
export SKYWALKING__SERVICENAME=sample_app
dotnet run
```

Each request to `/` produces a trace segment reported to the OAP backend and visible in
the SkyWalking UI.

## Next steps

- [Installation](installation.md) — package options and supported host types.
- [Configuration](skyapm_config.md) — every config field explained.
- [CLI](cli.md) — full `dotnet skyapm` command reference.
- [Docs index](/docs/) — all guides.
