@echo off
setlocal
cd /d "%~dp0"
pnputil /add-driver "AcuratexControlBridge.inf" /install
pause
