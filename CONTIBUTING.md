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

3. build
```
// Restore dependencies
dotnet restore
// Build Protocol
dotnet build src/SkyApm.Transport.Grpc.Protocol --no-restore
// Build
dotnet build --no-restore
// Test
dotnet test --no-build --verbosity normal --framework net6.0
```
