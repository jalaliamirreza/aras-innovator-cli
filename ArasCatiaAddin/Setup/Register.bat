@echo off
echo ========================================
echo Aras CATIA Add-in Registration
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

:: Check if DLL exists
if not exist "%ADDIN_PATH%" (
    echo ERROR: Add-in DLL not found at:
    echo   %ADDIN_PATH%
    echo.
    echo Please build the solution first in Release mode.
    pause
    exit /b 1
)

echo Registering Add-in DLL...
echo.

:: Register with 64-bit RegAsm (for 64-bit CATIA)
if exist "%REGASM64%" (
    echo Using 64-bit RegAsm...
    "%REGASM64%" "%ADDIN_PATH%" /codebase /tlb
    if %errorLevel% equ 0 (
        echo [OK] 64-bit registration successful.
    ) else (
        echo [ERROR] 64-bit registration failed.
    )
) else (
    echo [WARNING] 64-bit RegAsm not found.
)

echo.

:: Also register with 32-bit RegAsm (for compatibility)
if exist "%REGASM32%" (
    echo Using 32-bit RegAsm...
    "%REGASM32%" "%ADDIN_PATH%" /codebase /tlb
    if %errorLevel% equ 0 (
        echo [OK] 32-bit registration successful.
    ) else (
        echo [ERROR] 32-bit registration failed.
    )
) else (
    echo [WARNING] 32-bit RegAsm not found.
)

echo.
echo ========================================
echo Registration complete.
echo.
echo NOTE: You may need to restart CATIA for
echo the add-in to appear.
echo ========================================
pause
