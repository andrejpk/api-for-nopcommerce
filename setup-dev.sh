#!/bin/bash
# Setup script for local development
# This script clones nopCommerce at the correct version if it's not already present

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PARENT_DIR="$(dirname "$SCRIPT_DIR")"
NOPCOMMERCE_DIR="$PARENT_DIR/nopCommerce"
NOPCOMMERCE_VERSION="${NOPCOMMERCE_VERSION:-release-4.90.3}"

echo "🚀 Setting up development environment for API plugin"
echo ""

# Check if nopCommerce directory exists
if [ -d "$NOPCOMMERCE_DIR" ]; then
    echo "📁 nopCommerce directory already exists at: $NOPCOMMERCE_DIR"
    
    # Check if it's a git repository
    if [ -d "$NOPCOMMERCE_DIR/.git" ]; then
        cd "$NOPCOMMERCE_DIR"
        CURRENT_VERSION=$(git describe --tags --exact-match 2>/dev/null || git rev-parse --short HEAD)
        echo "   Current version: $CURRENT_VERSION"
        
        if [ "$CURRENT_VERSION" != "$NOPCOMMERCE_VERSION" ]; then
            echo ""
            echo "⚠️  Warning: nopCommerce is at $CURRENT_VERSION but plugin expects $NOPCOMMERCE_VERSION"
            echo "   You may want to checkout the correct version:"
            echo "   cd $NOPCOMMERCE_DIR && git checkout $NOPCOMMERCE_VERSION"
        fi
    else
        echo "   ⚠️  Directory exists but is not a git repository"
    fi
else
    echo "📦 Cloning nopCommerce at $NOPCOMMERCE_VERSION..."
    git clone --branch "$NOPCOMMERCE_VERSION" --depth 1 \
        https://github.com/nopSolutions/nopCommerce.git "$NOPCOMMERCE_DIR"
    echo "   ✅ nopCommerce cloned successfully"
fi

echo ""
echo "🔨 Building nopCommerce..."
cd "$NOPCOMMERCE_DIR/src"
dotnet restore
dotnet build --configuration Release --no-restore

echo ""
echo "🔨 Building API plugin..."
cd "$SCRIPT_DIR"
dotnet restore
dotnet build --configuration Release --no-restore

echo ""
echo "✅ Setup complete!"
echo ""
echo "Next steps:"
echo "  1. Run the Nop.Web project: cd $NOPCOMMERCE_DIR/src/Presentation/Nop.Web && dotnet run"
echo "  2. Complete nopCommerce installation in browser"
echo "  3. Install the API plugin from Admin > Configuration > Local plugins"
echo "  4. Visit /api/swagger to explore the API"
echo ""
