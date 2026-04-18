#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_PATH="$ROOT_DIR/src/Acuratex.Firmware/Acuratex.Firmware.nfproj"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "ERROR: dotnet SDK no encontrado. Instala .NET SDK 8+ y vuelve a ejecutar este script."
  exit 1
fi

echo "[1/3] Restaurando herramientas locales (.config/dotnet-tools.json)..."
dotnet tool restore --tool-manifest "$ROOT_DIR/.config/dotnet-tools.json"

echo "[2/3] Restaurando paquetes del proyecto nanoFramework..."
dotnet restore "$PROJECT_PATH"

echo "[3/3] Compilando firmware C# nanoFramework..."
dotnet build "$PROJECT_PATH" -c Release -v minimal

echo "OK: build firmware C# completado"
