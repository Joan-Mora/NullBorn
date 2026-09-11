# NullBorn — Arquitectura del repositorio

Un solo repositorio: **cliente Unity en la raíz** + **companion en `Server/`**.

```
NullBorn/
├── Assets/                 # Contenido del juego (Unity)
│   ├── Scripts/
│   │   ├── Player/         # Motor, cámara, pausa, settings, FPS
│   │   ├── Map/            # Minimapa, brújula, HUD, marcadores
│   │   ├── Net/            # Cliente API / SignalR / DTOs
│   │   ├── Combat/         # Melee, arma, dummy
│   │   └── Art/            # Highlight de interactuables
│   ├── Art/                # Arte propio / animaciones del proyecto
│   ├── Scenes/             # Escenas jugables (p. ej. ArtTest)
│   ├── Settings/           # URP / calidad
│   ├── Editor/             # Menús NullBorn (Build Art Test, baseline, etc.)
│   └── _ThirdParty/        # Packs importados aprobados (ASSET_STACK)
├── Packages/               # manifest UPM
├── ProjectSettings/        # Proyecto Unity
├── Server/                 # ASP.NET + PostgreSQL + PWA
├── docs/                   # Dirección de arte Cap 0 + marca
├── tools/                  # Scripts auxiliares (Python, etc.)
├── _Inbox/                 # Local: zips/packs crudos (gitignored)
├── ARCHITECTURE.md
├── ASSET_STACK.md
├── PERFORMANCE.md
├── DEPLOY.md
├── CHANGELOG.md
└── README.md
```

## Cómo abrir el proyecto

1. Unity Hub → **Add** → carpeta raíz del repo (`NullBorn/`).
2. Debe verse `Assets/`, `Packages/`, `ProjectSettings/` en esa carpeta.
3. Companion: ver [`Server/README.md`](Server/README.md).

## Capas en runtime

```
Unity (PC)  ←REST/SignalR→  ASP.NET API  ←→  PostgreSQL
                                  ↓
                            PWA (móvil)
```

| Capa | Carpeta | Responsabilidad |
|------|---------|-----------------|
| Presentación 3D / input | `Assets/Scripts/Player` | Locomoción, cámara OTS, UI pausa |
| Navegación / HUD mapa | `Assets/Scripts/Map` | Minimapa, brújula, markers en mundo |
| Red cliente | `Assets/Scripts/Net` | Sync pose + markers (opcional / offline-first) |
| API + persistencia | `Server/NullBorn.Api` | REST, SignalR, EF Core, seed zona |
| Mapa táctico | `Server/.../wwwroot` | PWA |

## Convención de scripts (crecimiento)

Nuevas features van en carpetas por dominio bajo `Assets/Scripts/`:

| Carpeta | Uso |
|---------|-----|
| `Player/` | Control del avatar |
| `Map/` | Sistemas de mapa en juego |
| `Net/` | Cliente de red |
| `Combat/` | Melee, arma, dummy de ArtTest |
| `UI/` | (futuro) menús / HUD no-mapa |
| `Core/` | (futuro) utilidades compartidas |

No mezclar lógica de red dentro de `Player/` ni arte de terceros fuera de `_ThirdParty/`.

## Qué ya no forma parte del repo

- **`Game/`** — absorbido en la raíz.
- **`Prototipo/`** — eliminado en el unificado de septiembre 2026.
- Packs sueltos en la antigua raíz `Assets/` — movidos a `_Inbox/SourcePacks/` (local).
