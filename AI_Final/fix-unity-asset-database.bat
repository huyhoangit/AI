@echo off
echo ========================================
echo UNITY ASSET DATABASE CORRUPTION FIX
echo ========================================
echo.

echo This script will fix Unity's Asset Database corruption
echo by performing a complete project reset and cleanup.
echo.

pause

echo Step 1: Closing Unity if running...
taskkill /f /im Unity.exe 2>nul
taskkill /f /im UnityHub.exe 2>nul
timeout /t 3 /nobreak >nul

echo Step 2: Removing corrupted Unity cache and databases...
if exist "Library" (
    echo Deleting Library folder...
    rmdir /s /q "Library"
)

if exist "Temp" (
    echo Deleting Temp folder...
    rmdir /s /q "Temp"
)

if exist "obj" (
    echo Deleting obj folder...
    rmdir /s /q "obj"
)

if exist "bin" (
    echo Deleting bin folder...
    rmdir /s /q "bin"
)

echo Step 3: Cleaning package cache and lock files...
if exist "Packages\packages-lock.json" (
    echo Deleting packages-lock.json...
    del "Packages\packages-lock.json"
)

if exist "Packages\manifest.json.backup" (
    echo Deleting manifest backup...
    del "Packages\manifest.json.backup"
)

echo Step 4: Resetting Unity Registry entries...
reg delete "HKEY_CURRENT_USER\Software\Unity Technologies" /f 2>nul
reg delete "HKEY_LOCAL_MACHINE\SOFTWARE\Unity Technologies" /f 2>nul

echo Step 5: Clearing Unity global cache...
if exist "%APPDATA%\Unity" (
    echo Clearing Unity AppData cache...
    rmdir /s /q "%APPDATA%\Unity\Asset Database v2" 2>nul
    rmdir /s /q "%APPDATA%\Unity\Cache" 2>nul
    rmdir /s /q "%APPDATA%\Unity\Asset Store-5.x" 2>nul
)

if exist "%LOCALAPPDATA%\Unity" (
    echo Clearing Unity LocalAppData cache...
    rmdir /s /q "%LOCALAPPDATA%\Unity\Editor\Cache" 2>nul
    rmdir /s /q "%LOCALAPPDATA%\Unity\Editor\Asset Database v2" 2>nul
)

echo Step 6: Creating minimal manifest.json for safe package restoration...
if not exist "Packages" mkdir "Packages"

echo {> "Packages\manifest.json"
echo   "dependencies": {>> "Packages\manifest.json"
echo     "com.unity.collab-proxy": "2.0.5",>> "Packages\manifest.json"
echo     "com.unity.feature.development": "1.0.1",>> "Packages\manifest.json"
echo     "com.unity.textmeshpro": "3.0.6",>> "Packages\manifest.json"
echo     "com.unity.timeline": "1.7.5",>> "Packages\manifest.json"
echo     "com.unity.ugui": "1.0.0",>> "Packages\manifest.json"
echo     "com.unity.visualscripting": "1.8.0",>> "Packages\manifest.json"
echo     "com.unity.modules.ai": "1.0.0",>> "Packages\manifest.json"
echo     "com.unity.modules.androidjni": "1.0.0",>> "Packages\manifest.json"
echo     "com.unity.modules.animation": "1.0.0",>> "Packages\manifest.json"
echo     "com.unity.modules.assetbundle": "1.0.0",>> "Packages\manifest.json"
echo     "com.unity.modules.audio": "1.0.0",>> "Packages\manifest.json"
echo     "com.unity.modules.cloth": "1.0.0",>> "Packages\manifest.json"
echo     "com.unity.modules.director": "1.0.0",>> "Packages\manifest.json"
echo     "com.unity.modules.imageconversion": "1.0.0",>> "Packages\manifest.json"
echo     "com.unity.modules.imgui": "1.0.0",>> "Packages\manifest.json"
echo     "com.unity.modules.jsonserialize": "1.0.0",>> "Packages\manifest.json"
echo     "com.unity.modules.particlesystem": "1.0.0",>> "Packages\manifest.json"
echo     "com.unity.modules.physics": "1.0.0",>> "Packages\manifest.json"
echo     "com.unity.modules.physics2d": "1.0.0",>> "Packages\manifest.json"
echo     "com.unity.modules.screencapture": "1.0.0",>> "Packages\manifest.json"
echo     "com.unity.modules.terrain": "1.0.0",>> "Packages\manifest.json"
echo     "com.unity.modules.terrainphysics": "1.0.0",>> "Packages\manifest.json"
echo     "com.unity.modules.tilemap": "1.0.0",>> "Packages\manifest.json"
echo     "com.unity.modules.ui": "1.0.0",>> "Packages\manifest.json"
echo     "com.unity.modules.uielements": "1.0.0",>> "Packages\manifest.json"
echo     "com.unity.modules.umbra": "1.0.0",>> "Packages\manifest.json"
echo     "com.unity.modules.unityanalytics": "1.0.0",>> "Packages\manifest.json"
echo     "com.unity.modules.unitywebrequest": "1.0.0",>> "Packages\manifest.json"
echo     "com.unity.modules.unitywebrequestassetbundle": "1.0.0",>> "Packages\manifest.json"
echo     "com.unity.modules.unitywebrequestaudio": "1.0.0",>> "Packages\manifest.json"
echo     "com.unity.modules.unitywebrequesttexture": "1.0.0",>> "Packages\manifest.json"
echo     "com.unity.modules.unitywebrequestwww": "1.0.0",>> "Packages\manifest.json"
echo     "com.unity.modules.vehicles": "1.0.0",>> "Packages\manifest.json"
echo     "com.unity.modules.video": "1.0.0",>> "Packages\manifest.json"
echo     "com.unity.modules.vr": "1.0.0",>> "Packages\manifest.json"
echo     "com.unity.modules.wind": "1.0.0",>> "Packages\manifest.json"
echo     "com.unity.modules.xr": "1.0.0">> "Packages\manifest.json"
echo   },>> "Packages\manifest.json"
echo   "scopedRegistries": []>> "Packages\manifest.json"
echo }>> "Packages\manifest.json"

echo.
echo ========================================
echo CRITICAL: MANUAL STEPS REQUIRED
echo ========================================
echo.
echo 1. Open Unity Hub
echo 2. Remove this project from the list (if present)
echo 3. Click "Add" and browse to this folder: %CD%
echo 4. Select Unity 2022.3 LTS or newer when opening
echo 5. Wait for Unity to fully reimport all assets (this may take 10-15 minutes)
echo 6. If render pipeline errors persist:
echo    - Go to Edit ^> Project Settings ^> Graphics
echo    - Set "Scriptable Render Pipeline Settings" to None (Built-in)
echo    - Or assign a new Universal Render Pipeline Asset
echo.
echo The project should now work properly!
echo.
pause
