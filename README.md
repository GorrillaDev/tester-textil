# tester-textil

Repositorio del sistema Acuratex para Windows y ESP32-S3.

## Estructura

- `src/windows/AcuratexControlApp`: app `C# WinForms` para control por `USB WinUSB` o `TCP`.
- `firmware/esp32`: firmware `ESP-IDF` para `ESP32-S3`.
- `drivers/windows/winusb`: paquete del driver `WinUSB` para el dispositivo `USB\VID_CAFE&PID_4030`.

## Flujo en Windows

1. Instalar el driver desde `drivers/windows/winusb`.
2. Abrir o publicar la app desde `src/windows/AcuratexControlApp`.
3. Compilar o flashear el firmware desde `firmware/esp32`.

## Relacion entre componentes

- El firmware expone el `USB vendor-specific` y el servidor `TCP`.
- La app usa el mismo protocolo de lineas para `USB` y `WiFi`.
- El driver asocia el `VID/PID` del equipo con `WinUSB`.
