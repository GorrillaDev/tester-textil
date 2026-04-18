#!/usr/bin/env bash
set -euo pipefail

if [[ $# -lt 1 ]]; then
  echo "Uso: $0 <SERIAL_PORT>"
  echo "Ejemplo: $0 COM7"
  exit 1
fi

PORT="$1"
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_PATH="$ROOT_DIR/src/Acuratex.Firmware/Acuratex.Firmware.nfproj"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "ERROR: dotnet SDK no encontrado."
  exit 1
fi

dotnet tool restore --tool-manifest "$ROOT_DIR/.config/dotnet-tools.json"

# Build garantizado antes de deploy
"$ROOT_DIR/build_firmware_cs.sh"

# Deploy con nanoff (tool local)
dotnet tool run nanoff --target ESP32_S3 --serialport "$PORT" --update

echo "OK: flash/deploy solicitado por nanoff en $PORT"
