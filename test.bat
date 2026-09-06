@echo off
setlocal
cd /d "%~dp0"

echo ========================================================
echo   AutoStructureMaker Build and Test Runner (x64)
echo ========================================================
echo.

set "MSBUILD="
if exist "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" set "MSBUILD=C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe"
if "%MSBUILD%"=="" if exist "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" set "MSBUILD=C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
if "%MSBUILD%"=="" if exist "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" set "MSBUILD=C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe"
if "%MSBUILD%"=="" if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" set "MSBUILD=C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"

if "%MSBUILD%"=="" (
    echo [ERROR] MSBuild.exe was not found.
    exit /b 1
)

set "VSTEST="
if exist "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" set "VSTEST=C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe"
if "%VSTEST%"=="" if exist "C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" set "VSTEST=C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe"
if "%VSTEST%"=="" if exist "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" set "VSTEST=C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe"

if "%VSTEST%"=="" (
    echo [ERROR] vstest.console.exe was not found.
    exit /b 1
)

echo [INFO] Found MSBuild: %MSBUILD%
echo [INFO] Found VSTest:  %VSTEST%
echo.

echo [INFO] Building Solution (Release ^| x64)...
"%MSBUILD%" AutoStructureMaker.sln /p:Configuration=Release /p:Platform="x64" /m /v:minimal

if %ERRORLEVEL% neq 0 (
    echo.
    echo [ERROR] Solution build failed!
    exit /b %ERRORLEVEL%
)

echo.
echo [INFO] Running Unit Tests (Release ^| x64)...
echo.
"%VSTEST%" AutoStructureMaker.Tests\bin\x64\Release\AutoStructureMaker.Tests.dll /Platform:x64

if %ERRORLEVEL% neq 0 (
    echo.
    echo [ERROR] Unit tests failed!
    exit /b %ERRORLEVEL%
)

echo.
echo ========================================================
echo   [SUCCESS] All builds and unit tests completed (100%% PASS)!
echo ========================================================
exit /b 0