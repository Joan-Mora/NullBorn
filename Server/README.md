# NullBorn Companion API — mapa móvil + markers en vivo

## Requisitos

- Docker Desktop
- .NET SDK 8+ (probado con 10)
- Unity (raíz del repo NullBorn) en la misma máquina (Play mode)

## Arranque rápido

```powershell
cd Server
docker compose up -d
cd NullBorn.Api
dotnet run
```

API + PWA: [http://127.0.0.1:5080](http://127.0.0.1:5080)

Deploy (PWA en Render, API+Postgres locales): [`../DEPLOY.md`](../DEPLOY.md).

Desde el teléfono (misma Wi‑Fi):

1. En el PC, obtén tu IP LAN (`ipconfig` → IPv4).
2. Abre `http://<IP-LAN>:5080` en el navegador del móvil.
3. Si no carga, permite el puerto **5080** en el Firewall de Windows (regla de entrada TCP).

## Endpoints

| Método | Ruta | Uso |
|--------|------|-----|
| GET | `/api/health` | Healthcheck |
| GET | `/api/zones/{id}` | Bounds + imagen mapa (`arttest`) |
| GET | `/api/markers?zoneId=` | Lista markers |
| POST | `/api/markers` | Crear pin |
| PATCH | `/api/markers/{id}` | Mover / activar |
| DELETE | `/api/markers/{id}` | Borrar |
| GET/PUT | `/api/player/{id}` | Pose del jugador |
| Hub | `/hubs/game` | SignalR (`PlayerPoseUpdated`, `MarkersChanged`) |

## Unity

1. Arranca API + PostgreSQL.
2. En Unity: **NullBorn → Build Art Test Scene**.
3. Play: el minimapa (esquina) y los beams 3D se actualizan con los pins del móvil.
4. Config URL por defecto: `http://127.0.0.1:5080` en el objeto `MapSync`.

## Ver tablas de PostgreSQL (manual)

No hace falta leer solo logs. Opciones:

### A) `psql` en Docker (rápido)

```powershell
cd Server
docker compose exec db psql -U nullborn -d nullborn
```

Dentro de psql:

```sql
\dt
SELECT * FROM map_zones;
SELECT "Id", "Label", "X", "Z", "IsActive", "CreatedAt" FROM map_markers ORDER BY "CreatedAt";
SELECT * FROM player_state;
\q
```

> EF Core creó las columnas en **PascalCase**. En PostgreSQL hay que citarlas: `"CreatedAt"`, no `created_at`.

Para listar columnas reales:

```sql
\d map_markers
```

### B) pgAdmin / DBeaver / Azure Data Studio

1. Host: `localhost`
2. Port: `5432`
3. Database: `nullborn`
4. User / password: `nullborn` / `nullborn`

Así ves y editas filas con UI.

### C) Endpoint HTTP (rápido en navegador)

- http://127.0.0.1:5080/api/markers?zoneId=arttest
- http://127.0.0.1:5080/api/player/local-player
- http://127.0.0.1:5080/api/zones/arttest

