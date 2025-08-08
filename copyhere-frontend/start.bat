@echo off
echo Starting CopyHere Frontend...
echo.
echo Options:
echo 1. Start with Python HTTP server (recommended)
echo 2. Start with Node.js HTTP server
echo 3. Open directly in browser
echo.
set /p choice="Enter your choice (1-3): "

if "%choice%"=="1" (
    echo Starting Python HTTP server...
    python -m http.server 8080
) else if "%choice%"=="2" (
    echo Starting Node.js HTTP server...
    npx http-server -p 8080 -c-1
) else if "%choice%"=="3" (
    echo Opening in browser...
    start index.html
) else (
    echo Invalid choice. Starting with Python HTTP server...
    python -m http.server 8080
)

pause 