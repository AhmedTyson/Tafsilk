@echo off
echo Starting application to initialize database...
start /B dotnet run --no-build
timeout /t 10
taskkill /IM TafsilkPlatform.Web.exe /F 2>nul
taskkill /IM dotnet.exe /F /FI "WINDOWTITLE eq *TafsilkPlatform.Web*" 2>nul
echo Database initialization complete!
pause
