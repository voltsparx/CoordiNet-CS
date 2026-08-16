OUTPUT_DIR := Application-Build
BINARY_NAME := coordinet-cs

ifeq ($(OS),Windows_NT)
TARGET_BINARY := $(OUTPUT_DIR)/$(BINARY_NAME).exe
else
TARGET_BINARY := $(OUTPUT_DIR)/$(BINARY_NAME)
endif

.PHONY: all build clean

all: build

build:
	@echo "Building CoordiNet-CS..."
	dotnet build src/CoordiNet-CS.csproj --configuration Release --output $(OUTPUT_DIR) -nologo
	@echo "✓ Built $(TARGET_BINARY)"

clean:
ifeq ($(OS),Windows_NT)
	-cmd /c rmdir /s /q "Application-Build" 2>nul
	-cmd /c rmdir /s /q "src\bin" 2>nul
	-cmd /c rmdir /s /q "src\obj" 2>nul
else
	-rm -rf "$(OUTPUT_DIR)" src/bin src/obj
endif
	@echo "✓ Clean complete"
