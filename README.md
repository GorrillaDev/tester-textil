# tester-textil
programa para app y firmware de tester textil
## Contexto para Codex

Este repo tiene una carpeta de continuidad para retomar el proyecto en sesiones nuevas de Codex:

- `codex-context/README_REANUDAR.md`
- `codex-context/RECUERDO_CODEX_WEB.txt`
- `codex-context/RESUMEN_CORTO_CODEX.txt`
- `codex-context/PROMPT_CODEX_WEB.txt`

Orden recomendado al retomar:

1. leer `codex-context/README_REANUDAR.md`
2. leer `codex-context/RECUERDO_CODEX_WEB.txt`
3. pegar o reutilizar `codex-context/PROMPT_CODEX_WEB.txt`

## Estructura base del proyecto

Quedo creada esta estructura para migrar el proyecto real a este repo:

- `firmware_esp32`
- `app_windows`
- `referencias`
- `drivers`

Guias:

- `MIGRAR_PROYECTO_AQUI.md`
- `COMO_USAR_EN_CODEX_WEB.md`

## Nota de firmware ESP-IDF

La carpeta `firmware_esp32/managed_components` no se versiona. Se regenera desde:

- `firmware_esp32/main/idf_component.yml`

Si hace falta reconstruir dependencias:

```powershell
idf.py reconfigure
idf.py build
```
