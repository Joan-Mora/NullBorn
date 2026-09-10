#!/bin/sh
# Render (Linux): escribe la URL pública de la API local (túnel HTTPS).
# Variable: NULLBORN_API_BASE  ejemplo: https://algo.trycloudflare.com
set -e
ROOT="${1:-Server/NullBorn.Api/wwwroot}"
VAL="${NULLBORN_API_BASE:-}"
VAL=$(printf '%s' "$VAL" | tr -d '"' | sed 's:/*$::')
mkdir -p "$ROOT"
printf 'window.NULLBORN_API_BASE = "%s";\n' "$VAL" > "$ROOT/config.js"
echo "PWA config: NULLBORN_API_BASE=$VAL -> $ROOT/config.js"
