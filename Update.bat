@echo off
echo ========================================
echo Loxifi.CurlImpersonate Update Tool
echo ========================================
echo.
echo This script will:
echo  1. Download the latest curl-impersonate release
echo  2. Extract native binaries to runtimes folder
echo  3. Generate BrowserProfile.cs and BrowserProfileSettings.cs
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
