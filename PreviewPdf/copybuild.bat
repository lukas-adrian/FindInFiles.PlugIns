@echo off
setlocal

REM === Configuration ===
REM Automatically detect the build configuration (Debug/Release) from the environment
IF "%CONFIG%"=="" (
    set "CONFIG=%1"
)
IF "%CONFIG%"=="" (
    REM Default to Debug if not provided
    set "CONFIG=Debug"
)

REM Set platform target (x64 or x86)
set "PLATFORM=x64"

REM Set target framework
set "TARGETFRAMEWORK=net8.0-windows"

REM Dynamically set the source path based on the configuration
set "SOURCE=%~dp0bin\%PLATFORM%\%CONFIG%\%TARGETFRAMEWORK%\"  

REM Extract the project name from the current directory
for %%i in ("%cd%") do set "PROJECTNAME=%%~nxi"

REM Set the destination path using the project name
set "DEST=..\..\FindInFiles\FindInFile\PlugIns\%PROJECTNAME%"

echo [INFO] Copying build output from:
echo   %SOURCE%
echo to:
echo   %DEST%

REM Ensure the destination directory for the project exists, create if it doesn't
if not exist "%DEST%" (
    echo [INFO] Destination directory "%DEST%" doesn't exist. Creating it...
    mkdir "%DEST%"
)

REM Perform the copy, excluding PlugInBase.dll
for /F "delims=" %%i in ('dir /B "%SOURCE%"') do (
    REM Skip PlugInBase.dll
    if /I not "%%i"=="PlugInBase.dll" (
        xcopy "%SOURCE%%%i" "%DEST%\" /Y /Q
    )
)

echo [DONE] Copy complete.
endlocal
