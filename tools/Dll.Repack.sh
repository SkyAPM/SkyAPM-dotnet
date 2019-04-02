#!/bin/sh

WorkDir=$(cd `dirname $0`; pwd)

BuildType=$1
if [ $BuildType !="Release" ]; then
   BuildType="Debug"
fi

BuildVersion=$2
if [ !$BuildVersion ]; then
   BuildVersion="1.0.0"
fi

OUTDLL="SkyApm.ClrProfiler.Trace.dll"
DLLS="SkyApm.ClrProfiler.Trace.dll  SkyAPM.Core.dll Newtonsoft.Json.dll"
DLLS="${DLLS} Serilog.dll Serilog.Sinks.File.dll Serilog.Sinks.RollingFile.dll"
DLLS="${DLLS} Microsoft.Extensions.DependencyInjection.Abstractions.dll Microsoft.Extensions.DependencyInjection.dll"
DLLS="${DLLS} Microsoft.Extensions.FileSystemGlobbing.dll Microsoft.Extensions.FileProviders.Physical.dll"
DLLS="${DLLS} Microsoft.Extensions.FileProviders.Abstractions.dll Microsoft.Extensions.Configuration.Json.dll"
DLLS="${DLLS} Microsoft.Extensions.Configuration.FileExtensions.dll Microsoft.Extensions.Configuration.EnvironmentVariables.dll"
DLLS="${DLLS} Microsoft.Extensions.Configuration.dll Microsoft.Extensions.Configuration.Binder.dll"
DLLS="${DLLS} Microsoft.Extensions.Configuration.Abstractions.dll Microsoft.Extensions.Primitives.dll System.Runtime.CompilerServices.Unsafe.dll"

TransportOUTDLL="SkyAPM.Transport.Grpc.dll"
TransportDLLS="SkyAPM.Transport.Grpc.dll System.Interactive.Async.dll Google.Protobuf.dll Grpc.Core.dll"
TransportDLLS="${TransportDLLS} SkyAPM.Transport.Grpc.Protocol.dll"

if [ ! -d "$WorkDir/il-repack" ];then
	git clone https://github.com/caozhiyuan/il-repack.git
fi

cd il-repack/ILRepack

if [ ! -d "$WorkDir/ILRepack" ];then
	dotnet publish -c release -f netcoreapp2.2 -o $WorkDir/ILRepack
fi

cd $WorkDir/../

dotnet build skyapm-dotnet.sln -c $BuildType

cd src
cd SkyApm.Transport.Grpc

dotnet publish -c $BuildType -f netstandard2.0 -o ../SkyApm.ClrProfiler.Trace/bin/$BuildType/netstandard2.0

cd ../
cd SkyApm.ClrProfiler.Trace

dotnet publish -c $BuildType -f netstandard2.0 -o bin/$BuildType/netstandard2.0
cd bin/$BuildType/netstandard2.0

OUTDLL=$WorkDir../src/SkyApm.ClrProfiler.Trace/bin/$BuildType/netstandard2.0/SkyApm.ClrProfiler.Trace.dll
TransportOUTDLL=$WorkDir../src/SkyApm.ClrProfiler.Trace/bin/$BuildType/netstandard2.0/SkyAPM.Transport.Grpc.dll

dotnet $WorkDir/ILRepack/ILRepack.dll /keyfile:$WorkDir/skyapm.snk /ver:$BuildVersion /xmldocs /ndebug /internalize /out:$OUTDLL $DLLS
dotnet $WorkDir/ILRepack/ILRepack.dll /keyfile:$WorkDir/skyapm.snk /ver:$BuildVersion /xmldocs /ndebug /internalize /out:$TransportOUTDLL $TransportDLLS
