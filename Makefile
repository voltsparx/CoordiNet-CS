OUTPUT_DIR := Application-Build
BINARY_NAME := coordinet-cs

ifeq ($(OS),Windows_NT)
TARGET_BINARY := $(OUTPUT_DIR)/$(BINARY_NAME).exe
else
TARGET_BINARY := $(OUTPUT_DIR)/$(BINARY_NAME)
endif

.PHONY: all build prep assets clean

all: build

build: prep assets
	@echo "Building CoordiNet-CS..."
	dotnet build src/CoordiNet-CS.csproj --configuration Release --output $(OUTPUT_DIR) -nologo
	@echo "✓ Built $(TARGET_BINARY)"

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
	@if exist "assets" xcopy "assets" "$(OUTPUT_DIR)\assets" /E /I /Y /Q >nul
	@if exist "config" xcopy "config" "$(OUTPUT_DIR)\config" /E /I /Y /Q >nul
	@if exist "docs" xcopy "docs" "$(OUTPUT_DIR)\docs" /E /I /Y /Q >nul
else
	@if [ -d assets ]; then mkdir -p "$(OUTPUT_DIR)/assets"; cp -R assets/. "$(OUTPUT_DIR)/assets/"; fi
	@if [ -d config ]; then mkdir -p "$(OUTPUT_DIR)/config"; cp -R config/. "$(OUTPUT_DIR)/config/"; fi
	@if [ -d docs ]; then mkdir -p "$(OUTPUT_DIR)/docs"; cp -R docs/. "$(OUTPUT_DIR)/docs/"; fi
endif

clean:
ifeq ($(OS),Windows_NT)
	-cmd /c rmdir /s /q "Application-Build" 2>nul
	-cmd /c rmdir /s /q "src\bin" 2>nul
	-cmd /c rmdir /s /q "src\obj" 2>nul
else
	-rm -rf "$(OUTPUT_DIR)" src/bin src/obj
endif
	@echo "✓ Clean complete"
