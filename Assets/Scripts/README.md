# Scripts

Código del juego por dominio (ver [`ARCHITECTURE.md`](../../ARCHITECTURE.md)).

| Carpeta | Responsabilidad |
|---------|-----------------|
| `Player/` | Locomoción, cámara, pausa, settings, overlay FPS |
| `Map/` | Minimapa, brújula, HUD mapa, markers 3D |
| `Net/` | Cliente companion (REST / SignalR), DTOs, config API |

Al añadir sistemas grandes (combate, UI general, utilidades), crear carpetas hermanas (`Combat/`, `UI/`, `Core/`) en lugar de mezclar dominios.
