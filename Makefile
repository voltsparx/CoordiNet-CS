OUTPUT_DIR := Application-Build
BINARY_NAME := coordinet-cs

ifeq ($(OS),Windows_NT)
TARGET_BINARY := $(OUTPUT_DIR)/$(BINARY_NAME).exe
DOTNET_ROOT := $(shell powershell -NoProfile -Command "(Get-Command dotnet).Source | Split-Path -Parent")
DOTNET_SDK := $(shell powershell -NoProfile -Command "dotnet --list-sdks | Select-Object -Last 1 | ForEach-Object { $_.ToString().Split(' ')[0] }")
CSC := $(DOTNET_ROOT)/../sdk/$(DOTNET_SDK)/Roslyn/bincore/csc.dll
CS_FILES := $(shell powershell -NoProfile -Command "Get-ChildItem -Path 'src' -Recurse -Filter '*.cs' -File | Where-Object { $$_.FullName -notmatch '\\obj\\' -and $$_.FullName -notmatch '\\bin\\' } | ForEach-Object { $$_.FullName }")
else
TARGET_BINARY := $(OUTPUT_DIR)/$(BINARY_NAME)
CSC := csc
CS_FILES := $(shell find src -type f -name '*.cs' ! -path '*/obj/*' ! -path '*/bin/*' | sort)
endif

.PHONY: all build prep assets clean

all: build

build: prep assets $(TARGET_BINARY)

prep:
ifeq ($(OS),Windows_NT)
	@if not exist "$(OUTPUT_DIR)" mkdir "$(OUTPUT_DIR)"
else
	@mkdir -p "$(OUTPUT_DIR)"
endif

assets:
ifeq ($(OS),Windows_NT)
	@if not exist "$(OUTPUT_DIR)\assets" mkdir "$(OUTPUT_DIR)\assets"
	@if not exist "$(OUTPUT_DIR)\config" mkdir "$(OUTPUT_DIR)\config"
	@if not exist "$(OUTPUT_DIR)\docs" mkdir "$(OUTPUT_DIR)\docs"
	@if not exist "$(OUTPUT_DIR)\templates" mkdir "$(OUTPUT_DIR)\templates"
	@if exist "assets" xcopy "assets" "$(OUTPUT_DIR)\assets" /E /I /Y /Q >nul
	@if exist "config" xcopy "config" "$(OUTPUT_DIR)\config" /E /I /Y /Q >nul
	@if exist "docs" xcopy "docs" "$(OUTPUT_DIR)\docs" /E /I /Y /Q >nul
	@if exist "templates" xcopy "templates" "$(OUTPUT_DIR)\templates" /E /I /Y /Q >nul
	@if exist "assets\injected" xcopy "assets\injected" "$(OUTPUT_DIR)\assets\injected" /E /I /Y /Q >nul
else
	@if [ -d assets ]; then mkdir -p "$(OUTPUT_DIR)/assets"; cp -R assets/. "$(OUTPUT_DIR)/assets/"; fi
	@if [ -d config ]; then mkdir -p "$(OUTPUT_DIR)/config"; cp -R config/. "$(OUTPUT_DIR)/config/"; fi
	@if [ -d docs ]; then mkdir -p "$(OUTPUT_DIR)/docs"; cp -R docs/. "$(OUTPUT_DIR)/docs/"; fi
	@if [ -d templates ]; then mkdir -p "$(OUTPUT_DIR)/templates"; cp -R templates/. "$(OUTPUT_DIR)/templates/"; fi
	@if [ -d assets/injected ]; then mkdir -p "$(OUTPUT_DIR)/assets/injected"; cp -R assets/injected/. "$(OUTPUT_DIR)/assets/injected/"; fi
endif

$(TARGET_BINARY): $(CS_FILES)
	dotnet exec "$(CSC)" -nologo -target:exe -out:$(TARGET_BINARY) $(CS_FILES)
	@echo "Built $(TARGET_BINARY)"

clean:
ifeq ($(OS),Windows_NT)
	@if exist "$(OUTPUT_DIR)" rmdir /s /q "$(OUTPUT_DIR)"
else
	@rm -rf "$(OUTPUT_DIR)"
endif
