# Fast Publish to RunASP.NET
# This script optimizes the publishing process for speed

param(
 [switch]$SkipBuild,
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"

Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "  Fast Publish to RunASP.NET - tafsilk.runasp.net" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""

$ProjectPath = "TafsilkPlatform.Web\TafsilkPlatform.Web.csproj"
$PublishProfile = "RunASP"

# Step 1: Clean old publish files (optional but recommended for speed)
if (-not $SkipBuild) {
    Write-Host "[1/4] Cleaning previous builds..." -ForegroundColor Yellow
    dotnet clean $ProjectPath -c Release --nologo -v q
    Write-Host "✓ Clean complete" -ForegroundColor Green
    Write-Host ""
}

# Step 2: Build optimized Release
if (-not $SkipBuild) {
    Write-Host "[2/4] Building Release configuration..." -ForegroundColor Yellow
 $buildArgs = @(
    "build",
        $ProjectPath,
  "-c", "Release",
 "--nologo"
    )
    
    if (-not $Verbose) {
        $buildArgs += "-v", "q"
    }
    
    & dotnet @buildArgs
    
    if ($LASTEXITCODE -ne 0) {
  Write-Host "✗ Build failed!" -ForegroundColor Red
      exit 1
    }
    Write-Host "✓ Build successful" -ForegroundColor Green
    Write-Host ""
}

# Step 3: Publish to RunASP
Write-Host "[3/4] Publishing to RunASP.NET..." -ForegroundColor Yellow
Write-Host "Target: http://tafsilk.runasp.net" -ForegroundColor Cyan

$publishArgs = @(
    "publish",
    $ProjectPath,
    "-c", "Release",
    "/p:PublishProfile=$PublishProfile",
    "/p:Password=oC@3D4w_+K7i",
    "--nologo"
)

if ($SkipBuild) {
    $publishArgs += "--no-build"
}

if (-not $Verbose) {
    $publishArgs += "-v", "q"
}

$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

& dotnet @publishArgs

$stopwatch.Stop()

if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Publish failed!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Common issues:" -ForegroundColor Yellow
    Write-Host "1. Check your internet connection" -ForegroundColor White
    Write-Host "2. Verify RunASP.NET credentials are correct" -ForegroundColor White
    Write-Host "3. Ensure Web Deploy is accessible: site41423.siteasp.net" -ForegroundColor White
    Write-Host "4. Try running with -Verbose flag for more details" -ForegroundColor White
    exit 1
}

Write-Host "✓ Publish successful in $($stopwatch.Elapsed.TotalSeconds.ToString('F2')) seconds" -ForegroundColor Green
Write-Host ""

# Step 4: Verify deployment
Write-Host "[4/4] Verifying deployment..." -ForegroundColor Yellow

try {
    $response = Invoke-WebRequest -Uri "http://tafsilk.runasp.net" -TimeoutSec 30 -UseBasicParsing -ErrorAction SilentlyContinue
    if ($response.StatusCode -eq 200) {
        Write-Host "✓ Site is accessible!" -ForegroundColor Green
    } else {
        Write-Host "⚠ Site returned status: $($response.StatusCode)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "⚠ Could not verify site (may take a few moments to start)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "✓ Deployment Complete!" -ForegroundColor Green
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "🌐 Your site: http://tafsilk.runasp.net" -ForegroundColor Cyan
Write-Host "🎛️  Control Panel: https://panel.runasp.net" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor White
Write-Host "1. Visit http://tafsilk.runasp.net to test your site" -ForegroundColor Gray
Write-Host "2. Configure connection string in RunASP control panel" -ForegroundColor Gray
Write-Host "3. Set up environment variables for OAuth credentials" -ForegroundColor Gray
Write-Host ""

# Open browser
$openBrowser = Read-Host "Open site in browser? (Y/n)"
if ($openBrowser -ne 'n' -and $openBrowser -ne 'N') {
    Start-Process "http://tafsilk.runasp.net"
}
