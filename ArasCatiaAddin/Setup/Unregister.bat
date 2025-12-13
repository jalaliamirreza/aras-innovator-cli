@echo off
echo ========================================
echo Aras CATIA Add-in Unregistration
echo ========================================
echo.

:: Check for admin privileges
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: This script requires Administrator privileges.
    echo Please run as Administrator.
    pause
    exit /b 1
)

:: Set paths
set "ADDIN_PATH=%~dp0..\ArasCatiaAddin\bin\Release\ArasCatiaAddin.dll"
set "REGASM32=C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe"
set "REGASM64=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe"

echo Unregistering Add-in DLL...
echo.

:: Unregister with 64-bit RegAsm
if exist "%REGASM64%" (
    echo Using 64-bit RegAsm...
    "%REGASM64%" "%ADDIN_PATH%" /unregister
    if %errorLevel% equ 0 (
        echo [OK] 64-bit unregistration successful.
    ) else (
        echo [WARNING] 64-bit unregistration failed (may already be unregistered).
    )
)

echo.

:: Unregister with 32-bit RegAsm
if exist "%REGASM32%" (
    echo Using 32-bit RegAsm...
    "%REGASM32%" "%ADDIN_PATH%" /unregister
    if %errorLevel% equ 0 (
        echo [OK] 32-bit unregistration successful.
    ) else (
        echo [WARNING] 32-bit unregistration failed (may already be unregistered).
    )
)

echo.
echo ========================================
echo Unregistration complete.
echo ========================================
pause
