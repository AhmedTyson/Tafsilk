#!/bin/bash
# Pre-Deployment Validation Script (Linux/Mac)
# Run this before deploying to production

echo "TafsilkPlatform.Web - Pre-Deployment Validation"
echo "================================================="
echo ""

ERROR_COUNT=0
WARNING_COUNT=0

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Check 1: .NET SDK Version
echo -e "${YELLOW}[1/10] Checking .NET SDK...${NC}"
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version)
 if [[ $DOTNET_VERSION == 9.* ]]; then
      echo -e "${GREEN}✓ .NET 9 SDK installed: $DOTNET_VERSION${NC}"
    else
  echo -e "${RED}✗ .NET 9 SDK not found. Current: $DOTNET_VERSION${NC}"
 ((ERROR_COUNT++))
  fi
else
    echo -e "${RED}✗ .NET SDK not installed${NC}"
  ((ERROR_COUNT++))
fi

# Check 2: Build Release Configuration
echo -e "${YELLOW}[2/10] Building Release configuration...${NC}"
if dotnet build TafsilkPlatform.Web/TafsilkPlatform.Web.csproj --configuration Release --no-incremental > /dev/null 2>&1; then
    echo -e "${GREEN}✓ Release build successful${NC}"
else
    echo -e "${RED}✗ Release build failed${NC}"
    ((ERROR_COUNT++))
fi

# Check 3: Run Tests
echo -e "${YELLOW}[3/10] Running tests...${NC}"
if dotnet test --configuration Release --no-build > /dev/null 2>&1; then
    echo -e "${GREEN}✓ All tests passed${NC}"
else
    echo -e "${YELLOW}⚠ No tests found or tests failed${NC}"
    ((WARNING_COUNT++))
fi

# Check 4: Check for pending migrations
echo -e "${YELLOW}[4/10] Checking database migrations...${NC}"
cd TafsilkPlatform.Web
PENDING_MIGRATIONS=$(dotnet ef migrations list 2>&1 | grep -i "pending")
cd ..
if [ -n "$PENDING_MIGRATIONS" ]; then
    echo -e "${YELLOW}⚠ Pending migrations found. Run 'dotnet ef database update' before deployment${NC}"
    ((WARNING_COUNT++))
else
    echo -e "${GREEN}✓ No pending migrations${NC}"
fi

# Check 5: Production appsettings
echo -e "${YELLOW}[5/10] Validating production configuration...${NC}"
if [ -f "TafsilkPlatform.Web/appsettings.Production.json" ]; then
    if grep -q "localdb\|localhost" "TafsilkPlatform.Web/appsettings.Production.json"; then
        echo -e "${YELLOW}⚠ Production config contains local connection string${NC}"
   ((WARNING_COUNT++))
    else
  echo -e "${GREEN}✓ Production configuration exists${NC}"
    fi
else
    echo -e "${RED}✗ appsettings.Production.json not found${NC}"
    ((ERROR_COUNT++))
fi

# Check 6: Secrets in appsettings.json
echo -e "${YELLOW}[6/10] Checking for hardcoded secrets...${NC}"
if grep -qE '"Key":\s*"[A-Za-z0-9+/=]{32,}"|"client_secret":\s*"[^"]{10,}"|"Password=[^;]{5,}"' "TafsilkPlatform.Web/appsettings.json"; then
    echo -e "${YELLOW}⚠ Potential secrets found in appsettings.json${NC}"
    ((WARNING_COUNT++))
else
    echo -e "${GREEN}✓ No hardcoded secrets detected${NC}"
fi

# Check 7: wwwroot folder size
echo -e "${YELLOW}[7/10] Checking static files...${NC}"
if [ -d "TafsilkPlatform.Web/wwwroot" ]; then
    SIZE=$(du -sm "TafsilkPlatform.Web/wwwroot" | cut -f1)
 if [ "$SIZE" -gt 100 ]; then
        echo -e "${YELLOW}⚠ wwwroot folder is large: ${SIZE} MB${NC}"
      ((WARNING_COUNT++))
    else
        echo -e "${GREEN}✓ Static files size OK: ${SIZE} MB${NC}"
    fi
fi

# Check 8: HTTPS configuration
echo -e "${YELLOW}[8/10] Checking HTTPS configuration...${NC}"
if grep -q "UseHttpsRedirection" "TafsilkPlatform.Web/Program.cs"; then
    echo -e "${GREEN}✓ HTTPS redirection enabled${NC}"
else
    echo -e "${YELLOW}⚠ HTTPS redirection not found${NC}"
    ((WARNING_COUNT++))
fi

# Check 9: Environment-specific logging
echo -e "${YELLOW}[9/10] Checking logging configuration...${NC}"
if [ -f "TafsilkPlatform.Web/appsettings.Production.json" ]; then
    if grep -qE '"Default":\s*"(Warning|Error)"' "TafsilkPlatform.Web/appsettings.Production.json"; then
        echo -e "${GREEN}✓ Production logging configured appropriately${NC}"
    else
        echo -e "${YELLOW}⚠ Production logging level may be too verbose${NC}"
        ((WARNING_COUNT++))
    fi
fi

# Check 10: Git status
echo -e "${YELLOW}[10/10] Checking Git status...${NC}"
if [ -n "$(git status --porcelain)" ]; then
    echo -e "${YELLOW}⚠ Uncommitted changes found:${NC}"
    git status --short
    ((WARNING_COUNT++))
else
    echo -e "${GREEN}✓ All changes committed${NC}"
fi

# Summary
echo ""
echo "================================================="
echo -e "${CYAN}Validation Summary${NC}"
echo "================================================="
if [ $ERROR_COUNT -eq 0 ]; then
    echo -e "${GREEN}Errors:   $ERROR_COUNT${NC}"
else
    echo -e "${RED}Errors:   $ERROR_COUNT${NC}"
fi

if [ $WARNING_COUNT -eq 0 ]; then
    echo -e "${GREEN}Warnings: $WARNING_COUNT${NC}"
else
 echo -e "${YELLOW}Warnings: $WARNING_COUNT${NC}"
fi
echo ""

if [ $ERROR_COUNT -eq 0 ]; then
    echo -e "${GREEN}✓ Validation passed! Ready for deployment.${NC}"
echo ""
 echo -e "${CYAN}Next steps:${NC}"
    echo "1. Review DEPLOYMENT.md for deployment options"
    echo "2. Configure production secrets (not in source control)"
    echo "3. Run: dotnet publish -c Release -o ./publish"
    echo "4. Deploy using your preferred method (Azure/IIS/Docker)"
    exit 0
else
    echo -e "${RED}✗ Validation failed. Please fix errors before deploying.${NC}"
    exit 1
fi
