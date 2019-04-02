@echo off
:: BatchGotAdmin
:-------------------------------------
REM  --> Check for permissions
    IF "%PROCESSOR_ARCHITECTURE%" EQU "amd64" (
>nul 2>&1 "%SYSTEMROOT%\SysWOW64\cacls.exe" "%SYSTEMROOT%\SysWOW64\config\system"
) ELSE (
>nul 2>&1 "%SYSTEMROOT%\system32\cacls.exe" "%SYSTEMROOT%\system32\config\system"
)

REM --> If error flag set, we do not have admin.
if '%errorlevel%' NEQ '0' (
    echo Requesting administrative privileges...
    goto UACPrompt
) else ( goto gotAdmin )

:UACPrompt
    echo Set UAC = CreateObject^("Shell.Application"^) > "%temp%\getadmin.vbs"
    set params= %*
    echo UAC.ShellExecute "cmd.exe", "/c ""%~s0"" %params:"=""%", "", "runas", 1 >> "%temp%\getadmin.vbs"

    "%temp%\getadmin.vbs"
    del "%temp%\getadmin.vbs"
    exit /B

:gotAdmin
    pushd "%CD%"
    CD /D "%~dp0"
:--------------------------------------    

cd /d %~dp0

SET BuildType=%1
if "%BuildType%"=="Release" (SET BuildType=Release) else (SET BuildType=Debug)

SET BuildVersion=%2
if "%BuildVersion%"=="" (SET BuildVersion=1.0.0)

SET WorkDir=%~dp0

SET OUTDLL=SkyApm.ClrProfiler.Trace.dll
SET DLLS=SkyApm.ClrProfiler.Trace.dll  SkyAPM.Core.dll Newtonsoft.Json.dll
SET DLLS=%DLLS% Serilog.dll Serilog.Sinks.File.dll Serilog.Sinks.RollingFile.dll
SET DLLS=%DLLS% Microsoft.Extensions.DependencyInjection.Abstractions.dll Microsoft.Extensions.DependencyInjection.dll
SET DLLS=%DLLS% Microsoft.Extensions.FileSystemGlobbing.dll Microsoft.Extensions.FileProviders.Physical.dll
SET DLLS=%DLLS% Microsoft.Extensions.FileProviders.Abstractions.dll Microsoft.Extensions.Configuration.Json.dll
SET DLLS=%DLLS% Microsoft.Extensions.Configuration.FileExtensions.dll Microsoft.Extensions.Configuration.EnvironmentVariables.dll
SET DLLS=%DLLS% Microsoft.Extensions.Configuration.dll Microsoft.Extensions.Configuration.Binder.dll
SET DLLS=%DLLS% Microsoft.Extensions.Configuration.Abstractions.dll Microsoft.Extensions.Primitives.dll System.Runtime.CompilerServices.Unsafe.dll

SET TransportOUTDLL=SkyApm.Transport.Grpc.dll
SET TransportDLLS=SkyApm.Transport.Grpc.dll System.Interactive.Async.dll Google.Protobuf.dll Grpc.Core.dll
SET TransportDLLS=%TransportDLLS% SkyAPM.Transport.Grpc.Protocol.dll 

set _VSWHERE="%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
if exist %_VSWHERE% (
  for /f "usebackq tokens=*" %%i in (`%_VSWHERE% -latest -prerelease -property installationPath`) do set _VSPATH=%%i
)

if not exist "%WorkDir%gacutil.exe" (
   "%_VSPATH%\MSBuild\15.0\Bin\MSBuild.exe" gacutil\gacutil.csproj /p:Configuration="Release" /p:OutputPath="../"
)

if not exist "%WorkDir%il-repack" (
   git clone https://github.com/caozhiyuan/il-repack.git
)

cd il-repack\ILRepack
if not exist "%WorkDir%ILRepack" (
   dotnet publish -c release -f netcoreapp2.1 -o %WorkDir%ILRepack
)

cd %WorkDir%../

"%_VSPATH%\MSBuild\15.0\Bin\MSBuild.exe" skyapm-dotnet.sln /p:Configuration="%BuildType%" /restore:True

cd src

cd SkyApm.Transport.Grpc
dotnet publish -c %BuildType% -f net461 -o ../SkyApm.ClrProfiler.Trace\bin\%BuildType%\net461
dotnet publish -c %BuildType% -f netstandard2.0 -o ../SkyApm.ClrProfiler.Trace\bin\%BuildType%\netstandard2.0

cd ../
cd SkyApm.ClrProfiler.Trace

dotnet publish -c %BuildType% -f net461 -o bin\%BuildType%\net461
cd bin\%BuildType%\net461

SET OUTDLL=%WorkDir%../src\SkyApm.ClrProfiler.Trace\bin\%BuildType%\net461\SkyApm.ClrProfiler.Trace.dll
SET TransportOUTDLL=%WorkDir%../src\SkyApm.ClrProfiler.Trace\bin\%BuildType%\net461\SkyApm.Transport.Grpc.dll

dotnet %WorkDir%ILRepack\ILRepack.dll /keyfile:%WorkDir%skyapm.snk /ver:%BuildVersion% /xmldocs /internalize /out:%OUTDLL% %DLLS%
dotnet %WorkDir%ILRepack\ILRepack.dll /keyfile:%WorkDir%skyapm.snk /ver:%BuildVersion% /xmldocs /internalize /out:%TransportOUTDLL% %TransportDLLS%

%WorkDir%gacutil.exe /i SkyApm.ClrProfiler.Trace.dll
%WorkDir%gacutil.exe /i SkyApm.Abstractions.dll
%WorkDir%gacutil.exe /i netstandard.dll

cd ../../../

dotnet publish -c %BuildType% -f netstandard2.0 -o bin\%BuildType%\netstandard2.0
cd bin\%BuildType%\netstandard2.0

SET OUTDLL=%WorkDir%../src\SkyApm.ClrProfiler.Trace\bin\%BuildType%\netstandard2.0\SkyApm.ClrProfiler.Trace.dll
SET TransportOUTDLL=%WorkDir%../src\SkyApm.ClrProfiler.Trace\bin\%BuildType%\netstandard2.0\SkyApm.Transport.Grpc.dll

dotnet %WorkDir%ILRepack\ILRepack.dll /keyfile:%WorkDir%skyapm.snk /ver:%BuildVersion% /xmldocs /internalize /out:%OUTDLL% %DLLS%
dotnet %WorkDir%ILRepack\ILRepack.dll /keyfile:%WorkDir%skyapm.snk /ver:%BuildVersion% /xmldocs /internalize /out:%TransportOUTDLL% %TransportDLLS%

cd /d %~dp0
