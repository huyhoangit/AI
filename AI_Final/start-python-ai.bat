@echo off
echo 🤖 PYTHON LOCAL AI SETUP
echo ========================

echo.
echo 1. Checking Python installation...
python --version >nul 2>&1
if errorlevel 1 (
    echo ❌ Python not found! Please install Python from: https://python.org
    echo After installation, run this script again.
    pause
    exit /b 1
) else (
    echo ✅ Python is installed
)

echo.
echo 2. Installing required packages...
pip install -r requirements.txt
if errorlevel 1 (
    echo ❌ Failed to install packages
    pause
    exit /b 1
) else (
    echo ✅ Packages installed successfully
)

echo.
echo 3. Starting Python AI server...
echo 📡 Server will run on: http://localhost:5000
echo 🛑 Press Ctrl+C to stop server
echo 🎯 Ready for Unity integration!
echo.

python simple_quoridor_ai.py

echo.
echo 🛑 Server stopped
pause
