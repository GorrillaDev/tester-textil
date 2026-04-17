# Como Retomar En Codex Web

## Flujo recomendado

1. Abrir este repo en Codex web
2. Abrir `codex-context/README_REANUDAR.md`
3. Abrir `codex-context/RECUERDO_CODEX_WEB.txt`
4. Si quieres arrancar con mas precision, pegar el contenido de `codex-context/PROMPT_CODEX_WEB.txt` como primer mensaje

## Mensaje corto recomendado para iniciar

Usa algo asi:

```text
Lee primero codex-context/README_REANUDAR.md y codex-context/RECUERDO_CODEX_WEB.txt. Retoma el proyecto desde ese estado. No replantees la arquitectura. Empieza verificando el ultimo punto de depuracion USB en firmware_esp32/main/main.c.
```

## Cuando ya copies el proyecto real al repo

El orden de lectura recomendado para Codex web sera:

1. `codex-context/README_REANUDAR.md`
2. `codex-context/RECUERDO_CODEX_WEB.txt`
3. `firmware_esp32/main/main.c`
4. `app_windows/WinUsbControllerTransport.cs`
5. `app_windows/Form1.cs`

## Importante

Codex web no garantiza memoria permanente entre sesiones. La forma correcta de "recordar" es:

- guardar el contexto en archivos dentro del repo
- empezar cada sesion leyendo esos archivos
- dejar nuevos hallazgos tecnicos tambien en `codex-context`

