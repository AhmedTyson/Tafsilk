# 🚀 Quick Publish Guide - TafsilkPlatform.Web

## ⚡ 5-Minute Azure Deployment

### **Prerequisites:**
- Azure subscription
- Azure CLI installed
- .NET 9 SDK installed

### **Step 1: Validate Project (2 minutes)**

```powershell
# Windows
.\validate-deployment.ps1

# Linux/Mac
chmod +x validate-deployment.sh
./validate-deployment.sh
```

### **Step 2: Create Azure Resources (2 minutes)**

```bash
# Login to Azure
az login

# Set variables
RESOURCE_GROUP="TafsilkPlatform-RG"
LOCATION="uaenorth"
APP_NAME="tafsilk-$(openssl rand -hex 4)"
SQL_SERVER="${APP_NAME}-sql"
DB_NAME="TafsilkPlatformDb"
SQL_ADMIN="sqladmin"
SQL_PASSWORD="$(openssl rand -base64 16)P@ss!"

# Create resource group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Create App Service Plan
az appservice plan create \
  --name "${APP_NAME}-plan" \
  --resource-group $RESOURCE_GROUP \
  --sku B1 \
  --is-linux

# Create Web App
az webapp create \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --plan "${APP_NAME}-plan" \
  --runtime "DOTNETCORE:9.0"

# Create SQL Server
az sql server create \
  --name $SQL_SERVER \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --admin-user $SQL_ADMIN \
  --admin-password $SQL_PASSWORD

# Create Database
az sql db create \
  --name $DB_NAME \
  --server $SQL_SERVER \
  --resource-group $RESOURCE_GROUP \
  --service-objective S0

# Allow Azure services to access SQL Server
az sql server firewall-rule create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Get connection string
CONNECTION_STRING="Server=tcp:${SQL_SERVER}.database.windows.net,1433;Database=${DB_NAME};User ID=${SQL_ADMIN};Password=${SQL_PASSWORD};Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;"

echo "✅ Resources created successfully!"
echo "App URL: https://${APP_NAME}.azurewebsites.net"
echo "Save this connection string: $CONNECTION_STRING"
```

### **Step 3: Configure App Settings (1 minute)**

```bash
# Generate JWT secret
JWT_SECRET=$(openssl rand -base64 32)

# Configure app settings
az webapp config appsettings set \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --settings \
    ASPNETCORE_ENVIRONMENT="Production" \
    ConnectionStrings__DefaultConnection="$CONNECTION_STRING" \
    Jwt__Key="$JWT_SECRET" \
    Jwt__Issuer="TafsilkPlatform" \
    Jwt__Audience="TafsilkPlatformUsers"

# Optional: Configure OAuth (replace with your credentials)
az webapp config appsettings set \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --settings \
    Google__client_id="YOUR_GOOGLE_CLIENT_ID" \
    Google__client_secret="YOUR_GOOGLE_CLIENT_SECRET" \
    Facebook__app_id="YOUR_FACEBOOK_APP_ID" \
    Facebook__app_secret="YOUR_FACEBOOK_APP_SECRET"

echo "✅ Configuration complete!"
```

### **Step 4: Deploy Application (< 1 minute)**

```bash
# Build and publish
cd TafsilkPlatform.Web
dotnet publish -c Release -o ../publish

# Create deployment package
cd ../publish
zip -r ../deploy.zip .

# Deploy to Azure
cd ..
az webapp deployment source config-zip \
  --resource-group $RESOURCE_GROUP \
  --name $APP_NAME \
  --src deploy.zip

echo "✅ Deployment complete!"
echo "🌐 Your app is live at: https://${APP_NAME}.azurewebsites.net"
```

---

## 🐳 Docker Quick Deploy (5 minutes)

### **Step 1: Create Environment File**

```bash
# Copy template and edit
cp .env.template .env

# Edit .env with your values
nano .env
```

### **Step 2: Build and Run**

```bash
# Build image
docker build -t tafsilkplatform:latest .

# Run with Docker Compose
docker-compose up -d

# Check status
docker-compose ps

echo "✅ Application running at http://localhost:8080"
```

### **Step 3: Initialize Database**

