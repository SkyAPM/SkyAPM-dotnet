SkyAPM C#/.NET instrument agent
==========

<img src="https://skyapmtest.github.io/page-resources/SkyAPM/skyapm.png" alt="Sky Walking logo" height="90px" align="right" />

[Apache SkyWalking](https://github.com/apache/incubator-skywalking) is an APM designed for microservices, cloud native and container-based (Docker, K8s, Mesos) architectures. **SkyAPM-dotnet** provides the native support agent in C# and .NETStandard platform, with the helps from Apache SkyWalking committer team.

[![issues](https://img.shields.io/github/issues-raw/skyapm/skyapm-dotnet.svg?style=flat-square)](https://github.com/SkyAPM/SkyAPM-dotnet/issues)
[![pulls](https://img.shields.io/github/issues-pr-raw/skyapm/skyapm-dotnet.svg?style=flat-square)](https://github.com/SkyAPM/SkyAPM-dotnet/pulls)
[![releases](https://img.shields.io/github/release/skyapm/skyapm-dotnet.svg?style=flat-square)](https://github.com/SkyAPM/SkyAPM-dotnet/releases)
[![Gitter](https://img.shields.io/gitter/room/openskywalking/lobby.svg?style=flat-square)](https://gitter.im/openskywalking/Lobby)
[![Twitter Follow](https://img.shields.io/twitter/follow/asfskywalking.svg?style=flat-square&label=Follow&logo=twitter)](https://twitter.com/AsfSkyWalking)

## CI Build Status

| Platform | Build Server | Master Status  |
|--------- |------------- |---------|
| AppVeyor |  Windows/Linux |[![Build status](https://ci.appveyor.com/api/projects/status/fl6vucwfn1vu94dv/branch/master?svg=true)](https://ci.appveyor.com/project/wu-sheng/skywalking-csharp/branch/master)|

## Nuget Packages

| Package Name |  NuGet | MyGet | Downloads 
|--------------|  ------- |  ------- |  ---- 
| SkyAPM.Agent.AspNetCore | [![nuget](https://img.shields.io/nuget/v/SkyAPM.Agent.AspNetCore.svg?style=flat-square)](https://www.nuget.org/packages/SkyAPM.Agent.AspNetCore) | [![myget](https://img.shields.io/myget/skyapm-dotnet/vpre/SkyAPM.Agent.AspNetCore.svg?style=flat-square)](https://www.myget.org/feed/skyapm-dotnet/package/nuget/SkyAPM.Agent.AspNetCore) | [![stats](https://img.shields.io/nuget/dt/SkyAPM.Agent.AspNetCore.svg?style=flat-square)](https://www.nuget.org/stats/packages/SkyAPM.Agent.AspNetCore?groupby=Version) 
| SkyAPM.Agent.AspNet | [![nuget](https://img.shields.io/nuget/v/SkyAPM.Agent.AspNet.svg?style=flat-square)](https://www.nuget.org/packages/SkyAPM.Agent.AspNet) | [![myget](https://img.shields.io/myget/skyapm-dotnet/vpre/SkyAPM.Agent.AspNet.svg?style=flat-square)](https://www.myget.org/feed/skyapm-dotnet/package/nuget/SkyAPM.Agent.AspNet) | [![](https://img.shields.io/nuget/dt/SkyAPM.Agent.AspNet.svg?style=flat-square)](https://www.nuget.org/stats/packages/SkyAPM.Agent.AspNet?groupby=Version)
| SkyAPM.Agent.GeneralHost | [![nuget](https://img.shields.io/nuget/v/SkyAPM.Agent.GeneralHost.svg?style=flat-square)](https://www.nuget.org/packages/SkyAPM.Agent.GeneralHost) | [![myget](https://img.shields.io/myget/skyapm-dotnet/vpre/SkyAPM.Agent.GeneralHost.svg?style=flat-square)](https://www.myget.org/feed/skyapm-dotnet/package/nuget/SkyAPM.Agent.GeneralHost) | [![](https://img.shields.io/nuget/dt/SkyAPM.Agent.GeneralHost.svg?style=flat-square)](https://www.nuget.org/stats/packages/SkyAPM.Agent.GeneralHost?groupby=Version)  

> MyGet feed URL https://www.myget.org/F/skyapm-dotnet/api/v3/index.json

# Supported
- This project currently supports apps targeting netcoreapp2.0/netframework4.6.1 or higher.
- [Supported middlewares, frameworks and libraries.](docs/Supported-list.md)

# Features
A quick list of SkyWalking .NET Core Agent's capabilities
- Application Topology
- Distributed Tracing
- ASP.NET Core Diagnostics
- HttpClient Diagnostics
- EntityFrameworkCore Diagnostics

# Getting Started

## Deploy SkyWalking Backend And UI

#### Requirements
Start with v1.0.0, SkyAPM .NET Core Agent only supports SkyWalking 8.0 or higher. The SkyWalking doc is [here](https://skywalking.apache.org/docs/). 

## Install SkyWalking .NET Core Agent

You can run the following command to install the SkyWalking .NET Core Agent in your project.

```
dotnet add package SkyAPM.Agent.AspNetCore
```

## How to use
Set the `ASPNETCORE_HOSTINGSTARTUPASSEMBLIES` environment variable to support the activation of the SkyAPM .NET Core Agent. 

- Add the assembly name of `SkyAPM.Agent.AspNetCore` to the `ASPNETCORE_HOSTINGSTARTUPASSEMBLIES` environment variable.

### Examples
- On windows

```
dotnet new mvc -n sampleapp
cd sampleapp
dotnet add package SkyAPM.Agent.AspNetCore
set ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=SkyAPM.Agent.AspNetCore
set SKYWALKING__SERVICENAME=sample_app
dotnet run
```

- On macOS/Linux

```
dotnet new mvc -n sampleapp
cd sampleapp
dotnet add package SkyAPM.Agent.AspNetCore
export ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=SkyAPM.Agent.AspNetCore
export SKYWALKING__SERVICENAME=sample_app
dotnet run
```

## Configuration

Install `SkyAPM.DotNet.CLI`

```
dotnet tool install -g SkyAPM.DotNet.CLI
```

Use `dotnet skyapm config [your_service_name] [your_servers]` to generate config file. 

```
dotnet skyapm config sample_app 192.168.0.1:11800
```

# Roadmap
[What are we going to do next?](/docs/roadmap.md)

# Contributing
This section is in progress here: [Contributing to SkyAPM-dotnet](/CONTIBUTING.md)

# Contact Us
* Submit an issue

If you have issues about SkyWalking protocol, its official backend, ask questions at their Apache official channels. All following channels are not suitable for .net agent, but good if you are facing backend/UI issues.
* Submit an official Apache SkyWalking [issue](https://github.com/apache/skywalking/issues). 
* Mail list: **dev@skywalking.apache.org**. Mail to `dev-subscribe@skywalking.apache.org`, follow the reply to subscribe the mail list.
* Join `skywalking` channel at [Apache Slack](https://join.slack.com/t/the-asf/shared_invite/enQtNzc2ODE3MjI1MDk1LTAyZGJmNTg1NWZhNmVmOWZjMjA2MGUyOGY4MjE5ZGUwOTQxY2Q3MDBmNTM5YTllNGU4M2QyMzQ4M2U4ZjQ5YmY). If the link is not working, find the latest one at [Apache INFRA WIKI](https://cwiki.apache.org/confluence/display/INFRA/Slack+Guest+Invites).
* QQ Group: 392443393(2000/2000, not available), 901167865(available)

# License
[Apache 2.0 License.](/LICENSE)
