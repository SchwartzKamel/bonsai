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
rm -rf "$APPDIR"
mkdir -p "$APPDIR/usr/bin"

# Copy published files into AppDir
cp -r "$PUBLISH_DIR"/* "$APPDIR/usr/bin/"

# Create a basic .desktop file
mkdir -p "$APPDIR/usr/share/applications"
cat > "$APPDIR/usr/share/applications/$APP_NAME.desktop" <<EOF
[Desktop Entry]
Name=$APP_NAME
Exec=$APP_NAME
Icon=$APP_NAME
Type=Application
Categories=Utility;
EOF

# Basic icon placeholder if none provided
if [ ! -f "$APPDIR/usr/share/icons/$APP_NAME.png" ]; then
  mkdir -p "$APPDIR/usr/share/icons"
  cp "installer/BonsaiIcon.ico" "$APPDIR/usr/share/icons/$APP_NAME.png" 2>/dev/null || true
fi

# Ensure AppRun exists (fallback to AppRun from AppImageKit if present)
if [ ! -f "$APPDIR/AppRun" ]; then
  if [ -f "AppImageKit/AppRun-x86_64" ]; then
    cp "AppImageKit/AppRun-x86_64" "$APPDIR/AppRun"
    chmod +x "$APPDIR/AppRun"
  fi
fi

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
