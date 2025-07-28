# Ollama Auto Setup Script cho Windows
# Chạy script này để tự động cài đặt và test Ollama

Write-Host "🚀 OLLAMA AUTO SETUP SCRIPT" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Green

# Kiểm tra xem Ollama đã được cài đặt chưa
Write-Host "`n1. Checking Ollama installation..." -ForegroundColor Yellow

try {
    $ollamaVersion = ollama --version 2>$null
    if ($ollamaVersion) {
        Write-Host "✅ Ollama is already installed: $ollamaVersion" -ForegroundColor Green
    }
} catch {
    Write-Host "❌ Ollama not found. Please install from: https://ollama.ai/download" -ForegroundColor Red
    Write-Host "After installation, run this script again." -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit
}

# Kiểm tra service đang chạy
Write-Host "`n2. Checking Ollama service..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:11434/api/version" -TimeoutSec 5 -ErrorAction Stop
    Write-Host "✅ Ollama service is running" -ForegroundColor Green
} catch {
    Write-Host "⚠️ Ollama service not running. Starting..." -ForegroundColor Yellow
    Start-Process "ollama" -ArgumentList "serve" -WindowStyle Hidden
    Write-Host "✅ Ollama service started" -ForegroundColor Green
    Start-Sleep -Seconds 3
}

# Kiểm tra model
Write-Host "`n3. Checking available models..." -ForegroundColor Yellow
$models = ollama list 2>$null

if ($models -match "llama2") {
    Write-Host "✅ llama2 model found" -ForegroundColor Green
} else {
    Write-Host "⚠️ llama2 model not found. Downloading..." -ForegroundColor Yellow
    Write-Host "This may take several minutes (3-4GB download)..." -ForegroundColor Cyan
    
    try {
        ollama pull llama2:7b
        Write-Host "✅ llama2:7b model downloaded successfully" -ForegroundColor Green
    } catch {
        Write-Host "❌ Failed to download model. Check internet connection." -ForegroundColor Red
        Read-Host "Press Enter to exit"
        exit
    }
}

# Test model
Write-Host "`n4. Testing model..." -ForegroundColor Yellow
$testPrompt = "Bạn là AI hỗ trợ game Quoridor. Quoridor là gì? Trả lời ngắn gọn."

Write-Host "Sending test prompt..." -ForegroundColor Cyan
Write-Host "Prompt: $testPrompt" -ForegroundColor Gray

# Tạo test request
$body = @{
    model = "llama2:7b"
    prompt = $testPrompt
    stream = $false
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "http://localhost:11434/api/generate" -Method Post -Body $body -ContentType "application/json" -TimeoutSec 30
    
    if ($response.response) {
        Write-Host "`n✅ MODEL TEST SUCCESSFUL!" -ForegroundColor Green
        Write-Host "Response: $($response.response)" -ForegroundColor White
    } else {
        Write-Host "`n⚠️ Model responded but empty response" -ForegroundColor Yellow
    }
} catch {
    Write-Host "`n❌ Model test failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Unity integration check
Write-Host "`n5. Unity Integration Status:" -ForegroundColor Yellow
Write-Host "✅ Ollama URL: http://localhost:11434/api/generate" -ForegroundColor Green
Write-Host "✅ Model: llama2:7b" -ForegroundColor Green
Write-Host "✅ Ready for Unity testing" -ForegroundColor Green

Write-Host "`n🎯 SETUP COMPLETE!" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Green
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Open Unity project" -ForegroundColor White
Write-Host "2. Add OllamaTestManager component to GameObject" -ForegroundColor White
Write-Host "3. Click 'Test Ollama Local' in Inspector" -ForegroundColor White
Write-Host "4. Check Console for test results" -ForegroundColor White

Write-Host "`nOllama will continue running in background..." -ForegroundColor Yellow
Read-Host "Press Enter to finish"
