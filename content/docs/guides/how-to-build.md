# How to build project

This document helps people to compile and build the SkyAPM-dotnet project from source.

## Build Project

**Because we use a Git submodule, we recommend you do NOT use the `GitHub` tag or release page to download source codes for compiling.**

### Build from GitHub

- Prepare git and the .NET 8 SDK or newer. The SkyAPM .NET agent targets `net8.0` and `net10.0`.
- `git clone https://github.com/SkyAPM/SkyAPM-dotnet.git`
- `cd SkyAPM-dotnet/`
- Switch to the tag by using `git checkout [tagname]` (optional, switch if you want to build a release from source codes)
- `git submodule init`
- `git submodule update`
- Run `dotnet restore`
- Build the protocol first (it generates the gRPC/protobuf code the agent depends on): `dotnet build src/SkyApm.Transport.Protocol`
- Run `dotnet build skyapm-dotnet.sln`

For the full contribution workflow, including how to run the tests, see [CONTRIBUTING](https://github.com/SkyAPM/SkyAPM-dotnet/blob/main/CONTRIBUTING.md).
