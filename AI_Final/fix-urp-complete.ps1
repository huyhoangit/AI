# Complete URP Recovery Script for Unity Project
# This script fixes URP package loading and shader issues

Write-Host "🔧 Starting Complete URP Recovery Process..." -ForegroundColor Cyan

# Stop any running Unity processes
Write-Host "📴 Stopping Unity processes..."
Get-Process Unity* -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

# Wait for processes to fully stop
Start-Sleep -Seconds 3

# Clear all Unity cache and temporary files
Write-Host "🧹 Clearing Unity cache and temp files..."

$projectPath = Get-Location
$libraryPath = Join-Path $projectPath "Library"
$tempPath = Join-Path $projectPath "Temp"
$logsPath = Join-Path $projectPath "Logs"
$objPath = Join-Path $projectPath "obj"
$binPath = Join-Path $projectPath "bin"

# Remove cache directories
if (Test-Path $libraryPath) {
    Write-Host "  Removing Library folder..."
    Remove-Item $libraryPath -Recurse -Force -ErrorAction SilentlyContinue
}

if (Test-Path $tempPath) {
    Write-Host "  Removing Temp folder..."
    Remove-Item $tempPath -Recurse -Force -ErrorAction SilentlyContinue
}

if (Test-Path $logsPath) {
    Write-Host "  Removing Logs folder..."
    Remove-Item $logsPath -Recurse -Force -ErrorAction SilentlyContinue
}

if (Test-Path $objPath) {
    Write-Host "  Removing obj folder..."
    Remove-Item $objPath -Recurse -Force -ErrorAction SilentlyContinue
}

if (Test-Path $binPath) {
    Write-Host "  Removing bin folder..."
    Remove-Item $binPath -Recurse -Force -ErrorAction SilentlyContinue
}

# Clear package lock and resolve files
$packagesPath = Join-Path $projectPath "Packages"
$manifestLockPath = Join-Path $packagesPath "packages-lock.json"

if (Test-Path $manifestLockPath) {
    Write-Host "  Removing packages-lock.json..."
    Remove-Item $manifestLockPath -Force -ErrorAction SilentlyContinue
}

# Clear global Unity cache
Write-Host "🌐 Clearing global Unity cache..."
$globalCachePaths = @(
    "$env:LOCALAPPDATA\Unity\cache",
    "$env:APPDATA\Unity\cache",
    "$env:TEMP\Unity",
    "$env:LOCALAPPDATA\Temp\Unity"
)

foreach ($cachePath in $globalCachePaths) {
    if (Test-Path $cachePath) {
        Write-Host "  Clearing $cachePath..."
        Remove-Item $cachePath -Recurse -Force -ErrorAction SilentlyContinue
    }
}

# Reset package manifest to force fresh package resolution
Write-Host "📦 Resetting package manifest..."
$manifestPath = Join-Path $packagesPath "manifest.json"

if (Test-Path $manifestPath) {
    # Create backup
    $backupPath = Join-Path $packagesPath "manifest.json.backup"
    Copy-Item $manifestPath $backupPath -Force
    
    # Read current manifest
    $manifest = Get-Content $manifestPath -Raw | ConvertFrom-Json
    
    # Ensure URP and core packages are explicitly listed
    if (-not $manifest.dependencies."com.unity.render-pipelines.universal") {
        $manifest.dependencies | Add-Member -MemberType NoteProperty -Name "com.unity.render-pipelines.universal" -Value "14.0.11" -Force
    }
    
    if (-not $manifest.dependencies."com.unity.render-pipelines.core") {
        $manifest.dependencies | Add-Member -MemberType NoteProperty -Name "com.unity.render-pipelines.core" -Value "14.0.11" -Force
    }
    
    # Write updated manifest
    $manifest | ConvertTo-Json -Depth 10 | Set-Content $manifestPath
    Write-Host "  ✅ Updated manifest.json with explicit URP packages"
}

# Create URP recovery instructions
Write-Host "📋 Creating URP recovery instructions..."
$instructionsPath = Join-Path $projectPath "URP_EMERGENCY_RECOVERY.md"
$instructions = @"
# URP Emergency Recovery Instructions

## Current Status
- Unity cache and temp files cleared
- Package manifest reset with explicit URP dependencies
- Global Unity cache cleared

## Next Steps in Unity Editor:

### 1. Open Unity Project
1. Open Unity Hub
2. Open this project: $projectPath
3. **WAIT** for Unity to fully load and resolve packages (this may take 5-10 minutes)

### 2. If URP Errors Persist:
1. Go to **Window > Package Manager**
2. In dropdown, select "**In Project**"
3. Find "**Universal RP**" package
4. Click **Remove** then **Install** (or click **Update** if available)
5. Wait for reinstallation to complete

### 3. Alternative: Switch to Built-in Pipeline (if URP keeps failing)
1. Go to **Edit > Project Settings**
2. Select **Graphics**
3. Change **Scriptable Render Pipeline Settings** to **None (Built-in Render Pipeline)**
4. Click **Apply**

### 4. Fix Any Remaining Material Issues:
1. Go to **Edit > Render Pipeline > Universal Render Pipeline > Convert Project Materials to URP**
2. Or manually reassign shaders to materials in the **Materials** folder

### 5. Verify Project Works:
1. Open the main scene: **Assets/Scenes/QuoridorGameScene**
2. Press **Play** to test the game
3. Check that all materials render correctly

## Troubleshooting:
- If errors persist, try switching to Built-in Pipeline (Step 3)
- If materials are pink, reassign shaders manually
- If persistent issues, consider creating a new project and importing assets

## Files Modified:
- Packages/manifest.json (URP dependencies added)
- All cache and temp files cleared

**Generated**: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
"@

Set-Content $instructionsPath $instructions

Write-Host ""
Write-Host "✅ URP Recovery Complete!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Next Steps:" -ForegroundColor Yellow
Write-Host "1. Open Unity Editor (it will take longer than usual to load)"
Write-Host "2. Let Unity resolve packages completely"
Write-Host "3. Follow instructions in: URP_EMERGENCY_RECOVERY.md"
Write-Host "4. If issues persist, consider switching to Built-in Render Pipeline"
Write-Host ""
Write-Host "Important: Do NOT interrupt Unity while it's resolving packages!" -ForegroundColor Red
Write-Host ""
