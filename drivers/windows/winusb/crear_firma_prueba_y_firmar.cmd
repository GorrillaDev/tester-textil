@echo off
setlocal
powershell -ExecutionPolicy Bypass -File "%~dp0crear_firma_prueba_y_firmar.ps1"
pause
