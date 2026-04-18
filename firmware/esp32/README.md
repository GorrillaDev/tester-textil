# Control dual CAN con ESP32-S3 y ESP-IDF

Proyecto base para `ESP32-S3` usando `ESP-IDF v6.0`, con:

- `UART/FTDI` solo para `flash`, rescate y depuracion
- `USB nativo` del `ESP32-S3` como `vendor-specific` para la app de Windows
- `WiFi STA` hacia hotspot o red existente
- servidor `TCP` para la app de Windows
- `TWAI/CAN` con seleccion de `CAN1` y `CAN2`

## Que hace

- Configura `TWAI/CAN` a `1 Mbps`
- Permite seleccionar en tiempo de ejecucion `CAN1` o `CAN2`
- Expone el mismo protocolo simple por lineas en `USB vendor-specific`, `UART` y `TCP`:
  - `ping`
  - `status`
  - `can1`
  - `can2`
  - `send 320 07`
  - o directamente `320 07`
- Levanta `WiFi` como cliente hacia:
  - `SSID: ACURATEX_NET`
  - `PASS: acuratex1`
- Usa:
  - `IP fija: 192.168.137.2`
  - `TCP port: 3333`

## USB para la app

- `USB nativo`: `vendor-specific + WinUSB`
- `VID`: `0xCAFE`
- `PID`: `0x4030`
- `GUID de interfaz`: `{D7761D50-5F1B-4D33-95F2-733B0E5F2EED}`
- No aparece como `COM` para la app

El `UART/FTDI` queda aparte y se usa solo para servicio.

## Idea de arquitectura

Esto sirve para usar:

- `1 solo TWAI` del `ESP32-S3`
- `2 transceivers CAN`
- `1 bus activo a la vez`

No transmite por ambos al mismo tiempo. Cambia de un bus al otro durante la ejecucion.

## Conexion con la app

- `USB`: la app se conecta al `USB nativo` por `WinUSB`
- `WiFi`: la app se conecta por `TCP` a `192.168.137.2:3333`

Esto asume el escenario de hotspot fijo de Windows.

## Pines de ejemplo

Estan definidos en `main/main.cpp`:

- `CAN1 TX -> GPIO 8`
- `CAN1 RX -> GPIO 9`
- `CAN2 TX -> GPIO 10`
- `CAN2 RX -> GPIO 11`

Cambialos por los pines reales de tu placa.

## Funcion clave

La funcion importante es:

- `app_can_select_bus(APP_CAN_BUS_1)`
- `app_can_select_bus(APP_CAN_BUS_2)`

Y para enviar:

- `app_can_send_standard(...)`

Con eso, durante tu algoritmo puedes decidir que transceiver usar antes de enviar la trama.

## Compilar

ESP-IDF ya fue instalado en esta maquina en:

- `C:\Espressif\v6.0\esp-idf`

La forma mas confiable de usarlo aqui es con `eim run`.

### Compilar con el entorno instalado

```bash
eim run "idf.py build" v6.0
```

### Flashear por FTDI/UART

```bash
eim run "idf.py -p COMx flash monitor" v6.0
```

Reemplaza `COMx` por el puerto real del `FTDI/UART`.

## Archivos principales

- `main/main.cpp`
- `src/windows/AcuratexControlApp/`

## Estado actual

- El firmware ya compila y genera `build/control_de_motores.bin`
- La app `C#` ya compila y publica su `.exe` portable
- Falta probar el enlace real en hardware por `USB nativo WinUSB`
