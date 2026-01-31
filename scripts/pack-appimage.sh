#!/usr/bin/env bash
set -euo pipefail

PUBLISH_DIR=${1:-}
OUT_DIR=${2:-artifacts}
APP_NAME="Bonsai"
APPDIR="$PWD/AppDir"

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

TEMPLATE_DIR="$PWD/AppDir"
APPDIR="$PWD/AppDir-new"  # Use a different dir to avoid conflict

# Copy template structure
rm -rf "$APPDIR"
if [ -d "$TEMPLATE_DIR" ]; then
  cp -r "$TEMPLATE_DIR" "$APPDIR"
fi

# Ensure directories exist
mkdir -p "$APPDIR/usr/bin"
mkdir -p "$APPDIR/usr/share/applications"
mkdir -p "$APPDIR/usr/share/icons"

# Copy published files into AppDir, overwriting bin
rm -rf "$APPDIR/usr/bin"/*
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

# Also create in applications dir for good measure
mkdir -p "$APPDIR/usr/share/applications"
cp "$APPDIR/Bonsai.desktop" "$APPDIR/usr/share/applications/Bonsai.desktop"

# Copy icon to root for appimagetool - use project image
cp "docs/img/bonsai_mvp.png" "$APPDIR/bonsai.png"
echo "Icon copied from docs/img/bonsai_mvp.png to root"

# Copy icon to icons dir as well
mkdir -p "$APPDIR/usr/share/icons"
cp "docs/img/bonsai_mvp.png" "$APPDIR/usr/share/icons/bonsai.png"
echo "Icon copied to icons dir"

# Copy root desktop if exists in template (fallback)
if [ ! -f "$APPDIR/Bonsai.desktop" ] && [ -f "$TEMPLATE_DIR/Bonsai.desktop" ]; then
  cp "$TEMPLATE_DIR/Bonsai.desktop" "$APPDIR/Bonsai.desktop"
  sed -i 's|Exec=.*|Exec=Bonsai.UI|' "$APPDIR/Bonsai.desktop"
fi

# The desktop file is now created with correct Exec, no need for sed

# Ensure AppRun is executable
chmod +x "$APPDIR/AppRun" 2>/dev/null || true

# Ensure icon exists (should be copied from template)
if [ ! -f "$APPDIR/usr/share/icons/bonsai.png" ]; then
  mkdir -p "$APPDIR/usr/share/icons"
  cp "$TEMPLATE_DIR/usr/share/icons/bonsai.png" "$APPDIR/usr/share/icons/bonsai.png" 2>/dev/null || true
fi

# Ensure AppRun exists (fallback to AppRun from AppImageKit if present)
if [ ! -f "$APPDIR/AppRun" ]; then
  if [ -f "AppImageKit/AppRun-x86_64" ]; then
    cp "AppImageKit/AppRun-x86_64" "$APPDIR/AppRun"
    chmod +x "$APPDIR/AppRun"
  fi
fi

echo "Debug: About to build AppImage. Listing AppDir contents"
ls -la "$APPDIR"
echo "Debug: Listing applications dir"
ls -la "$APPDIR/usr/share/applications/" || echo "Applications dir not found"
echo "Debug: Listing icons dir"
ls -la "$APPDIR/usr/share/icons/" || echo "Icons dir not found"
echo "Debug: Desktop file content:"
cat "$APPDIR/usr/share/applications/Bonsai.desktop" || echo "Desktop file not found"

# Build AppImage if appimagetool present or can be downloaded
if command -v appimagetool >/dev/null 2>&1; then
  appimagetool "$APPDIR" "$OUT_DIR/${APP_NAME}-x86_64.AppImage"
  echo "AppImage created at $OUT_DIR/${APP_NAME}-x86_64.AppImage"
else
  echo "appimagetool not found; attempting to download..."
  if command -v wget >/dev/null 2>&1; then
    wget -q https://github.com/AppImage/AppImageKit/releases/download/continuous/appimagetool-x86_64.AppImage -O appimagetool
  elif command -v curl >/dev/null 2>&1; then
    curl -sL https://github.com/AppImage/AppImageKit/releases/download/continuous/appimagetool-x86_64.AppImage -o appimagetool
  else
    echo "Neither appimagetool nor wget/curl available; creating tarball fallback"
    tar -C "$APPDIR" -czf "$OUT_DIR/${APP_NAME}-linux-x64.tar.gz" .
    echo "Tarball created at $OUT_DIR/${APP_NAME}-linux-x64.tar.gz"
    exit 0
  fi
  chmod +x appimagetool
  ./appimagetool "$APPDIR" "$OUT_DIR/${APP_NAME}-x86_64.AppImage"
  echo "AppImage created at $OUT_DIR/${APP_NAME}-x86_64.AppImage"
fi
