[CmdletBinding()]
param(
  [Parameter(Mandatory=$true)][string]$BinariesDir,
  [Parameter(Mandatory=$false)][string]$OutputDir = "artifacts"
)

$ErrorActionPreference = 'Stop'

Write-Host "BinariesDir: $BinariesDir"
Write-Host "OutputDir: $OutputDir"

if (-not (Test-Path $BinariesDir)) {
  Write-Error "BinariesDir does not exist: $BinariesDir"
  exit 1
}

# Normalize path to Windows separators
$BinariesDir = $BinariesDir -replace '/', '\'

# Verify key files exist
$exePath = Join-Path $BinariesDir "Bonsai.UI.exe"
if (-not (Test-Path $exePath)) {
  Write-Error "Bonsai.UI.exe not found in $BinariesDir"
  exit 1
}

$iconPath = Join-Path $BinariesDir "BonsaiIcon.ico"
if (-not (Test-Path $iconPath)) {
  Write-Warning "BonsaiIcon.ico not found in $BinariesDir - icon component may fail"
}

if (-not (Get-Command candle -ErrorAction SilentlyContinue)) {
  Write-Host "WiX 'candle' not found. Try installing via choco: choco install wixtoolset"
}

# Set variables for candle/light
$wxs = Join-Path -Path "installer" -ChildPath "Bonsai.Product.wxs"
$wixobj = [System.IO.Path]::ChangeExtension($wxs, '.wixobj')

# Use normalized BinariesDir for candle/light
# Use wix build for WiX v4 (basic, without UI extension on Linux)
$msiOut = Join-Path -Path $OutputDir -ChildPath "Bonsai-1.0.1.msi"
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$wixBuildCmd = "wix build `"$wxs`" -o `"$msiOut`" -d BinariesDir=`"$BinariesDir`""
Write-Host "Running: $wixBuildCmd"
try {
  iex $wixBuildCmd
  Write-Host "Basic MSI created at $msiOut (no UI extension on Linux)"
} catch {
  Write-Warning "wix build failed. As a fallback, copying exe to artifacts."
  Copy-Item -Path (Join-Path $BinariesDir 'Bonsai.UI.exe') -Destination $OutputDir -Force
}
