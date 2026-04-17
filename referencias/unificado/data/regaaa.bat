@echo off
title Configurador de Hotspot OrinProb
:: Verifica permisos de administrador
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo [ERROR] Por favor, dale clic derecho y "Ejecutar como administrador"
    pause
    exit /b
)

echo Configurando Nombre: orinprob...
echo Configurando Clave: orinprop4000...

:: 1. Metodo Netsh (Para compatibilidad)
netsh wlan set hostednetwork mode=allow ssid=orinprob key=orinprop4000 >nul

:: 2. Comando PowerShell en una sola linea para evitar el error del simbolo ^
powershell -ExecutionPolicy Bypass -Command "$tetheringManager = [Windows.Networking.NetworkOperators.NetworkOperatorTetheringManager, Windows.Networking.NetworkOperators, ContentType=WindowsRuntime]::CreateFromConnectionProfile([Windows.Networking.Connectivity.NetworkInformation, Windows.Networking.Connectivity, ContentType=WindowsRuntime]::GetInternetConnectionProfile()); $config = $tetheringManager.GetCurrentAccessPointConfiguration(); $config.Ssid = 'orinprob'; $config.Passphrase = 'orinprop4000'; $tetheringManager.ConfigureAccessPointAsync($config).GetResults(); $tetheringManager.StartTetheringAsync().GetResults();"

echo.
echo ===========================================
echo   LISTO! Los cambios fueron aplicados.
echo ===========================================
pause
