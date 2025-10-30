@echo off
REM Ultra-fast publish to RunASP.NET - One command deployment
echo ================================================
echo   Quick Publish to tafsilk.runasp.net
echo ================================================
echo.

dotnet publish TafsilkPlatform.Web\TafsilkPlatform.Web.csproj -c Release /p:DeployOnBuild=true /p:PublishProfile=RunASP /p:Password=oC@3D4w_+K7i --nologo

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ================================================
    echo   Deployment Successful!
    echo ================================================
    echo.
    echo Your site: http://tafsilk.runasp.net
    echo Control Panel: https://panel.runasp.net
    echo.
    echo Opening site in browser...
    timeout /t 3
    start http://tafsilk.runasp.net
) else (
    echo.
    echo ================================================
    echo   Deployment Failed!
    echo ================================================
    echo.
    echo Try: publish-runasp.ps1 -Verbose
    echo For detailed error information
)

pause
