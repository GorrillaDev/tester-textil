# Reanudar Proyecto Acuratex

Este archivo existe para retomar rapido el contexto tecnico del proyecto en cualquier sesion nueva.

## Objetivo

Sistema con `ESP32-S3` para controlar cabezal y motores por `CAN`, usando una app de Windows por `USB nativo` o por `WiFi`, sin depender del navegador.

## Arquitectura ya definida

- Firmware en `ESP-IDF v6.0`
- App de PC en `C# WinForms` sobre `.NET 8`
- `USB nativo` del `ESP32-S3` como `vendor-specific + WinUSB`
- `UART externo` (`CH340` o `FTDI`) solo para flasheo y debug
- `WiFi STA` como alternativa de comunicacion
- `CAN` a `1 Mbps`
- Dos buses `CAN` con un solo `TWAI`, seleccionando `CAN1` o `CAN2` por software

## Rutas importantes

- Firmware: `control de motores`
- Archivo principal firmware: `control de motores/main/main.c`
- App: `control de motores/AcuratexControlApp`
- Driver WinUSB: `control de motores/driver_winusb`
- Referencia Arduino: `proyectos de referencia/unificado/unificado.ino`

## Estado del firmware actual

- El firmware compila correctamente.
- Binario actual:
  - `control de motores/build/control_de_motores.bin`
- Comunicaciones disponibles:
  - `USB nativo vendor-specific`
  - `WiFi TCP`
  - `UART de servicio`
- Comandos de prueba actuales:
  - `ping`
  - `status`
  - `can1`
  - `can2`
  - `send 320 07`
  - `start`
  - `stop`
  - `testeo`

## Estado de la app actual

- La app ya detecta el dispositivo por `WinUSB`.
- Tiene modo `USB` y `WiFi`.
- Permite enviar comandos manuales y ver log RX/TX.
- Ejecutable publicado:
  - `control de motores/AcuratexControlApp/bin/Release/net8.0-windows/win-x64/publish/AcuratexControlApp.exe`

## Problema tecnico mas reciente

Sintoma observado:

- El primer comando por `USB` funciona.
- El `ESP32-S3` responde.
- El segundo comando falla.
- Si se desconecta y reconecta el `USB`, vuelve a funcionar una sola vez.

## Diagnostico mas probable

El problema apuntaba principalmente al firmware USB, no a la UI. El patron encaja con un endpoint `OUT` o buffer RX de TinyUSB que no quedaba listo para el siguiente paquete del host.

## Ultima correccion aplicada

Archivo:

- `control de motores/main/main.c`

Cambio:

- dentro de `tud_vendor_rx_cb(...)` se agrego:

```c
#if CFG_TUD_VENDOR_RX_BUFSIZE > 0
    tud_vendor_n_read_flush(APP_USB_VENDOR_ITF);
#endif
```

Motivo:

- liberar el buffer RX interno de TinyUSB para aceptar el siguiente paquete `OUT`

Estado:

- el firmware recompilo correctamente despues de ese cambio

## Que probar primero al retomar

1. Flashear el firmware actual al `ESP32-S3`
2. Probar por `USB nativo` esta secuencia:
   - `status`
   - `can1`
   - `send 320 07`
   - otro comando adicional
3. Verificar si el segundo comando sigue rompiendo el enlace

## Si el error sigue

Los siguientes puntos a revisar son:

1. `app_reply_usb_vendor(...)` en `control de motores/main/main.c`
2. eventos `mount/detach/suspend/resume` del USB
3. comportamiento de `WinUsbControllerTransport.cs`
4. posible reenumeracion del dispositivo entre el primer y segundo intercambio

## Comandos utiles

Compilar firmware:

```powershell
& "C:\Users\Users\AppData\Local\Microsoft\WinGet\Packages\Espressif.EIM-CLI_Microsoft.Winget.Source_8wekyb3d8bbwe\eim.exe" run "idf.py build" v6.0
```

Flashear firmware por UART:

```powershell
& "C:\Users\Users\AppData\Local\Microsoft\WinGet\Packages\Espressif.EIM-CLI_Microsoft.Winget.Source_8wekyb3d8bbwe\eim.exe" run "idf.py -p COM7 flash" v6.0
```

Publicar la app:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## Archivo de recuerdo completo

Si hace falta mas detalle, revisar:

- `RECUERDO_CODEX_WEB.txt`

