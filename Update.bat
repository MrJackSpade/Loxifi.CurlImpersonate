@echo off
echo ========================================
echo Loxifi.CurlImpersonate Update Tool
echo ========================================
echo.
echo This script will:
echo  1. Download the latest curl-impersonate release
echo  2. Extract the Windows native library to the runtimes folder
echo  3. Generate BrowserProfile.cs and BrowserProfileTargets.cs
echo  4. Build the Linux library via WSL (if available; otherwise run build/linux/build-shim.sh on Linux)
echo.
echo Running code generator...
echo.

dotnet run --project src/Loxifi.CurlImpersonate.CodeGen

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: Code generation failed with error code %ERRORLEVEL%
    pause
    exit /b %ERRORLEVEL%
)

echo.
echo ========================================
echo Update complete!
echo ========================================
echo.
echo Rebuild the solution to use the new version:
echo   dotnet build
echo.
pause
