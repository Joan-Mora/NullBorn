$ErrorActionPreference = 'Stop'
$exe = @(
  "${env:ProgramFiles(x86)}\cloudflared\cloudflared.exe",
  "$env:ProgramFiles\cloudflared\cloudflared.exe"
) | Where-Object { Test-Path $_ } | Select-Object -First 1

if (-not $exe) {
  Write-Host "cloudflared no esta instalado. Ejecuta:"
  Write-Host '  winget install --id Cloudflare.cloudflared -e'
  exit 1
}

Write-Host "Usando $exe"
Write-Host "Deja esta ventana abierta. Copia la URL https://....trycloudflare.com"
& $exe tunnel --url http://127.0.0.1:5080
