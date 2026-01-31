#!/usr/bin/env bash
set -euo pipefail

PUBLISH_DIR=${1:-}
OUT_DIR=${2:-artifacts}
APP_NAME="Bonsai"
APPDIR="$PWD/AppDir"

if [ -z "$PUBLISH_DIR" ]; then
  echo "Usage: $0 <publish_dir> [out_dir]"
  exit 2
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

# Build AppImage if appimagetool present
if command -v appimagetool >/dev/null 2>&1; then
  appimagetool "$APPDIR" "$OUT_DIR/${APP_NAME}-x86_64.AppImage"
  echo "AppImage created at $OUT_DIR/${APP_NAME}-x86_64.AppImage"
else
  echo "appimagetool not found; creating tarball fallback"
  tar -C "$APPDIR" -czf "$OUT_DIR/${APP_NAME}-linux-x64.tar.gz" .
  echo "Tarball created at $OUT_DIR/${APP_NAME}-linux-x64.tar.gz"
fi
