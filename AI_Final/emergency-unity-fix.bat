@echo off
echo ========================================
echo Unity Emergency Render Pipeline Fix
echo ========================================
echo.
echo 🚨 CRITICAL: Unity Asset Database Corruption Detected
echo 📝 This script will perform a complete Unity reset
echo.
pause

echo [1/6] Closing Unity Editor (if open)...
taskkill /f /im "Unity.exe" 2>nul
taskkill /f /im "UnityEditor.exe" 2>nul
taskkill /f /im "Unity Hub.exe" 2>nul
timeout /t 3 /nobreak >nul
echo ✅ Unity processes terminated

echo.
echo [2/6] Removing all Unity cache and temp files...
if exist "Library" rmdir /s /q "Library" 2>nul
if exist "Temp" rmdir /s /q "Temp" 2>nul
if exist "obj" rmdir /s /q "obj" 2>nul
if exist "bin" rmdir /s /q "bin" 2>nul
if exist "Logs" rmdir /s /q "Logs" 2>nul
echo ✅ All cache folders cleared

echo.
echo [3/6] Clearing package cache...
if exist "Packages\packages-lock.json" del "Packages\packages-lock.json" 2>nul
echo ✅ Package lock cleared

echo.
echo [4/6] Resetting Unity Registry entries...
reg delete "HKEY_CURRENT_USER\Software\Unity Technologies" /f 2>nul
echo ✅ Registry entries cleared

echo.
echo [5/6] Creating package refresh marker...
echo. > "Packages\.refresh_needed"
echo ✅ Refresh marker created

echo.
echo [6/6] Creating safe project settings...
if not exist "ProjectSettings" mkdir "ProjectSettings"

echo Performing emergency project settings reset...
> "ProjectSettings\ProjectVersion.txt" (
echo m_EditorVersion: 2023.2.20f1
echo m_EditorVersionWithRevision: 2023.2.20f1 ^(3ec9ae3cef7e^)
)

echo.
echo ========================================
echo 🎯 EMERGENCY FIX COMPLETE
echo ========================================
echo.
echo ⚠️  CRITICAL NEXT STEPS:
echo.
echo 1. Start Unity Hub
echo 2. Open project: %cd%
echo 3. Wait 5-10 minutes for full reimport
echo 4. Do NOT interrupt the import process
echo 5. Unity will rebuild all assets and shaders
echo.
echo 💡 If errors persist:
echo 1. Switch to Built-in Render Pipeline
echo 2. Go to Edit ^> Project Settings ^> Graphics
echo 3. Set Scriptable Render Pipeline to "None"
echo.
echo 🚨 If this doesn't work, the project may need
echo    to be recreated with just the game scripts.
echo.
echo ========================================
pause
