SkyAPM C#/.NET instrument agent
==========

<img src="https://skyapmtest.github.io/page-resources/SkyAPM/skyapm.png" alt="SkyAPM logo" height="90px" align="right" />

**SkyAPM-dotnet** is a community, open-source C#/.NET auto-instrumentation agent for the .NET ecosystem. It provides distributed tracing, application topology, and metrics for ASP.NET Core and .NET applications, and reports the collected telemetry to an [Apache SkyWalking](https://skywalking.apache.org/) backend over the `sw8` / `v8` protocol. It is an independent project and is not affiliated with or endorsed by the Apache Software Foundation.

[![issues](https://img.shields.io/github/issues-raw/skyapm/skyapm-dotnet.svg?style=flat-square)](https://github.com/SkyAPM/SkyAPM-dotnet/issues)
[![pulls](https://img.shields.io/github/issues-pr-raw/skyapm/skyapm-dotnet.svg?style=flat-square)](https://github.com/SkyAPM/SkyAPM-dotnet/pulls)
[![releases](https://img.shields.io/github/release/skyapm/skyapm-dotnet.svg?style=flat-square)](https://github.com/SkyAPM/SkyAPM-dotnet/releases)
[![Twitter Follow](https://img.shields.io/twitter/follow/asfskywalking.svg?style=flat-square&label=Follow&logo=twitter)](https://twitter.com/AsfSkyWalking)

## 📖 Documentation

**Full documentation lives at [skyapm.github.io/SkyAPM-dotnet](https://skyapm.github.io/SkyAPM-dotnet/)** — getting started, installation & activation, the complete configuration reference (EN / 中文), transports (gRPC / Kafka), the supported-component list, plugins, logging, the CLI, and troubleshooting.

## CI Build Status

[![NET CI AND IT](https://github.com/SkyAPM/SkyAPM-dotnet/actions/workflows/net-ci-it.yml/badge.svg)](https://github.com/SkyAPM/SkyAPM-dotnet/actions/workflows/net-ci-it.yml)

## NuGet Packages

| Package Name |  NuGet | Downloads |
|--------------|  ------- |  ---- |
| SkyAPM.Agent.AspNetCore | [![nuget](https://img.shields.io/nuget/v/SkyAPM.Agent.AspNetCore.svg?style=flat-square)](https://www.nuget.org/packages/SkyAPM.Agent.AspNetCore) | [![stats](https://img.shields.io/nuget/dt/SkyAPM.Agent.AspNetCore.svg?style=flat-square)](https://www.nuget.org/stats/packages/SkyAPM.Agent.AspNetCore?groupby=Version) |
| SkyAPM.Agent.GeneralHost | [![nuget](https://img.shields.io/nuget/v/SkyAPM.Agent.GeneralHost.svg?style=flat-square)](https://www.nuget.org/packages/SkyAPM.Agent.GeneralHost) | [![stats](https://img.shields.io/nuget/dt/SkyAPM.Agent.GeneralHost.svg?style=flat-square)](https://www.nuget.org/stats/packages/SkyAPM.Agent.GeneralHost?groupby=Version) |

## Quick start

Supported runtimes: **net8.0** and **net10.0**.

```bash
dotnet add package SkyApm.Agent.AspNetCore
export ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=SkyApm.Agent.AspNetCore
export SKYWALKING__SERVICENAME=my_service
dotnet run
```

See [**Getting Started**](https://skyapm.github.io/SkyAPM-dotnet/docs/guides/getting-started/) for the full walkthrough, and the [**Configuration reference**](https://skyapm.github.io/SkyAPM-dotnet/docs/guides/skyapm_config/) for every option.

## Contributing

See [Contributing to SkyAPM-dotnet](./CONTRIBUTING.md).

## Contact Us

* Submit an issue on this repository for questions about the **.NET agent**.

For questions about the **SkyWalking protocol or its backend/UI**, use the official Apache SkyWalking channels (these are not suitable for the .NET agent):
* Submit an official Apache SkyWalking [issue](https://github.com/apache/skywalking/issues).
* Mail list: **dev@skywalking.apache.org**. Mail to `dev-subscribe@skywalking.apache.org`, follow the reply to subscribe the mail list.
* Send `Request to join SkyWalking slack` mail to the mail list (`dev@skywalking.apache.org`), we will invite you in.
* For Chinese speaker, send `[CN] Request to join SkyWalking slack` mail to the mail list (`dev@skywalking.apache.org`), we will invite you in.

## License

[Apache 2.0 License.](/LICENSE)
