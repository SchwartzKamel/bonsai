DOTNET=dotnet
CONFIG=Release
TF=net10.0

.PHONY: all restore build test run check-sdk check-packaging-deps publish-linux publish-win publish-mac publish-all package-linux package-win package-mac package-all clean

all: build

restore:
	$(DOTNET) restore

build: restore
	$(DOTNET) build -c $(CONFIG)

test:
	AVALONIA_HEADLESS=1 $(DOTNET) test -c $(CONFIG) --no-build

run:
	$(DOTNET) run --project src/Bonsai.UI

check-sdk:
	@echo "Checking for .NET 10 SDK..."
	@if ! $(DOTNET) --list-sdks | grep -q '^10\.' ; then \
		echo "ERROR: .NET 10 SDK not found. Install .NET 10 SDK to build (https://aka.ms/dotnet/download)"; exit 1; \
	fi

check-packaging-deps: check-sdk
	@echo "Checking packaging dependencies..."
	@if ! command -v pwsh >/dev/null 2>&1 ; then \
		echo "INFO: PowerShell 'pwsh' not found; Windows MSI packaging may not be available on this host."; \
	fi
	@if ! command -v appimagetool >/dev/null 2>&1 ; then \
		echo "INFO: appimagetool not found; AppImage packaging may attempt to download appimagetool or fall back to a tarball."; \
	fi
	@if ! command -v hdiutil >/dev/null 2>&1 ; then \
		echo "INFO: hdiutil not found; DMG creation will fallback to zip on non-mac hosts."; \
	fi

publish-linux: check-packaging-deps
	$(DOTNET) publish src/Bonsai.UI -c $(CONFIG) -f $(TF) -r linux-x64 --self-contained true \
		-p:PublishSingleFile=true -p:PublishTrimmed=true -p:PublishReadyToRun=true -p:DebugType=None -o ./publish/linux-x64

publish-win: check-sdk
	$(DOTNET) publish src/Bonsai.UI -c $(CONFIG) -f $(TF) -r win-x64 --self-contained true \
		-p:PublishSingleFile=true -p:PublishTrimmed=true -p:PublishReadyToRun=true -p:DebugType=None -o ./publish/win-x64

publish-mac: check-sdk
	$(DOTNET) publish src/Bonsai.UI -c $(CONFIG) -f $(TF) -r osx-x64 --self-contained true \
		-p:PublishSingleFile=true -p:PublishTrimmed=true -p:PublishReadyToRun=true -p:DebugType=None -o ./publish/osx-x64

publish-all: publish-linux publish-win publish-mac

package-linux: check-packaging-deps publish-linux
	mkdir -p artifacts
	@if [ ! -d ./publish/linux-x64 ]; then \
		echo "ERROR: publish/linux-x64 not found. Run: make publish-linux or make publish-all"; exit 1; \
	fi
	./scripts/pack-appimage.sh ./publish/linux-x64 ./artifacts

package-win: check-packaging-deps publish-win
	@if [ ! -d ./publish/win-x64 ]; then \
		echo "ERROR: publish/win-x64 not found. Run: make publish-win or make publish-all"; exit 1; \
	fi
	@if ! command -v pwsh >/dev/null 2>&1 ; then \
		echo "ERROR: PowerShell 'pwsh' not found. Install PowerShell to enable MSI packaging on this host."; exit 1; \
	fi
	pwsh -File scripts/build-msi.ps1 -BinariesDir ./publish/win-x64 -OutputDir ./artifacts

package-mac: check-packaging-deps publish-mac
	@if [ ! -d ./publish/osx-x64 ]; then \
		echo "ERROR: publish/osx-x64 not found. Run: make publish-mac or make publish-all"; exit 1; \
	fi
	chmod +x scripts/make-dmg.sh && scripts/make-dmg.sh ./publish/osx-x64 ./artifacts

package-all: package-linux package-win package-mac

clean:
	rm -rf **/bin **/obj publish artifacts
