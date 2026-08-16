#!/usr/bin/env bash
set -u

APP_NAME="coordinet-cs"
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
BUILD_DIR="$ROOT_DIR/Application-Build"
TARGET_BIN="$ROOT_DIR/Application-Build/$APP_NAME"

printf '\033[0;35m'
printf '========================================\n'
printf '  CoordiNet-CS Termux Installer\n'
printf '========================================\n'
printf '\033[0m\n'

ensure_tool() {
  if command -v csc >/dev/null 2>&1; then
    return 0
  fi

  if command -v dotnet >/dev/null 2>&1; then
    return 0
  fi

  printf '\033[0;33m[WARN] C# compiler not detected.\033[0m\n'
  read -r -p 'Install Mono or .NET SDK now? [Y/n]: ' answer
  answer=${answer:-Y}
  if [[ "$answer" =~ ^[Yy]$ ]]; then
    pkg update && pkg install -y git clang make mono
  else
    printf '\033[0;31m[ERROR] Installation aborted because the compiler toolchain is unavailable.\033[0m\n'
    exit 1
  fi
}

menu() {
  echo
  echo '1) INSTALL'
  echo '2) TEST'
  echo '3) UPDATE'
  echo
  read -r -p 'Choose an action [1-3]: ' choice
  case "$choice" in
    1)
      make -C "$ROOT_DIR" || make -C "$ROOT_DIR" -f Makefile
      if [ -f "$TARGET_BIN" ]; then
        read -r -p 'Custom install path? (leave blank for /data/data/com.termux/files/usr/bin): ' custom_path
        if [ -n "$custom_path" ]; then
          mkdir -p "$custom_path"
          install -m 755 "$TARGET_BIN" "$custom_path/$APP_NAME"
          echo "export PATH=\"\$PATH:$custom_path\"" >> "$HOME/.bashrc"
          printf '\033[0;32m[OK] Installed to custom path and exported to shell PATH.\033[0m\n'
        else
          install -m 755 "$TARGET_BIN" /data/data/com.termux/files/usr/bin/$APP_NAME
          printf '\033[0;32m[OK] Installed to /data/data/com.termux/files/usr/bin/%s\033[0m\n' "$APP_NAME"
        fi
      else
        printf '\033[0;31m[ERROR] Binary missing from %s\033[0m\n' "$BUILD_DIR"
        exit 2
      fi
      ;;
    2)
      make -C "$ROOT_DIR" || make -C "$ROOT_DIR" -f Makefile
      printf '\033[0;32m[OK] Localized test build preserved within %s\033[0m\n' "$BUILD_DIR"
      ;;
    3)
      if ! command -v "$APP_NAME" >/dev/null 2>&1; then
        printf '\033[0;31m[ERROR] %s is not registered in PATH. Update cannot continue.\033[0m\n' "$APP_NAME"
        exit 3
      fi
      make -C "$ROOT_DIR" || make -C "$ROOT_DIR" -f Makefile
      printf '\033[0;32m[OK] Repository rebuilt while preserving local config and logs.\033[0m\n'
      ;;
    *)
      printf '\033[0;31m[ERROR] Invalid menu selection.\033[0m\n'
      exit 1
      ;;
  esac
}

ensure_tool
menu
