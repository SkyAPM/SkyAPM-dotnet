# Contributing to SkyAPM-dotnet

One of the easiest ways to contribute is to participate in discussions and discuss issues. You can also contribute by submitting pull requests with code changes.

The SkyAPM .NET agent targets `net8.0` and `net10.0`, and reports traces to the SkyWalking OAP backend over the sw8 (v8) protocol.

## General feedback and discussions?

Please start a discussion on the [issue tracker](https://github.com/SkyAPM/SkyAPM-dotnet/issues).

# Build

1. Clone this repo

```bash
git clone https://github.com/SkyAPM/SkyAPM-dotnet.git
```

2. Set up the submodule

```bash
git submodule update --init
```

If this process is hung or has problems, please download all the contents from https://github.com/apache/skywalking-data-collect-protocol manually and put them in the `src/SkyApm.Transport.Protocol/protocol-v3` directory.

3. Build

> Make sure you have the .NET 8 SDK or newer installed. The protocol project (`src/SkyApm.Transport.Protocol`) must be built before the rest of the solution, because it generates the gRPC/protobuf code the agent depends on.

* Debug Mode

```bash
# Restore dependencies
dotnet restore
# Build Protocol
dotnet build src/SkyApm.Transport.Protocol --no-restore -c debug
# Build
dotnet build --no-restore -c debug
# Test
dotnet test --no-build --verbosity normal --framework net8.0 -c debug
```

* Release Mode

```bash
# Restore dependencies
dotnet restore
# Build Protocol
dotnet build src/SkyApm.Transport.Protocol --no-restore -c release
# Build
dotnet build --no-restore -c release
# Test
dotnet test --no-build --verbosity normal --framework net8.0 -c release
```

The agent multi-targets `net8.0` and `net10.0`. To run the tests against .NET 10 instead, pass `--framework net10.0`.

See also: [How to build](docs/guides/how-to-build.md) and the [documentation index](docs/README.md).
