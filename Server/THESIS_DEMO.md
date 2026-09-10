# Guion de demo (tesis) — Mapa móvil NullBorn

## Setup (2 minutos)

1. `cd Server` → `docker compose up -d`
2. `cd NullBorn.Api` → `dotnet run`
3. Unity → **NullBorn → Build Art Test Scene** → Play
4. Teléfono en la misma Wi‑Fi → `http://<IP-PC>:5080`

## Demo en vivo

1. **Caminar en PC** → el triángulo verde se mueve en el mapa del móvil.
2. **Tap en el mapa del móvil** → aparece un pin naranja.
3. En **menos de 1 s** el pin se ve en el minimapa del PC y como cilindro en el mundo.
4. **Botón “Ir”** (o tap en el pin) → se marca activo; la brújula del PC apunta al destino.
5. **Borrar** en el móvil → desaparece en el PC.
6. **Reiniciar Play en Unity** → los pins siguen (PostgreSQL).

## Qué demostrar conceptualmente

- Persistencia relacional (PostgreSQL)
- API REST + SignalR
- Cliente Unity + cliente PWA
- Sync casi en tiempo real en LAN (sin cloud)