```bash
# Run migrations
docker-compose exec web dotnet ef database update

echo "✅ Database initialized!"
```

---

## 💻 IIS Quick Deploy (Windows Server)

### **Step 1: Install Prerequisites**

```powershell
# Install .NET 9 Hosting Bundle
# Download from: https://dotnet.microsoft.com/download/dotnet/9.0
# Run: dotnet-hosting-9.0.x-win.exe
# Restart IIS
iisreset
```

### **Step 2: Publish Application**

```powershell
# Publish to folder
dotnet publish TafsilkPlatform.Web\TafsilkPlatform.Web.csproj `
  -c Release `
  -o C:\inetpub\wwwroot\TafsilkPlatform
```

### **Step 3: Create IIS Site**

```powershell
# Import IIS module
Import-Module WebAdministration

# Create Application Pool
New-WebAppPool -Name "TafsilkPlatformPool"
Set-ItemProperty IIS:\AppPools\TafsilkPlatformPool `
  -Name managedRuntimeVersion -Value ""

# Create Website
New-Website -Name "TafsilkPlatform" `
  -PhysicalPath "C:\inetpub\wwwroot\TafsilkPlatform" `
  -ApplicationPool "TafsilkPlatformPool" `
  -Port 80

# Start website
Start-Website -Name "TafsilkPlatform"

echo "✅ IIS site created and started!"
```

### **Step 4: Configure Connection String**

Edit `C:\inetpub\wwwroot\TafsilkPlatform\appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SQL_SERVER;Database=TafsilkPlatformDb;Trusted_Connection=True;"
  }
}
```

---

## 📋 Post-Deployment Checklist

### **Verify Deployment:**

```bash
# Test homepage
curl https://your-app-url.com

# Test health endpoint
curl https://your-app-url.com/health

# Check logs
az webapp log tail --name $APP_NAME --resource-group $RESOURCE_GROUP
```

### **Configure OAuth Redirect URIs:**

#### **Google Console:**
1. Go to: https://console.cloud.google.com/apis/credentials
2. Select your OAuth 2.0 Client
3. Add Authorized redirect URIs:
   - `https://your-app-url.com/signin-google`

#### **Facebook Developer:**
1. Go to: https://developers.facebook.com/apps
2. Select your app → Settings → Basic
3. Add Valid OAuth Redirect URIs:
   - `https://your-app-url.com/signin-facebook`

### **Security Checklist:**

- [ ] HTTPS enabled and enforced
- [ ] Connection strings not in source control
- [ ] JWT secret is strong and unique
- [ ] OAuth credentials configured
- [ ] Database firewall rules configured
- [ ] Application insights enabled (optional)
- [ ] Backup strategy configured

---

## 🔧 Troubleshooting

### **Issue: 500 Error After Deployment**

```bash
# Enable detailed errors temporarily
az webapp config appsettings set \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --settings ASPNETCORE_ENVIRONMENT="Development"

# Check logs
az webapp log tail --name $APP_NAME --resource-group $RESOURCE_GROUP
```

### **Issue: Database Connection Failed**

```bash
# Verify connection string
az webapp config connection-string list \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP

# Check firewall rules
az sql server firewall-rule list \
  --server $SQL_SERVER \
  --resource-group $RESOURCE_GROUP
```

### **Issue: OAuth Not Working**

1. Verify redirect URIs match exactly
2. Check OAuth credentials are set:
```bash
az webapp config appsettings list \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP | grep -E "Google|Facebook"
```

---

## 📞 Support Resources

- **Azure Docs:** https://docs.microsoft.com/azure/app-service/
- **Docker Docs:** https://docs.docker.com/
- **ASP.NET Core Docs:** https://docs.microsoft.com/aspnet/core/

---

## ✨ You're Done!

Your TafsilkPlatform.Web application is now deployed and running! 🎉

**Next Steps:**
1. Test all features thoroughly
2. Configure monitoring and alerts
3. Set up CI/CD pipeline
4. Configure custom domain
5. Enable SSL certificate

**Your app URL:** 
- Azure: `https://${APP_NAME}.azurewebsites.net`
- Docker: `http://localhost:8080`
- IIS: `http://your-server-ip`
