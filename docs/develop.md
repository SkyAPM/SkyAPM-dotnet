# Developer Guide

## Prerequisites

* CoreCLR Repository (build from source) Dependencies
* Visual Studio 2017 (C++ Required) 
* CLang3.9 (Linux)
* Vcpkg (Windows Linux)
* [WiX Toolset 3.11.1](http://wixtoolset.org/releases/) to build Windows installer (msi)
* [WiX Toolset VS2017 Extension](https://marketplace.visualstudio.com/items?itemName=RobMensching.WixToolsetVisualStudio2017Extension)
  
  
## Building

### Build

#### windows 

```batch

git clone https://github.com/SkyAPM/SkyAPM-dotnet.git

cd SkyAPM-dotnet
git checkout coreclr_profiler
powershell ./tools/install-vcpkgs.ps1

: if has no executionpolicy ,powershell(admin) run set-executionpolicy remotesigned
: if no install vs english language pack vcpkgs will run error

set _VSWHERE="%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
if exist %_VSWHERE% ( for /f "usebackq tokens=*" %i in (`%_VSWHERE% -latest -prerelease -property installationPath`) do set _VSPATH=%i)
call "%_VSPATH%\VC\Auxiliary\Build\vcvars64.bat" 

cd src\SkyAPM.ClrProfiler
SET BuildArch=x64
SET BuildType=Debug
build
```

#### linux

```batch

git clone https://github.com/SkyAPM/SkyAPM-dotnet.git

git clone https://github.com/Microsoft/vcpkg.git
cd ~/vcpkg
./bootstrap-vcpkg.sh
./vcpkg install spdlog

cd ~/SkyAPM-dotnet
git checkout coreclr_profiler

export VCPKG_ROOT=~/vcpkg
cd src/SkyApm.ClrProfiler
mkdir build
cd build 
cmake ..
make

```

### Setup

```batch

git submodule update --init -- "src/SkyApm.Transport.Grpc.Protocol/protocol" 
dotnet msbuild -restore:True src/SkyApm.Transport.Grpc.Protocol/SkyApm.Transport.Grpc.Protocol.csproj

SET CORECLR_PROFILER={cf0d821e-299b-5307-a3d8-b283c03916dd}
SET CORECLR_ENABLE_PROFILING=1
SET CORECLR_PROFILER_PATH=%WorkDir%src\SkyApm.ClrProfiler\x64\Debug\SkyApm.ClrProfiler.dll
SET CORECLR_PROFILER_HOME=%WorkDir%src\SkyApm.ClrProfiler.Trace\bin\Debug\netstandard2.0

SET COR_PROFILER={af0d821e-299b-5307-a3d8-b283c03916dd}
SET COR_ENABLE_PROFILING=1
SET COR_PROFILER_PATH=%WorkDir%src\SkyApm.ClrProfiler\x64\Debug\SkyApm.ClrProfiler.dll
SET COR_PROFILER_HOME=%WorkDir%src\SkyApm.ClrProfiler.Trace\bin\Debug\net461

cd tools
Dll.Repack.cmd Debug 

: or Dll.Repack.cmd Release

: .net core can rebuild SkyAPM.ClrProfiler.Trace to Debug
: c++ native code debug see https://docs.microsoft.com/en-us/visualstudio/debugger/how-to-debug-managed-and-native-code?view=vs-2017

run program

```

### Windows Installer

vs2017 build SkyApm.DotNet.Installer , please read ReadMe.txt first.
