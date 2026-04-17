# Migrar Proyecto A Este Repo

Este repo ya tiene una estructura base para recibir el proyecto real:

- `firmware_esp32`
- `app_windows`
- `referencias`
- `drivers`
- `codex-context`

## Que copiar en cada carpeta

### `firmware_esp32`

Copiar aqui el contenido principal de:

- `C:\Users\Users\Desktop\acuratex final\control de motores`

Pero evitando mezclar binarios o basura temporal si no los quieres versionar. Lo minimo importante es:

- `main/`
- `.vscode/` si te sirve
- `CMakeLists.txt`
- `sdkconfig`
- `sdkconfig.defaults`
- `README.md`
- `dependencies.lock`
- `eim_config.toml`
- `managed_components/` si quieres reproducibilidad completa

Normalmente NO conviene versionar:

- `build/`

### `app_windows`

Copiar aqui el contenido de:

- `C:\Users\Users\Desktop\acuratex final\control de motores\AcuratexControlApp`

### `referencias`

Copiar aqui el contenido de:

- `C:\Users\Users\Desktop\acuratex final\proyectos de referencia`

### `drivers`

Copiar aqui el contenido de:

- `C:\Users\Users\Desktop\acuratex final\control de motores\driver_winusb`

## Contexto ya guardado

La continuidad para Codex ya esta en:

- `codex-context/README_REANUDAR.md`
- `codex-context/RECUERDO_CODEX_WEB.txt`
- `codex-context/RESUMEN_CORTO_CODEX.txt`
- `codex-context/PROMPT_CODEX_WEB.txt`

## Orden recomendado de migracion

1. copiar firmware a `firmware_esp32`
2. copiar app a `app_windows`
3. copiar referencias a `referencias`
4. copiar drivers a `drivers`
5. revisar `.gitignore`
6. hacer commit

## Nota

La arquitectura del proyecto ya esta decidida. Al migrar, no replantear:

- `ESP-IDF` para firmware
- `C# WinForms` para app
- `USB nativo vendor-specific + WinUSB`
- `UART externo` para servicio
- `WiFi TCP` como alternativa

