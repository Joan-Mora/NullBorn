# Scripts

Código del juego por dominio (ver [`ARCHITECTURE.md`](../../ARCHITECTURE.md)).

| Carpeta | Responsabilidad |
|---------|-----------------|
| `Player/` | Locomoción, cámara, pausa, settings, overlay FPS |
| `Map/` | Minimapa, brújula, HUD mapa, markers 3D |
| `Net/` | Cliente companion (REST / SignalR), DTOs, config API |
| `Combat/` | Melee, arma, dummy |
| `Art/` | Highlight de interactuables |

Al añadir sistemas grandes (UI general, utilidades), crear carpetas hermanas (`UI/`, `Core/`) en lugar de mezclar dominios.
