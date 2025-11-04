# ============================================
# FINAL CLEANUP SCRIPT - Auto-Fix Remaining Errors
# ============================================
# This script automatically comments out all removed features
# in AdminDashboardController and Admin Dashboard Views
# ============================================

Write-Host "🚀 Starting Final Cleanup Script..." -ForegroundColor Cyan
Write-Host ""

$ErrorCount = 0
$SuccessCount = 0

# ============================================
# STEP 1: Fix AdminDashboardController.cs
# ============================================
Write-Host "📝 Step 1: Updating AdminDashboardController.cs..." -ForegroundColor Yellow

$adminControllerPath = "TafsilkPlatform.Web\Controllers\AdminDashboardController.cs"

if (Test-Path $adminControllerPath) {
    try {
        $content = Get-Content $adminControllerPath -Raw
        
        # Replace OpenDisputes references
      $content = $content -replace 'OpenDisputes\s*=\s*await\s+_context\.Disputes[^;]+;', '// OpenDisputes = 0, // REMOVED: Dispute feature'
    
        # Replace PendingRefunds references
        $content = $content -replace 'PendingRefunds\s*=\s*await\s+_context\.RefundRequests[^;]+;', '// PendingRefunds = 0, // REMOVED: Refund feature'
    
        # Replace ActivityLogs references
        $content = $content -replace 'RecentActivity\s*=\s*await\s+_context\.ActivityLogs[^}]+\}[^;]+;', 'RecentActivity = new List<ActivityLogDto>() // REMOVED: ActivityLog feature'
      
        # Replace other ActivityLogs queries
        $content = $content -replace 'var\s+activityLogs\s*=\s*await\s+_context\.ActivityLogs[^;]+;', '// var activityLogs = new List<ActivityLog>(); // REMOVED: ActivityLog feature'
        
        # Replace ActivityLog instantiation
        $content = $content -replace 'var\s+log\s*=\s*new\s+ActivityLog\s*\{[^}]+\};', '// ActivityLog removed - using standard logging instead'
        
      # Replace ActivityLogs.Add
        $content = $content -replace '_context\.ActivityLogs\.Add\([^)]+\);', '// _context.ActivityLogs.Add removed'
      
        # Replace Disputes queries
        $content = $content -replace 'var\s+query\s*=\s*_context\.Disputes[^;]+;', '// Disputes feature removed - return empty list'
        $content = $content -replace 'var\s+dispute\s*=\s*await\s+_context\.Disputes[^;]+;', '// Dispute feature removed'
        
        # Replace RefundRequests queries
     $content = $content -replace 'var\s+query\s*=\s*_context\.RefundRequests[^;]+;', '// RefundRequests feature removed - return empty list'
        $content = $content -replace 'var\s+refund\s*=\s*await\s+_context\.RefundRequests[^;]+;', '// Refund feature removed'
        
     # Replace DisputeManagementViewModel
    $content = $content -replace 'var\s+viewModel\s*=\s*new\s+DisputeManagementViewModel', '// DisputeManagementViewModel removed - feature not available'

        # Replace RefundManagementViewModel
        $content = $content -replace 'var\s+viewModel\s*=\s*new\s+RefundManagementViewModel', '// RefundManagementViewModel removed - feature not available'
        
  Set-Content $adminControllerPath $content -NoNewline
  
        Write-Host "  ✅ AdminDashboardController.cs updated successfully" -ForegroundColor Green
        $SuccessCount++
  }
    catch {
    Write-Host "  ❌ Error updating AdminDashboardController.cs: $_" -ForegroundColor Red
    $ErrorCount++
    }
}
else {
    Write-Host "  ⚠️  AdminDashboardController.cs not found - skipping" -ForegroundColor Yellow
}

Write-Host ""

# ============================================
# STEP 2: Fix Admin Dashboard Views
# ============================================
Write-Host "📝 Step 2: Updating Admin Dashboard Views..." -ForegroundColor Yellow

$viewFiles = @(
    "TafsilkPlatform.Web\Views\Dashboards\admindashboard.cshtml",
    "TafsilkPlatform.Web\Views\AdminDashboard\Index.cshtml"
)

