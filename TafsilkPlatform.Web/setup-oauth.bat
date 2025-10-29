@echo off
REM Tafsilk Platform - OAuth Setup Script (Windows)
REM This script helps you quickly configure Google and Facebook OAuth credentials

echo.
echo ====================================
echo Tafsilk Platform - OAuth Setup
echo ====================================
echo.

REM Check if dotnet is installed
where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: .NET SDK is not installed. Please install it first.
    pause
    exit /b 1
)

echo This script will help you configure OAuth credentials using User Secrets.
echo.

REM Initialize user secrets
echo Initializing User Secrets...
cd TafsilkPlatform.Web
dotnet user-secrets init

echo.
echo ====================================
echo Google OAuth Configuration
echo ====================================
echo.
echo To get Google OAuth credentials:
echo 1. Go to: https://console.cloud.google.com/
echo 2. Create a new project or select existing
echo 3. Enable Google+ API
echo 4. Create OAuth 2.0 credentials (Web application)
echo 5. Add redirect URI: https://localhost:7186/signin-google
echo.

set /p google_client_id="Enter Google Client ID: "
set /p google_client_secret="Enter Google Client Secret: "

if not "%google_client_id%"=="" if not "%google_client_secret%"=="" (
    dotnet user-secrets set "Google:client_id" "%google_client_id%"
    dotnet user-secrets set "Google:client_secret" "%google_client_secret%"
    echo Google OAuth credentials saved!
) else (
    echo Skipping Google configuration (empty values)
)

echo.
echo ====================================
echo Facebook OAuth Configuration
echo ====================================
echo.
echo To get Facebook OAuth credentials:
echo 1. Go to: https://developers.facebook.com/
echo 2. Create a new app (Consumer type)
echo 3. Set up Facebook Login
echo 4. Add redirect URI: https://localhost:7186/signin-facebook
echo 5. Get App ID and App Secret from Settings ^> Basic
echo.

set /p facebook_app_id="Enter Facebook App ID: "
set /p facebook_app_secret="Enter Facebook App Secret: "

if not "%facebook_app_id%"=="" if not "%facebook_app_secret%"=="" (
    dotnet user-secrets set "Facebook:app_id" "%facebook_app_id%"
    dotnet user-secrets set "Facebook:app_secret" "%facebook_app_secret%"
    echo Facebook OAuth credentials saved!
) else (
    echo Skipping Facebook configuration (empty values)
)

echo.
echo ====================================
echo Current Configuration
echo ====================================
dotnet user-secrets list

echo.
echo Setup complete!
echo.
echo Next steps:
echo 1. Run the application: dotnet run
echo 2. Navigate to: https://localhost:7186/Account/Login
echo 3. Test OAuth login buttons
echo.
echo For detailed instructions, see: OAuth_Setup_Guide.md
echo.
pause
