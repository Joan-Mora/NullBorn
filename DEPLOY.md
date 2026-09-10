# NullBorn — Deploy (PWA en Render, resto local)

Cumple la rúbrica: **se publica el cliente web**; **PostgreSQL se queda en tu PC**.

```
Teléfono  →  PWA (Render)
                 │  HTTPS  variable NULLBORN_API_BASE
                 ▼
              Túnel  →  API (dotnet en tu PC)  →  Postgres (Docker local)

Unity (PC)  ───────────────────────►  API  http://127.0.0.1:5080  →  mismo Docker
```

La PWA **no** habla con Docker. Solo llama a la API. La API es lo único que usa `localhost:5432`.

---

## Variable en Render (una sola)

| Nombre | Qué pones | Ejemplo |
|--------|-----------|---------|
| `NULLBORN_API_BASE` | URL **HTTPS** del túnel que apunta a tu API local, **sin** barra al final | `https://random-words.trycloudflare.com` |

No pongas:

- `http://127.0.0.1:5080` — Render y el teléfono no entran a tu localhost
- `http://192.168.x.x:5080` — Render es HTTPS y el navegador **bloquea** mixed content
- la URL de Postgres (`localhost:5432`) — eso no es web

No hace falta `ConnectionStrings` en Render: ahí **no** corre la API ni la base.

---

## Orden para dejarlo conectado

### 1. Local: Docker + API (siempre encendidos en la demo)

```powershell
cd C:\Users\joanm\Documents\GitHub\NullBorn\Server
docker compose up -d

cd NullBorn.Api
dotnet run
```

Comprueba: [http://127.0.0.1:5080/api/health](http://127.0.0.1:5080/api/health) → `{"status":"ok",...}`

### 2. Local: túnel HTTPS hacia el puerto 5080

El comando es **`cloudflared`** (con **d**): *cloud-flare-d*. No `clouflared`.  
El puerto de NullBorn es **5080**, no 5000.

Si acabas de instalarlo, **cierra y abre** PowerShell.

```powershell
cloudflared tunnel --url http://127.0.0.1:5080
```

Copia la URL que imprime, tipo `https://xxxxx.trycloudflare.com`.

Esa URL **es el valor de `NULLBORN_API_BASE`**. Cada vez que reinicias cloudflared **cambia**; actualiza la variable en Render y Redeploy (o espera el auto-deploy).

Deja esa ventana abierta mientras dura la demo.

### 3. Unity (sigue local)

Objeto **MapSync** en la escena:

| Campo | Valor |
|-------|--------|
| Base Url | `http://127.0.0.1:5080` |
| Auto Connect | **on** (si quieres pins/pose en vivo) |

Unity **no** usa Render ni el túnel.

### 4. Render: Static Site (solo la PWA)

1. Sube estos cambios a GitHub (`main`).
2. [render.com](https://render.com) → **New** → **Static Site** → repo `Joan-Mora/NullBorn`.
3. Ajustes:

   | Campo | Valor |
   |-------|--------|
   | Branch | `main` |
   | Build command | `sh tools/write-pwa-config.sh` |
   | Publish directory | `Server/NullBorn.Api/wwwroot` |

4. **Environment** → Add:

   | Key | Value |
   |-----|--------|
   | `NULLBORN_API_BASE` | `https://xxxxx.trycloudflare.com` (la del paso 2, sin `/` final) |

5. Deploy. La PWA queda en `https://nullborn-pwa.onrender.com` (el nombre que te asigne Render).

6. Teléfono: abre **esa URL de Render** (cualquier red). En la página, la línea “API” debe mostrar el túnel.

Si cambias el túnel: edita `NULLBORN_API_BASE` → **Manual Deploy**.

Plan free se duerme: entra a la PWA un minuto antes de la defensa.

---

## Cómo saber que está conectado

| Prueba | OK |
|--------|-----|
| PC: `http://127.0.0.1:5080/api/health` | JSON `ok` |
| Navegador: `https://TU-TUNEL/api/health` | el mismo JSON |
| Unity Play, Auto Connect | pose llega a la PWA |
| Teléfono → URL Render → tap en el mapa | pin en Unity |

Si la PWA dice “Sin conexión”: API o túnel apagados, o `NULLBORN_API_BASE` mal (http, barra final, túnel viejo).

---

## Build Unity (opcional)

**NullBorn → Build Windows Player** → `Builds/Windows/NullBorn.exe`  
MapSync igual: `http://127.0.0.1:5080`.

---

## LAN sin Render (respaldo el día de la defensa)

Si internet falla: teléfono en la misma Wi‑Fi → `http://IP-DEL-PC:5080`  
(la API también sirve la PWA). Unity igual en localhost. Docker igual.
