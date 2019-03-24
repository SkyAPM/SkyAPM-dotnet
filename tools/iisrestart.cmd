@ECHO OFF


ECHO this tool will restart iis, input any to contine, or you can restart iis manually
pause

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

iisreset /stop

Echo Deleting x86 Temporary ASP.NET Files

for /d %%i in ("%systemroot%\Microsoft.Net\Framework\v*") do for /d %%f in ("%%i\Temporary ASP.NET Files\*") do RD /q/s "%%f"

Echo Deleting x64 Temporary ASP.NET Files
for /d %%i in ("%systemroot%\Microsoft.Net\Framework64\v*") do for /d %%f in ("%%i\Temporary ASP.NET Files\*") do RD /q/s "%%f"

iisreset /start

@ECHO ON
