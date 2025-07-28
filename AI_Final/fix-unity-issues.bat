@echo off
echo ========================================
echo Unity Project Cleanup and Fix Script
echo ========================================
echo.

echo [1/4] Clearing Unity cache folders...
if exist "Library" (
    rmdir /s /q "Library" 2>nul
    echo ✅ Cleared Library folder
) else (
    echo ℹ️ Library folder not found
)

if exist "Temp" (
    rmdir /s /q "Temp" 2>nul
    echo ✅ Cleared Temp folder  
) else (
    echo ℹ️ Temp folder not found
)

echo.
echo [2/4] Clearing Visual Studio cache...
if exist "obj" (
    rmdir /s /q "obj" 2>nul
    echo ✅ Cleared obj folder
)

if exist "bin" (
    rmdir /s /q "bin" 2>nul
    echo ✅ Cleared bin folder
)

echo.
echo [3/4] Refreshing package cache...
if exist "Packages\packages-lock.json" (
    del "Packages\packages-lock.json" 2>nul
    echo ✅ Cleared package lock file
)

echo.
echo [4/4] Cleanup complete!
echo.
echo 🎯 Next Steps:
echo 1. Open Unity Editor
echo 2. Wait for package resolution to complete
echo 3. If UI errors persist, go to Window ^> Package Manager
echo 4. Find "UI Toolkit" and "UI Elements" packages and ensure they're installed
echo.
echo ✅ Your project is now clean and ready!
echo ========================================
pause
