# Setup script for local development (PowerShell)
# This script clones nopCommerce at the correct version if it's not already present

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ParentDir = Split-Path -Parent $ScriptDir
$NopCommerceDir = Join-Path $ParentDir "nopCommerce"
$NopCommerceVersion = if ($env:NOPCOMMERCE_VERSION) { $env:NOPCOMMERCE_VERSION } else { "release-4.90.3" }

Write-Host "🚀 Setting up development environment for API plugin" -ForegroundColor Cyan
Write-Host ""

# Check if nopCommerce directory exists
if (Test-Path $NopCommerceDir) {
    Write-Host "📁 nopCommerce directory already exists at: $NopCommerceDir" -ForegroundColor Yellow
    
    # Check if it's a git repository
    if (Test-Path (Join-Path $NopCommerceDir ".git")) {
        Push-Location $NopCommerceDir
        try {
            $CurrentVersion = git describe --tags --exact-match 2>$null
            if (-not $CurrentVersion) {
                $CurrentVersion = git rev-parse --short HEAD
            }
            Write-Host "   Current version: $CurrentVersion" -ForegroundColor Gray
            
            if ($CurrentVersion -ne $NopCommerceVersion) {
                Write-Host ""
                Write-Host "⚠️  Warning: nopCommerce is at $CurrentVersion but plugin expects $NopCommerceVersion" -ForegroundColor Yellow
                Write-Host "   You may want to checkout the correct version:" -ForegroundColor Yellow
                Write-Host "   cd $NopCommerceDir; git checkout $NopCommerceVersion" -ForegroundColor Gray
            }
        }
        finally {
            Pop-Location
        }
    }
    else {
        Write-Host "   ⚠️  Directory exists but is not a git repository" -ForegroundColor Yellow
    }
}
else {
    Write-Host "📦 Cloning nopCommerce at $NopCommerceVersion..." -ForegroundColor Cyan
    git clone --branch $NopCommerceVersion --depth 1 `
        https://github.com/nopSolutions/nopCommerce.git $NopCommerceDir
    Write-Host "   ✅ nopCommerce cloned successfully" -ForegroundColor Green
}

Write-Host ""
Write-Host "🔨 Building nopCommerce..." -ForegroundColor Cyan
Push-Location (Join-Path $NopCommerceDir "src")
try {
    dotnet restore
    dotnet build --configuration Release --no-restore
}
finally {
    Pop-Location
}

Write-Host ""
Write-Host "🔨 Building API plugin..." -ForegroundColor Cyan
Push-Location $ScriptDir
try {
    dotnet restore
    dotnet build --configuration Release --no-restore
}
finally {
    Pop-Location
}

Write-Host ""
Write-Host "✅ Setup complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Run the Nop.Web project: cd $NopCommerceDir\src\Presentation\Nop.Web; dotnet run" -ForegroundColor Gray
Write-Host "  2. Complete nopCommerce installation in browser" -ForegroundColor Gray
Write-Host "  3. Install the API plugin from Admin > Configuration > Local plugins" -ForegroundColor Gray
Write-Host "  4. Visit /api/swagger to explore the API" -ForegroundColor Gray
Write-Host ""
