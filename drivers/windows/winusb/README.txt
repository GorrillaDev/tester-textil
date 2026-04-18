Acuratex Control Bridge - Driver WinUSB

Archivos:
- AcuratexControlBridge.inf
- AcuratexControlBridge.cdf
- crear_firma_prueba_y_firmar.ps1
- crear_firma_prueba_y_firmar.cmd
- instalar_driver_firmado.cmd

Hardware esperado:
- USB\VID_CAFE&PID_4030

GUID de interfaz:
- {D7761D50-5F1B-4D33-95F2-733B0E5F2EED}

Uso para pruebas en esta PC:
1. Conecta el USB nativo del ESP32-S3.
2. Ejecuta crear_firma_prueba_y_firmar.cmd como administrador.
3. Cuando termine, ejecuta instalar_driver_firmado.cmd como administrador.
4. Desconecta y reconecta el USB nativo.

Notas:
- Esto es solo para pruebas.
- El certificado es de prueba y solo servira en PCs donde se instale/confie ese certificado.
- Para cliente final, lo correcto es firma real.
- Este paquete ya referencia CatalogFile y fuerza la asociacion del VID/PID a WinUSB.
