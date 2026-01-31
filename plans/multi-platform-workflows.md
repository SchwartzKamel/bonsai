+++ 
id = \"GITHUB-WORKFLOW-MULTI-PLATFORM-PLAN\"
title = \"GitHub Workflows Multi-Platform Implementation Plan\"
status = \"planned\"
last_updated = \"2026-01-31\"
tags = [\"ci-cd\", \"multi-platform\", \".net8\", \"avalonia\", \"blazor\", \"maui\"]
+++

# GitHub Workflows for Multi-Platform Bonsai App

## Overview
This document details proposed changes to GitHub workflows to support Desktop (Avalonia), Web (Blazor WASM), and Android (MAUI) builds on .NET 8.0. Changes include .NET version updates, multi-TFM/RID matrices, and platform-specific jobs.

## Common Changes Across Workflows
- Replace `dotnet-version: '10.x'` with `'8.x'`.
- Add matrix for `platform: [desktop, web, android]` where applicable.
- Use `--framework net8.0` in dotnet commands.
- Extend RIDs: Add `osx-arm64`, `android-arm64`.
- Cache: Include platform-specific keys.

## 1. ci-test.yml Updates
**Purpose:** Test all platforms across OS/TFM.

**Proposed Full Rewrite (Key Sections):**
```yaml
name: CI Test
on:
  push:
    branches: [ main ]
    tags: [ 'v*' ]
  pull_request:
  workflow_dispatch:

permissions:
  checks: write

jobs:
  test:
    name: Test on ${{ matrix.os }} (net10.0)
    runs-on: ${{ matrix.os }}
    strategy:
      matrix:
        os: [ubuntu-latest, macos-latest, windows-latest]
      fail-fast: false
    steps:
      - uses: actions/checkout@v4

      - name: Cache NuGet packages
        uses: actions/cache@v4
        with:
          path: ~/.nuget/packages
          key: nuget-${{ runner.os }}-${{ hashFiles('**/*.csproj') }}
          restore-keys: nuget-${{ runner.os }}-

      - name: Setup .NET 10
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.x'

      - name: Install Linux prerequisites
        if: runner.os == 'Linux'
        run: |
          sudo apt-get update
          sudo apt-get install -y libgtk-3-0 libgdk-pixbuf2.0-0 libx11-6 xvfb fuse

      - name: Start Xvfb (Linux)
        if: runner.os == 'Linux'
        run: |
          Xvfb :99 -screen 0 1280x1024x24 &
          sleep 2
          echo "DISPLAY=:99" >> $GITHUB_ENV

      - name: Restore and Build
        run: |
          dotnet restore
          dotnet build -c Release --no-restore

      - name: Run tests (non-Windows)
        if: runner.os != 'Windows'
        env:
          AVALONIA_HEADLESS: '1'
        run: |
          dotnet test --logger "trx;LogFileName=TestResults.trx" --results-directory ./test-results -c Release --no-build

      - name: Run tests (Windows)
        if: runner.os == 'Windows'
        run: |
          dotnet test --logger "trx;LogFileName=TestResults.trx" --results-directory ./test-results -c Release --no-build

      # Existing conversion and upload steps unchanged

      # Platform-specific setup
      - name: Setup Android (MAUI)
        if: matrix.platform == 'android'
        uses: microsoft/setup-msbuild@v2  # For MAUI/Android tools

      - name: Install Linux deps (Desktop)
        if: runner.os == 'Linux' && matrix.platform == 'desktop'
        run: sudo apt-get update && sudo apt-get install -y libgtk-3-0 libgdk-pixbuf2.0-0 libx11-6 xvfb

      - name: Start Xvfb (Linux Desktop)
        if: runner.os == 'Linux' && matrix.platform == 'desktop'
        run: Xvfb :99 -screen 0 1280x1024x24 & sleep 2 && echo \"DISPLAY=:99\" >> $GITHUB_ENV

      - name: Restore
        run: dotnet restore

      - name: Build
        run: dotnet build -c Release --no-restore --framework net8.0

      - name: Test Desktop (headless)
        if: matrix.platform == 'desktop'
        env:
          AVALONIA_HEADLESS: '1'
        run: dotnet test src/Bonsai.Desktop.Tests --logger \"trx\" --results-directory ./test-results -c Release --no-build --framework net8.0

      - name: Test Web (Blazor)
        if: matrix.platform == 'web'
        run: dotnet test src/Bonsai.Web.Tests --logger \"trx\" --results-directory ./test-results -c Release --no-build --framework net8.0

      - name: Test Android (MAUI)
        if: matrix.platform == 'android'
        run: dotnet test src/Bonsai.Mobile.Tests --logger \"trx\" --results-directory ./test-results -c Release --no-build --framework net8.0-android

      # Publish test results (existing logic, adapted for platform)
      - name: Publish Test Results
        if: always()
        uses: dorny/test-reporter@v1
        with:
          name: test-${{ matrix.platform }}-${{ matrix.os }}
          path: ./test-results/*.trx
          reporter: dotnet-trx

      - name: Upload Artifacts
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: test-results-${{ matrix.platform }}-${{ matrix.os }}
          path: ./test-results/**
```

## 2. publish-self-contained.yml Updates
**Purpose:** Publish self-contained binaries for all platforms.

**Proposed Key Changes (Matrix Extension):**
- Matrix include:
  - Desktop Linux: ubuntu-latest, linux-x64, desktop
  - Desktop macOS x64: macos-latest, osx-x64, desktop
  - Desktop macOS arm64: macos-latest, osx-arm64, desktop
  - Desktop Windows: windows-latest, win-x64, desktop
  - Web: ubuntu-latest, browser-wasm, web
  - Android: windows-latest, android-arm64, android
- Publish step:
  ```yaml
  - name: Publish ${{ matrix.platform }}
    run: |
      if [ \"${{ matrix.platform }}\" = \"desktop\" ]; then
        dotnet publish src/Bonsai.Desktop -c Release -f net8.0 -r ${{ matrix.rid }} --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=true -o ./publish/${{ matrix.rid }}
      elif [ \"${{ matrix.platform }}\" = \"web\" ]; then
        dotnet publish src/Bonsai.Web.Client -c Release -f net8.0 --self-contained false -o ./publish/web
      elif [ \"${{ matrix.platform }}\" = \"android\" ]; then
        dotnet publish src/Bonsai.Mobile -c Release -f net8.0-android -r ${{ matrix.rid }} --self-contained true -o ./publish/${{ matrix.rid }}
      fi
  ```
- Packaging: Conditional based on platform (AppImage for desktop Linux, DMG for macOS, MSI for Windows, AAB/APK for Android, static files for Web).

## 3. build-msi.yml Updates
**Purpose:** Windows MSI for Desktop only.

**Proposed Changes:**
- Update to `dotnet-version: '8.x'`
- Publish: `-f net8.0`
- Keep WiX install and script call.

## 4. release.yml Updates
**Purpose:** Create release with all platform artifacts.

**Proposed Key Changes:**
- Matrix for platforms as in publish-self-contained.
- Download artifacts per platform.
- Use softprops/action-gh-release to attach all (e.g., MSI, AppImage, DMG, APK, web-dist.zip).

## Implementation Notes
- Create new projects: Bonsai.Web (Blazor), Bonsai.Mobile (MAUI) in Code mode.
- Add tests per platform.
- global.json: Pin to .NET 8.0.100.
- Directory.Build.props: Set `<TargetFrameworks>net8.0</TargetFrameworks>` for shared libs; platforms override for specifics (e.g., net8.0-windows for Desktop).

This completes the proposal. Proceed to Code mode for creation/edits.