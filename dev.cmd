cd /d %~dp0

SET WorkDir=%~dp0

SET CORECLR_PROFILER={cf0d821e-299b-5307-a3d8-b283c03916dd}
SET CORECLR_ENABLE_PROFILING=1
SET CORECLR_PROFILER_PATH=%WorkDir%src\SkyApm.ClrProfiler\x64\Debug\SkyApm.ClrProfiler.dll
SET CORECLR_PROFILER_HOME=%WorkDir%src\SkyApm.ClrProfiler.Trace\bin\Debug\netstandard2.0

SET COR_PROFILER={af0d821e-299b-5307-a3d8-b283c03916dd}
SET COR_ENABLE_PROFILING=1
SET COR_PROFILER_PATH=%WorkDir%src\SkyApm.ClrProfiler\x64\Debug\SkyApm.ClrProfiler.dll
SET COR_PROFILER_HOME=%WorkDir%src\SkyApm.ClrProfiler.Trace\bin\Debug\net461

SET SKYAPM__CONFIG__PATH=%WorkDir%skyapm.json
SET SkyWalking:Transport:gRPC:Servers=127.0.0.1:11800

echo Starting Visual Studio...
set _VSWHERE="%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
if exist %_VSWHERE% (
  for /f "usebackq tokens=*" %%i in (`%_VSWHERE% -latest -prerelease -property installationPath`) do set _VSPATH=%%i
)
START "Visual Studio" "%_VSPATH%\Common7\IDE\devenv.exe" "%~dp0\skyapm-dotnet.sln"
