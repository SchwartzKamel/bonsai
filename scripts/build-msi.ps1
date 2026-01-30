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

if (-not (Get-Command candle -ErrorAction SilentlyContinue)) {
  Write-Host "WiX 'candle' not found. Try installing via choco: choco install wixtoolset"
}

# Set variables for candle/light
$wxs = Join-Path -Path "installer" -ChildPath "Bonsai.Product.wxs"
$wixobj = [System.IO.Path]::ChangeExtension($wxs, '.wixobj')

# Replace BinariesDir var usage when running candle/light
$candleCmd = "candle -dBinariesDir=$BinariesDir $wxs"
Write-Host "Running: $candleCmd"
try {
  iex $candleCmd
} catch {
  Write-Warning "candle failed (maybe WiX not installed). Skipping actual MSI compilation in stub."
}

# If wix light is available, produce the msi
if (Get-Command light -ErrorAction SilentlyContinue) {
  $msiOut = Join-Path -Path $OutputDir -ChildPath "Bonsai-1.0.0.msi"
  New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
  $lightCmd = "light -ext WixUIExtension -o $msiOut $wixobj"
  Write-Host "Running: $lightCmd"
  iex $lightCmd
  Write-Host "MSI created at $msiOut"
} else {
  Write-Warning "light.exe not found. Generate MSI locally on a machine with WiX installed. As a fallback, copying exe to artifacts."
  New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
  Copy-Item -Path (Join-Path $BinariesDir '*') -Destination $OutputDir -Recurse -Force
}