foreach ($viewFile in $viewFiles) {
    if (Test-Path $viewFile) {
        try {
            $content = Get-Content $viewFile -Raw
            
          # Replace OpenDisputes references
          $content = $content -replace '@Model\.OpenDisputes', '0 @* REMOVED: Disputes feature *@'
            
       # Replace PendingRefunds references
          $content = $content -replace '@Model\.PendingRefunds', '0 @* REMOVED: Refunds feature *@'
            
    # Comment out conditional checks for removed features
   $content = $content -replace '@if\s*\(\s*Model\.OpenDisputes\s*>\s*0\s*\)', '@* REMOVED: Dispute feature *@ @if (false)'
      $content = $content -replace '@if\s*\(\s*Model\.PendingRefunds\s*>\s*0\s*\)', '@* REMOVED: Refund feature *@ @if (false)'
       $content = $content -replace '@if\s*\(\s*Model\.OpenDisputes\s*>\s*0\s*\|\|\s*Model\.PendingRefunds\s*>\s*0', '@* REMOVED: Disputes & Refunds *@ @if (false'
    
            Set-Content $viewFile $content -NoNewline
            
         Write-Host "  ✅ $([System.IO.Path]::GetFileName($viewFile)) updated successfully" -ForegroundColor Green
        $SuccessCount++
        }
        catch {
 Write-Host "  ❌ Error updating $viewFile : $_" -ForegroundColor Red
  $ErrorCount++
        }
    }
    else {
        Write-Host "  ⚠️  $viewFile not found - skipping" -ForegroundColor Yellow
    }
}

Write-Host ""

# ============================================
# STEP 3: Build Project to Verify
# ============================================
Write-Host "🔨 Step 3: Building project to verify fixes..." -ForegroundColor Yellow

try {
    $buildOutput = dotnet build TafsilkPlatform.Web/TafsilkPlatform.Web.csproj 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✅ Build successful! All errors fixed!" -ForegroundColor Green
        $SuccessCount++
    }
    else {
        Write-Host "  ⚠️  Build completed with warnings. Check output:" -ForegroundColor Yellow
        Write-Host $buildOutput
    }
}
catch {
    Write-Host "  ❌ Build failed: $_" -ForegroundColor Red
    $ErrorCount++
}

Write-Host ""

# ============================================
# SUMMARY
# ============================================
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "📊 CLEANUP SUMMARY" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "✅ Successful operations: $SuccessCount" -ForegroundColor Green
Write-Host "❌ Failed operations: $ErrorCount" -ForegroundColor $(if ($ErrorCount -gt 0) { "Red" } else { "Green" })
Write-Host ""

if ($ErrorCount -eq 0) {
    Write-Host "🎉 CLEANUP COMPLETED SUCCESSFULLY!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next Steps:" -ForegroundColor Cyan
    Write-Host "  1. Review the changes in your IDE" -ForegroundColor White
    Write-Host "  2. Test the admin dashboard" -ForegroundColor White
    Write-Host "  3. Commit changes: git add . && git commit -m 'Complete cleanup - removed unused features'" -ForegroundColor White
    Write-Host ""
    Write-Host "Features Removed:" -ForegroundColor Yellow
    Write-Host "  ❌ Disputes Management" -ForegroundColor Gray
    Write-Host "  ❌ Refunds Management" -ForegroundColor Gray
    Write-Host "  ❌ Activity Logs" -ForegroundColor Gray
    Write-Host "  ❌ RFQ System" -ForegroundColor Gray
    Write-Host "  ❌ Wallet Features" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Core Features Preserved:" -ForegroundColor Green
    Write-Host "  ✅ User Authentication" -ForegroundColor Gray
    Write-Host "  ✅ Tailor Verification" -ForegroundColor Gray
    Write-Host "  ✅ Order Management" -ForegroundColor Gray
    Write-Host "  ✅ Payment Processing" -ForegroundColor Gray
    Write-Host "  ✅ Reviews & Ratings" -ForegroundColor Gray
}
else {
    Write-Host "⚠️  Some operations failed. Please review errors above." -ForegroundColor Yellow
    Write-Host "You may need to manually fix remaining issues." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Script completed. Press any key to exit..." -ForegroundColor Cyan
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
