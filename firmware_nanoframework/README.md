# Acuratex Firmware (.NET nanoFramework)

Migración inicial del firmware ESP32-S3 a **C# con .NET nanoFramework** siguiendo principios **POO + SOLID**.

## Objetivo de esta etapa

- Tener una base mantenible para crecer en comandos de cabezales y motores.
- Separar responsabilidades en capas (Domain/Core/Infrastructure/Composition).
- Mantener protocolo textual de comandos (`ping`, `status`, `can1`, `can2`, `send`, `start`, `stop`, `testeo`).

## Estado

- ✅ Arquitectura base y routing de comandos en C#.
- ✅ Servicios de dominio abstraídos por interfaces.
- ✅ Canales de transporte desacoplados del dominio.
- ⚠️ Pendiente: integración hardware completa de USB vendor-specific WinUSB en nanoFramework.

## Nota técnica importante

En nanoFramework para ESP32-S3 hay soporte de targets y USB CDC integrado en imágenes de referencia, pero no se garantiza de serie el mismo flujo **USB vendor-specific + WinUSB custom** que hoy tienes en ESP-IDF/TinyUSB.

Por eso esta fase deja el firmware en C# con contratos listos para:

1. UART de servicio,
2. WiFi TCP,
3. y una implementación USB específica cuando se confirme/implemente en el target.

## Estructura

- `Domain/`: contratos e interfaces (abstracciones).
- `Core/`: parser y casos de uso (command router).
- `Infrastructure/`: canales concretos y servicios simulados/placeholder.
- `Composition/`: wiring manual de dependencias.
- `Program.cs`: punto de entrada.

