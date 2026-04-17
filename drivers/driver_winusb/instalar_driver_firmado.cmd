@echo off
setlocal
cd /d "%~dp0"

for /f "usebackq delims=" %%I in (`powershell -NoProfile -ExecutionPolicy Bypass -Command "(Get-PnpDevice -PresentOnly | Where-Object { $_.InstanceId -like 'USB\\VID_CAFE&PID_4030*' } | Select-Object -ExpandProperty InstanceId)"`) do (
  echo Reiniciando dispositivo %%I
  pnputil /remove-device "%%I"
)

pnputil /add-driver "AcuratexControlBridge.inf" /install
pnputil /scan-devices
pause
