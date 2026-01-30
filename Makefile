DOTNET=dotnet
CONFIG=Release
TF=net10.0

.PHONY: all restore build test run check-sdk publish-linux publish-win publish-mac publish-all package-linux package-win package-mac package-all clean

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

publish-linux: check-sdk
	$(DOTNET) publish src/Bonsai.UI -c $(CONFIG) -f $(TF) -r linux-x64 --self-contained true \
		-p:PublishSingleFile=true -p:PublishTrimmed=true -p:PublishReadyToRun=true -o ./publish/linux-x64

publish-win: check-sdk
	$(DOTNET) publish src/Bonsai.UI -c $(CONFIG) -f $(TF) -r win-x64 --self-contained true \
		-p:PublishSingleFile=true -p:PublishTrimmed=true -p:PublishReadyToRun=true -o ./publish/win-x64

publish-mac: check-sdk
	$(DOTNET) publish src/Bonsai.UI -c $(CONFIG) -f $(TF) -r osx-x64 --self-contained true \
		-p:PublishSingleFile=true -p:PublishTrimmed=true -p:PublishReadyToRun=true -o ./publish/osx-x64

publish-all: publish-linux publish-win publish-mac

package-linux: publish-linux
	chmod +x scripts/pack-appimage.sh && scripts/pack-appimage.sh ./publish/linux-x64 ./artifacts

package-win: publish-win
	pwsh -File scripts/build-msi.ps1 -BinariesDir ./publish/win-x64 -OutputDir ./artifacts

package-mac: publish-mac
	chmod +x scripts/make-dmg.sh && scripts/make-dmg.sh ./publish/osx-x64 ./artifacts

package-all: package-linux package-win package-mac

clean:
	rm -rf **/bin **/obj publish artifacts
