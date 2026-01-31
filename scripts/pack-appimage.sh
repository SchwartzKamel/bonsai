#!/usr/bin/env bash
set -euo pipefail

PUBLISH_DIR=${1:-}
OUT_DIR=${2:-artifacts}
APP_NAME="Bonsai"
APPDIR="$PWD/AppDir-new"

require_cmd() {
  command -v "$1" >/dev/null 2>&1 || { echo "ERROR: required command '$1' not found. Install it and retry."; exit 1; }
}

if [ -z "$PUBLISH_DIR" ]; then
  echo "Usage: $0 <publish_dir> [out_dir]"
  exit 2
fi

if [ ! -d "$PUBLISH_DIR" ]; then
  echo "ERROR: publish dir '$PUBLISH_DIR' does not exist. Run: make publish-linux or make publish-all";
  exit 1
fi

mkdir -p "$OUT_DIR"

# Create AppDir from scratch, no template dependency
rm -rf "$APPDIR"
mkdir -p "$APPDIR/usr/bin"
mkdir -p "$APPDIR/usr/share/applications"
mkdir -p "$APPDIR/usr/share/icons"

# Copy published files into AppDir
cp -r "$PUBLISH_DIR"/* "$APPDIR/usr/bin/"

# Create desktop file at root for AppImage requirement
cat > "$APPDIR/Bonsai.desktop" <<EOF
[Desktop Entry]
Name=Bonsai
Exec=Bonsai.UI
Icon=bonsai
Type=Application
Categories=Utility;
EOF

# Also create in applications dir
cp "$APPDIR/Bonsai.desktop" "$APPDIR/usr/share/applications/Bonsai.desktop"

# Copy icon to root and icons dir - use project image
if [ -f "docs/img/bonsai_mvp.png" ]; then
  cp "docs/img/bonsai_mvp.png" "$APPDIR/bonsai.png"
  cp "docs/img/bonsai_mvp.png" "$APPDIR/usr/share/icons/bonsai.png"
  echo "Icon copied from docs/img/bonsai_mvp.png"
else
  echo "Warning: Icon not found at docs/img/bonsai_mvp.png"
fi

# Download or use AppRun from AppImageKit if available
if [ -f "AppImageKit/AppRun-x86_64" ]; then
  cp "AppImageKit/AppRun-x86_64" "$APPDIR/AppRun"
else
  # Download AppRun if not present
  wget -q https://github.com/AppImage/AppImageKit/releases/download/continuous/AppRun-x86_64 -O "$APPDIR/AppRun" || {
    echo "Failed to download AppRun, AppImage build may fail"
  }
fi
chmod +x "$APPDIR/AppRun"

echo "Debug: About to build AppImage. Listing AppDir contents"
ls -la "$APPDIR"
echo "Debug: Desktop file content:"
cat "$APPDIR/usr/share/applications/Bonsai.desktop"

# Build AppImage
require_cmd appimagetool
appimagetool "$APPDIR" "$OUT_DIR/${APP_NAME}-x86_64.AppImage"
echo "AppImage created at $OUT_DIR/${APP_NAME}-x86_64.AppImage"
