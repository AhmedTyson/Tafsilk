#!/bin/bash

# Tafsilk Platform - OAuth Setup Script
# This script helps you quickly configure Google and Facebook OAuth credentials

echo "🔐 Tafsilk Platform - OAuth Configuration Setup"
echo "=============================================="
echo ""

# Check if dotnet is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK is not installed. Please install it first."
    exit 1
fi

echo "📝 This script will help you configure OAuth credentials using User Secrets."
echo ""

# Initialize user secrets if not already done
echo "🔧 Initializing User Secrets..."
cd TafsilkPlatform.Web
dotnet user-secrets init

echo ""
echo "======================================"
echo "🌐 Google OAuth Configuration"
echo "======================================"
echo ""
echo "To get Google OAuth credentials:"
echo "1. Go to: https://console.cloud.google.com/"
echo "2. Create a new project or select existing"
echo "3. Enable Google+ API"
echo "4. Create OAuth 2.0 credentials (Web application)"
echo "5. Add redirect URI: https://localhost:7186/signin-google"
echo ""
read -p "Enter Google Client ID: " google_client_id
read -p "Enter Google Client Secret: " google_client_secret

if [ ! -z "$google_client_id" ] && [ ! -z "$google_client_secret" ]; then
    dotnet user-secrets set "Google:client_id" "$google_client_id"
    dotnet user-secrets set "Google:client_secret" "$google_client_secret"
 echo "✅ Google OAuth credentials saved!"
else
    echo "⚠️  Skipping Google configuration (empty values)"
fi

echo ""
echo "======================================"
echo "📘 Facebook OAuth Configuration"
echo "======================================"
echo ""
echo "To get Facebook OAuth credentials:"
echo "1. Go to: https://developers.facebook.com/"
echo "2. Create a new app (Consumer type)"
echo "3. Set up Facebook Login"
echo "4. Add redirect URI: https://localhost:7186/signin-facebook"
echo "5. Get App ID and App Secret from Settings > Basic"
echo ""
read -p "Enter Facebook App ID: " facebook_app_id
read -p "Enter Facebook App Secret: " facebook_app_secret

if [ ! -z "$facebook_app_id" ] && [ ! -z "$facebook_app_secret" ]; then
    dotnet user-secrets set "Facebook:app_id" "$facebook_app_id"
    dotnet user-secrets set "Facebook:app_secret" "$facebook_app_secret"
    echo "✅ Facebook OAuth credentials saved!"
else
    echo "⚠️  Skipping Facebook configuration (empty values)"
fi

echo ""
echo "======================================"
echo "📋 Current Configuration"
echo "======================================"
dotnet user-secrets list

echo ""
echo "✅ Setup complete!"
echo ""
echo "Next steps:"
echo "1. Run the application: dotnet run"
echo "2. Navigate to: https://localhost:7186/Account/Login"
echo "3. Test OAuth login buttons"
echo ""
echo "📖 For detailed instructions, see: OAuth_Setup_Guide.md"
echo ""
