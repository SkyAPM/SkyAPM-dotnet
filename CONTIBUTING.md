# Contributing to SkyAPM-dotnet

One of the easiest ways to contribute is to participate in discussions and discuss issues. You can also contribute by submitting pull requests with code changes.

## General feedback and discussions?
Please start a discussion on the [issue tracker](https://github.com/SkyAPM/SkyAPM-dotnet/issues).

# Build

1. clone this repo

```
git clone https://github.com/SkyAPM/SkyAPM-dotnet.git
```

2. Setup Submodule

```
git submodule update --init
```

If this process is hung or has problems, please download all the contents from https://github.com/apache/skywalking-data-collect-protocol manually and put them in src/SkyApm.Transport.Protocol/protocol-v3 directory.

3. build

* Debug Mode

```
// Restore dependencies
dotnet restore
// Build Protocol
dotnet build src/SkyApm.Transport.Protocol --no-restore -c debug
// Build
dotnet build --no-restore -c debug
// Test
dotnet test --no-build --verbosity normal --framework net6.0 -c debug
```

* Release Mode

```
// Restore dependencies
dotnet restore
// Build Protocol
dotnet build src/SkyApm.Transport.Protocol --no-restore -c release
// Build
dotnet build --no-restore -c release
// Test
dotnet test --no-build --verbosity normal --framework net6.0 -c release
```
