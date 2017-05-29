REM @ECHO OFF
REM Run the generator from the current configuration

REM Find a VS 2017 installation with the C++ toolset installed.
set InstallDir=
for /f "usebackq tokens=*" %%i in (`..\..\..\..\..\External\vswhere\vswhere -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 Microsoft.VisualStudio.Component.Windows10SDK.14393 -property installationPath`) do (
  set InstallDir=%%i
)
set ToolsVersion=
for /F %%A in ('type "%InstallDir%\VC\Auxiliary\Build\Microsoft.VCToolsVersion.default.txt"') do (
    set "ToolsVersion=%%A"
)
set VCToolsInstallDir=
if exist "%InstallDir%\VC\Tools\MSVC\%ToolsVersion%\" (
    set "VCToolsInstallDir=%InstallDir%\VC\Tools\MSVC\%ToolsVersion%\"
)
tools\SharpGen\SharpGen.exe --castxml tools\castxml\bin\castxml.exe config.xml --apptype WINDOWS_DESKTOP --od sources\
set LOCALERROR = %ERRORLEVEL%
exit /B %LOCALERROR%