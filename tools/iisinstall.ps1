
$base_path = Split-Path -Parent $MyInvocation.MyCommand.Definition
$work_dir = Split-Path -Parent $base_path
$COR_PROFILER_PATH = "COR_PROFILER_PATH=" + $work_dir + "\src\ClrProfiler\x64\Debug\ClrProfiler.dll"
$COR_PROFILER_HOME = "COR_PROFILER_HOME=" + $work_dir + "\src\ClrProfiler.Trace\bin\Debug\net461"
$val = @(
    "COR_ENABLE_PROFILING=1",
    "COR_PROFILER={af0d821e-299b-5307-a3d8-b283c03916dd}",
    $COR_PROFILER_PATH,
    $COR_PROFILER_HOME
)

Set-ItemProperty -type MultiString HKLM:\SYSTEM\CurrentControlSet\Services\WAS -name "Environment" -value $val
