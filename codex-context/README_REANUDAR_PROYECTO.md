# Reanudar Proyecto Desde Esta Carpeta

Este archivo existe para que el contexto del proyecto quede tambien dentro de `control de motores`.

## Punto Actual

- Firmware: `ESP-IDF v6.0`
- App Windows: `C# WinForms .NET 8`
- Transporte principal PC <-> ESP32-S3: `USB nativo vendor-specific + WinUSB`
- Transporte alterno: `WiFi TCP`
- Servicio y flasheo: `UART externo CH340/FTDI`
- CAN: `1 Mbps`
- Dos buses CAN con un solo `TWAI`

## Ultimo problema investigado

Sintoma:

- El primer comando USB funciona.
- El ESP responde.
- El segundo comando falla hasta desconectar y reconectar USB.

Diagnostico mas probable:

- Problema del lado del firmware USB, especialmente en el manejo del RX de TinyUSB vendor.

Ultima correccion ya aplicada:

- Archivo: `main/main.c`
- En `tud_vendor_rx_cb(...)` se agrego:

```c
#if CFG_TUD_VENDOR_RX_BUFSIZE > 0
    tud_vendor_n_read_flush(APP_USB_VENDOR_ITF);
#endif
```

## Archivos que leer primero al retomar

1. `main/main.c`
2. `AcuratexControlApp/WinUsbControllerTransport.cs`
3. `AcuratexControlApp/Form1.cs`
4. `driver_winusb/`

## Recuerdo extendido

Si hace falta contexto completo, revisar en la raiz del proyecto:

- `..\RECUERDO_CODEX_WEB.txt`
- `..\PROMPT_CODEX_WEB.txt`

