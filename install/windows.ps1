$ErrorActionPreference = 'Stop'
$appName = 'coordinet-cs'
$rootDir = Split-Path -Parent $PSScriptRoot
$buildDir = Join-Path $rootDir 'Application-Build'
$targetBin = Join-Path $buildDir $appName
if (-not $targetBin.EndsWith('.exe')) { $targetBin = "$targetBin.exe" }

Write-Host "========================================" -ForegroundColor Magenta
Write-Host "  CoordiNet-CS Windows Installer" -ForegroundColor Magenta
Write-Host "========================================" -ForegroundColor Magenta

function Ensure-Toolchain {
    if (Get-Command csc -ErrorAction SilentlyContinue) { return }
    if (Get-Command dotnet -ErrorAction SilentlyContinue) { return }

    Write-Warning 'C# compiler not detected on this system.'
    $answer = Read-Host 'Install the .NET SDK or Mono toolchain now? [Y/n]'
    if ($answer -match '^[Nn]$') {
        throw 'Installation aborted because the compiler toolchain is unavailable.'
    }

    if (Get-Command winget -ErrorAction SilentlyContinue) {
        winget install --id Microsoft.DotNet.SDK.8 --accept-source-agreements --accept-package-agreements
    }
    elseif (Get-Command choco -ErrorAction SilentlyContinue) {
        choco install dotnet-sdk -y
    }
    else {
        throw 'No supported Windows package manager was detected.'
    }
}

function Invoke-Build {
    if (Get-Command mingw32-make -ErrorAction SilentlyContinue) {
        mingw32-make -C $rootDir
    }
    elseif (Get-Command make -ErrorAction SilentlyContinue) {
        make -C $rootDir
    }
    else {
        throw 'make is not installed. Please install a Make toolchain before continuing.'
    }
}

function Show-Menu {
    Write-Host ''
    Write-Host '1) INSTALL'
    Write-Host '2) TEST'
    Write-Host '3) UPDATE'
    Write-Host ''
    $choice = Read-Host 'Choose an action [1-3]'

    switch ($choice) {
        '1' {
            Invoke-Build
            if (Test-Path $targetBin) {
                $customPath = Read-Host 'Custom install path? (leave blank for user local AppData)'
                if ([string]::IsNullOrWhiteSpace($customPath)) {
                    $customPath = Join-Path $env:USERPROFILE 'AppData\Local\Microsoft\WindowsApps'
                }

                New-Item -ItemType Directory -Path $customPath -Force | Out-Null
                Copy-Item $targetBin (Join-Path $customPath $appName) -Force

                $currentPath = [Environment]::GetEnvironmentVariable('Path', 'User')
                if ($currentPath -notmatch [regex]::Escape($customPath)) {
                    $newPath = if ([string]::IsNullOrWhiteSpace($currentPath)) { $customPath } else { "$currentPath;$customPath" }
                    [Environment]::SetEnvironmentVariable('Path', $newPath, 'User')
                }

                Write-Host "[OK] Installed to $customPath" -ForegroundColor Green
            }
            else {
                throw "Binary missing from $buildDir"
            }
        }
        '2' {
            Invoke-Build
            Write-Host "[OK] Localized test build retained in $buildDir" -ForegroundColor Green
        }
        '3' {
            if (-not (Get-Command $appName -ErrorAction SilentlyContinue)) {
                throw "$appName is not registered in PATH. Update cannot continue."
            }
            Invoke-Build
            Write-Host '[OK] Repository rebuilt while preserving local config and logs.' -ForegroundColor Green
        }
        default {
            throw 'Invalid menu selection.'
        }
    }
}

Ensure-Toolchain
Show-Menu
