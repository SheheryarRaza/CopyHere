Write-Host "Starting CopyHere Frontend..." -ForegroundColor Green
Write-Host ""
Write-Host "Options:" -ForegroundColor Yellow
Write-Host "1. Start with Python HTTP server (recommended)" -ForegroundColor White
Write-Host "2. Start with Node.js HTTP server" -ForegroundColor White
Write-Host "3. Open directly in browser" -ForegroundColor White
Write-Host "4. Start mock server for testing" -ForegroundColor White
Write-Host ""

$choice = Read-Host "Enter your choice (1-4)"

switch ($choice) {
    "1" {
        Write-Host "Starting Python HTTP server..." -ForegroundColor Cyan
        python -m http.server 8080
    }
    "2" {
        Write-Host "Starting Node.js HTTP server..." -ForegroundColor Cyan
        npx http-server -p 8080 -c-1
    }
    "3" {
        Write-Host "Opening in browser..." -ForegroundColor Cyan
        Start-Process "index.html"
    }
    "4" {
        Write-Host "Starting mock server..." -ForegroundColor Cyan
        Write-Host "This will start a mock backend server for testing." -ForegroundColor Yellow
        node mock-server.js
    }
    default {
        Write-Host "Invalid choice. Starting with Python HTTP server..." -ForegroundColor Yellow
        python -m http.server 8080
    }
}

Read-Host "Press Enter to exit" 