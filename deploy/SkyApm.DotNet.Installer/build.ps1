$args = @(
    "Product.wxs",
    "-out", "bin\Release\",
    "-arch", "x64",
    "-dPlatform=x64",
    "-dConfiguration=Release",
    "-ext", "WixUIExtension",
	"-ext", "WixUtilExtension"
)

Start-Process -NoNewWindow -Wait -FilePath "${Env:ProgramFiles(x86)}\WiX Toolset v3.11\bin\candle.exe" -ArgumentList $args

$args = @(
    ".\bin\Release\Product.wixobj",
    "-out", ".\bin\Release\SkyApm.DotNet.Installer.msi",
    "-ext", "WixUIExtension",
	"-ext", "WixUtilExtension"
)

Start-Process -NoNewWindow -Wait -FilePath "${Env:ProgramFiles(x86)}\WiX Toolset v3.11\bin\light.exe" -ArgumentList $args