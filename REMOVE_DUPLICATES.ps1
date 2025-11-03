# Remove Duplicate Methods from AccountController.cs

$file = "TafsilkPlatform.Web\Controllers\AccountController.cs"
$backup = "$file.backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"

# Create backup
Write-Host "Creating backup: $backup"
Copy-Item $file $backup

# Read all lines
$lines = Get-Content $file

Write-Host "Original file: $($lines.Length) lines"

# Strategy: Remove duplicate sections
# Lines to REMOVE:
# - 773-923: First CompleteTailorProfile (old version without TailorPolicy)
# - 987-1050: Duplicate VerifyEmail and ResendVerificationEmail

# Lines to KEEP:
# - 1-772: Everything before first duplicate
# - 924-986: VerifyEmail and ResendVerificationEmail (first occurrence)
# - 1051-end: ProvideTailorEvidence and final CompleteTailorProfile with TailorPolicy

$newContent = @()

# Keep lines 1-772
Write-Host "Keeping lines 1-772..."
$newContent += $lines[0..771]

# Skip lines 773-923 (first CompleteTailorProfile duplicate)
Write-Host "Skipping lines 773-923 (duplicate CompleteTailorProfile)..."

# Keep lines 924-986 (VerifyEmail and ResendVerificationEmail)
Write-Host "Keeping lines 924-986 (VerifyEmail, ResendVerificationEmail)..."
$newContent += $lines[923..985]

# Skip lines 987-1050 (duplicate VerifyEmail and ResendVerificationEmail)
Write-Host "Skipping lines 987-1050 (duplicate VerifyEmail, ResendVerificationEmail)..."

# Keep lines 1051-end (ProvideTailorEvidence and final CompleteTailorProfile)
Write-Host "Keeping lines 1051-$($lines.Length) (ProvideTailorEvidence, final CompleteTailorProfile)..."
$newContent += $lines[1050..($lines.Length-1)]

# Write new content
$newContent | Set-Content $file -Encoding UTF8

Write-Host "✅ Done! New file: $($newContent.Length) lines (removed $($lines.Length - $newContent.Length) lines)"
Write-Host "✅ Backup saved to: $backup"
Write-Host ""
Write-Host "Please verify the changes and run: dotnet build"
