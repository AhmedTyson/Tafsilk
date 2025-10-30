# TafsilkPlatform.Web - Production Deployment Guide

## 🚀 Quick Deployment Checklist

### ✅ Pre-Deployment Verification

- [ ] All code committed and pushed to repository
- [ ] Release build successful
- [ ] Database migrations generated
- [ ] Production configuration prepared
- [ ] Secrets configured (not in source control)
- [ ] HTTPS certificate ready
- [ ] Domain name configured

---

## 📋 Publishing Options

### Option 1: Azure App Service (Recommended)

#### **Step 1: Prepare Azure Resources**

```bash
# Login to Azure
az login

# Create Resource Group
az group create --name TafsilkPlatform-RG --location "UAE North"

# Create App Service Plan
az appservice plan create \
  --name TafsilkPlatform-Plan \
  --resource-group TafsilkPlatform-RG \
  --sku B1 \
  --is-linux

# Create Web App
az webapp create \
  --name tafsilk-platform \
  --resource-group TafsilkPlatform-RG \
  --plan TafsilkPlatform-Plan \
  --runtime "DOTNET|9.0"
```

#### **Step 2: Configure Database**

```bash
# Create Azure SQL Database
az sql server create \
  --name tafsilk-sql-server \
  --resource-group TafsilkPlatform-RG \
  --location "UAE North" \
  --admin-user sqladmin \
  --admin-password "YOUR_SECURE_PASSWORD"

az sql db create \
  --name TafsilkPlatformDb \
  --server tafsilk-sql-server \
  --resource-group TafsilkPlatform-RG \
  --service-objective S0

# Get connection string
az sql db show-connection-string \
  --client ado.net \
  --server tafsilk-sql-server \
  --name TafsilkPlatformDb
```

#### **Step 3: Configure Application Settings**

```bash
# Set connection string
az webapp config connection-string set \
  --name tafsilk-platform \
  --resource-group TafsilkPlatform-RG \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="YOUR_CONNECTION_STRING"

# Set application settings
az webapp config appsettings set \
  --name tafsilk-platform \
  --resource-group TafsilkPlatform-RG \
  --settings \
    ASPNETCORE_ENVIRONMENT="Production" \
    Jwt__Key="YOUR_JWT_SECRET_KEY" \
    Google__client_id="YOUR_GOOGLE_CLIENT_ID" \
    Google__client_secret="YOUR_GOOGLE_CLIENT_SECRET" \
    Facebook__app_id="YOUR_FACEBOOK_APP_ID" \
    Facebook__app_secret="YOUR_FACEBOOK_APP_SECRET"
```

#### **Step 4: Publish from Visual Studio**

1. Right-click project → **Publish**
2. Select **Azure** → **Azure App Service (Linux)**
3. Sign in and select your app
4. Click **Publish**

#### **Step 5: Publish from Command Line**

```bash
# Navigate to project directory
cd TafsilkPlatform.Web

# Publish to folder
dotnet publish -c Release -o ./publish

# Deploy to Azure (requires Azure CLI)
az webapp deployment source config-zip \
  --resource-group TafsilkPlatform-RG \
  --name tafsilk-platform \
  --src ./publish.zip
```

---

### Option 2: IIS (Windows Server)

#### **Prerequisites**

- Windows Server with IIS installed
- .NET 9 Hosting Bundle installed
- SQL Server accessible

#### **Step 1: Install .NET 9 Hosting Bundle**

```powershell
# Download from: https://dotnet.microsoft.com/download/dotnet/9.0
# Run installer: dotnet-hosting-9.0.x-win.exe
# Restart IIS
iisreset
```

#### **Step 2: Publish Application**

```bash
dotnet publish -c Release -o C:\inetpub\wwwroot\TafsilkPlatform
```

#### **Step 3: Create IIS Site**

