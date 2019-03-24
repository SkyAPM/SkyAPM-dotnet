msbuild /p:Configuration="%BuildType%" /p:BuildOS="Windows_NT" /p:BuildType="%BuildType%" /p:BuildArch="%BuildArch%" /p:OutDir="%BuildArch%/%BuildType%"
