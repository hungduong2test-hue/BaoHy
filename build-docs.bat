@echo off
setlocal

cd /d "%~dp0"

echo Dang build va xuat website ra thu muc docs...
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0publish-github-pages.ps1"

if errorlevel 1 (
    echo.
    echo Build docs bi loi. Vui long xem thong bao phia tren.
    pause
    exit /b 1
)

echo.
echo Da build xong. Website tinh nam trong thu muc docs.
pause