1. Open **IIS Manager**
2. Right-click **Sites** → **Add Website**
3. Site name: `TafsilkPlatform`
4. Physical path: `C:\inetpub\wwwroot\TafsilkPlatform`
5. Binding: `https` on port `443`
6. Create Application Pool:
   - Name: `TafsilkPlatformPool`
   - .NET CLR version: **No Managed Code**
   - Managed pipeline mode: **Integrated**

#### **Step 4: Configure Environment Variables**

```powershell
# Set environment variables in web.config or IIS Manager
# Or use appsettings.Production.json
```

---

### Option 3: Docker Container

#### **Step 1: Create Dockerfile**

See `Dockerfile` in project root.

#### **Step 2: Build Docker Image**

```bash
docker build -t tafsilkplatform:latest .
```

#### **Step 3: Run Container**

```bash
docker run -d \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="YOUR_CONNECTION_STRING" \
  -e Jwt__Key="YOUR_JWT_KEY" \
  --name tafsilk \
  tafsilkplatform:latest
```

#### **Step 4: Push to Container Registry**

```bash
# Azure Container Registry
az acr login --name yourregistry
docker tag tafsilkplatform:latest yourregistry.azurecr.io/tafsilkplatform:latest
docker push yourregistry.azurecr.io/tafsilkplatform:latest
```

---

## 🔐 Security Configuration

### **1. Connection Strings**

**❌ Don't store in appsettings.json:**
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=...;Password=MyPassword123"
}
```

**✅ Use Environment Variables or Azure Key Vault:**
```bash
# Environment variable
export ConnectionStrings__DefaultConnection="..."

# Azure Key Vault
az keyvault secret set \
  --vault-name tafsilk-keyvault \
  --name ConnectionStrings--DefaultConnection \
  --value "YOUR_CONNECTION_STRING"
```

### **2. JWT Secret**

```bash
# Generate secure key
openssl rand -base64 32

# Set in Azure
az webapp config appsettings set \
  --name tafsilk-platform \
  --resource-group TafsilkPlatform-RG \
  --settings Jwt__Key="YOUR_GENERATED_KEY"
```

### **3. OAuth Credentials**

Configure in Azure App Service → **Configuration** → **Application settings**

---

## 📊 Database Migration

### **Option A: Automatic on Startup**

Add to `Program.cs`:

```csharp
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}
```

### **Option B: Manual Migration**

```bash
# Generate SQL script
dotnet ef migrations script -o migration.sql

# Apply manually to production database
sqlcmd -S your-server -d TafsilkPlatformDb -i migration.sql
```

---

## 🔍 Health Checks

Verify deployment:

- [ ] **Health endpoint**: `https://your-app.azurewebsites.net/health`
- [ ] **Home page**: `https://your-app.azurewebsites.net`
- [ ] **Database connectivity**: Check logs
- [ ] **Authentication**: Test login flow
- [ ] **HTTPS**: Verify certificate

---

## 📝 Post-Deployment Checklist

- [ ] Test all major features
- [ ] Verify OAuth login (Google/Facebook)
- [ ] Check database operations
- [ ] Test file uploads
- [ ] Review application logs
- [ ] Configure monitoring/alerts
- [ ] Set up backup strategy
- [ ] Document rollback procedure

---

## 🐛 Troubleshooting

### **Issue: 500 Internal Server Error**

**Solution:**
```bash
# Enable detailed errors
az webapp config appsettings set \
  --settings ASPNETCORE_ENVIRONMENT="Development"

# Check logs
az webapp log tail --name tafsilk-platform --resource-group TafsilkPlatform-RG
```

### **Issue: Database Connection Failed**

**Solution:**
- Check connection string format
- Verify firewall rules
- Test connectivity from app server

### **Issue: OAuth Not Working**

**Solution:**
- Update redirect URIs in Google/Facebook consoles
- Verify secrets are configured
- Check HTTPS is enabled

---

## 📞 Support

For deployment assistance:
- Azure Support: https://azure.microsoft.com/support
- Documentation: https://docs.microsoft.com/aspnet/core/host-and-deploy

---

**Deployment Status:** Ready ✅
**Last Updated:** 2025
