# Contributing to SkyAPM-dotnet

One of the easiest ways to contribute is to participate in discussions and discuss issues. You can also contribute by submitting pull requests with code changes.

## General feedback and discussions?
Please start a discussion on the [issue tracker](https://github.com/SkyAPM/SkyAPM-dotnet/issues).

# Build

1. clone this repo

```
git clone https://github.com/SkyAPM/SkyAPM-dotnet.git
```

2. clone `skywalking-data-collect-protocol` to get the protocol file

```
git clone https://github.com/apache/skywalking-data-collect-protocol.git
```

3. copy `skywalking-data-collect-protocol` all files to `SkyAPM-dotnet/src/SkyApm.Transport.Grpc.Protocol/protocol-v3`

```
cp skywalking-data-collect-protocol/* SkyAPM-dotnet/src/SkyApm.Transport.Grpc.Protocol/protocol-v3/
```

4. open `SkyAPM-dotnet/skyapm-dotnet.sln`
