#!/usr/bin/env bash
set -euo pipefail

PUBLISH_DIR=${1:-}
OUT_DIR=${2:-artifacts}
APP_NAME="Bonsai"
APP_BUNDLE="$APP_NAME.app"

require_cmd() {
  command -v "$1" >/dev/null 2>&1 || { echo "ERROR: required command '$1' not found. Install it and retry."; exit 1; }
}

if [ -z "$PUBLISH_DIR" ]; then
  echo "Usage: $0 <publish_dir> [out_dir]"
  exit 2
fi

if [ ! -d "$PUBLISH_DIR" ]; then
  echo "ERROR: publish dir '$PUBLISH_DIR' does not exist. Run: make publish-mac or make publish-all";
  exit 1
fi

mkdir -p "$OUT_DIR"
rm -rf "$APP_BUNDLE"
mkdir -p "$APP_BUNDLE/Contents/MacOS"

# Copy published files
cp -r "$PUBLISH_DIR"/* "$APP_BUNDLE/Contents/MacOS/"

# Minimal Info.plist
cat > "$APP_BUNDLE/Contents/Info.plist" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
  <dict>
    <key>CFBundleName</key>
    <string>$APP_NAME</string>
    <key>CFBundleExecutable</key>
    <string>$APP_NAME</string>
  </dict>
</plist>
EOF

DMG_OUT="$OUT_DIR/$APP_NAME.dmg"
if command -v hdiutil >/dev/null 2>&1; then
  hdiutil create -volname "$APP_NAME" -srcfolder "$APP_BUNDLE" -ov -format UDZO "$DMG_OUT"
  echo "DMG created at $DMG_OUT"
else
  echo "hdiutil not found. Creating a zip fallback."
  if command -v zip >/dev/null 2>&1; then
    zip -r "$OUT_DIR/$APP_NAME-osx-x64.zip" "$APP_BUNDLE"
    echo "Zip created at $OUT_DIR/$APP_NAME-osx-x64.zip"
  elif command -v tar >/dev/null 2>&1; then
    tar -czf "$OUT_DIR/$APP_NAME-osx-x64.tar.gz" -C "$(pwd)" "$APP_BUNDLE"
    echo "Tarball created at $OUT_DIR/$APP_NAME-osx-x64.tar.gz"
  else
    echo "ERROR: neither hdiutil, zip, nor tar found. Cannot produce macOS distribution on this host.";
    exit 1
  fi
fi
