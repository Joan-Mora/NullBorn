# NullBorn

**NullBorn** es un **action-RPG ágil** en tercera persona (Unity 6 + URP) donde el jugador es un **Guardián de Runtime** que patrulla una Ciudad-Runtime en colapso. Combate legible primero; la informática es sabor y lore (isomorfismo lúdico), no el manual.

## Estado

**Open World RPG en desarrollo** — **fases 0–9 implementadas** (plan [`docs/WORLD_RPG_PLAN.md`](docs/WORLD_RPG_PLAN.md); polish/backend real pendientes). Escena principal: `World_Main`. Arena de combate: `Combat_Arena`. El vertical slice **Demo_10min** se conserva como *Simulación de arranque (legacy)*.

**Diseño:** [`docs/GDD_v2.md`](docs/GDD_v2.md) · **Plan:** [`docs/WORLD_RPG_PLAN.md`](docs/WORLD_RPG_PLAN.md) · **Arte entorno:** [`docs/ENVIRONMENT_ART_BRIEF.md`](docs/ENVIRONMENT_ART_BRIEF.md) · **Legacy:** [`docs/GDD_v1.md`](docs/GDD_v1.md)

## Requisitos

| Componente | Versión |
|---|---|
| Unity | **6000.5.9f1** (Unity 6) |
| Plataforma objetivo | Windows 10/11 x64 |
| Render pipeline | Universal Render Pipeline (URP) |

## Cómo jugar

1. Abre esta carpeta en **Unity Hub** con Unity **6000.5.9f1**.
2. **Mundo:** abre `Assets/Scenes/World_Main.unity` y pulsa **Play ►** (terreno, árboles, edificios y biomas se generan solos si la escena está vacía). Opcional: **NullBorn → Build World Main**.
3. **Arena de combate (Fase 0b):** `Assets/Scenes/Combat_Arena.unity` o **NullBorn → Build Combat Arena**.
4. **Demo legacy:** **NullBorn → Generar Demo 10min** → `Demo_10min.unity`.
5. Haz **click en la ventana Game** para capturar el mouse.

### Controles (mundo / arena)

| Tecla | Acción |
|---|---|
| WASD | Mover |
| Shift | Sprint (gasta Energía) |
| Espacio | Dash con i-frames |
| Mouse | Cámara |
| Click / F | Corte de Traza (combo L-L-H) |
| G | Onda de Purga (AoE, cooldown) |
| V | Restaurar (canalización) |
| Q / click rueda | Lock-on |
| E | Interactuar (NPC, tienda, terminal, portal) |
| Tab | Memory Cache (1–4 usar ítem) |
| Esc | Pausa |
| R | Respawn / reiniciar |
| F5 / F9 | Guardar / cargar (overworld) |

### Demo_10min (legacy)

Click/F = combo (si hay `TraceCutCombo`) · **E/G = Garbage Collection** combinado · 3 zonas lineales Boot → Heap → Kernel.

## Tech stack

- **Motor:** Unity 6 + URP + uGUI
- **Lenguaje:** C# (`NullBorn` namespace)
- **Arte:** Primitivas URP + materiales temáticos; Mixamo pendiente de importar
- **Persistencia:** JSON local + mock API (`INullBornApi`). PostgreSQL real pendiente (RF-17–20 parcial)
- **Documentación:** GDD v2, ARCHITECTURE, REQUIREMENTS, WORLD_RPG_PLAN, [ENVIRONMENT_ART_BRIEF](docs/ENVIRONMENT_ART_BRIEF.md), CHANGELOG

## Escenas

| Escena | Propósito |
|---|---|
| `World_Main` | Overworld (hub + Heap / Stack / I/O / Kernel) |
| `Combat_Arena` | Playtest de combate 60 s |
| `Dungeons/Dungeon_HeapCave` | Instancia Heap |
| `Dungeons/Dungeon_StackChasm` | Instancia Stack |
| `Demo_10min` | Mini-demo lineal legacy |
| `Prototype` | Arena mínima de pruebas |

## Build Windows

Menú **NullBorn → Build Windows (.exe)** → salida en `Builds/Windows/NullBorn.exe`

## Regenerar mundo (CLI)

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.5.9f1\Editor\Unity.exe" `
  -batchmode -nographics -quit `
  -projectPath "C:\Users\joanm\Documents\GitHub\NullBorn" `
  -executeMethod NullBorn.EditorTools.BuildWorldMainScene.BuildBatch `
  -logFile "Logs\batch-world.log"
```

## Equipo

<table>
  <tr>
    <td align="center">
      <img src="https://github.com/Joan-Mora.png" width="100px;" alt="Darwin Joan Aveiga Mora"/><br />
      <sub><b>Darwin Joan Aveiga Mora</b></sub><br />
      <a href="https://github.com/Joan-Mora">@Joan-Mora</a><br />
      <sub>Desarrollador Full Stack</sub>
    </td>
    <td align="center">
      <img src="https://github.com/MrRoar154.png" width="100px;" alt="Julian Esteban Chavez Zamora"/><br />
      <sub><b>Julian Esteban Chavez Zamora</b></sub><br />
      <a href="https://github.com/MrRoar154">@MrRoar154</a><br />
      <sub>Desarrollador Full Stack</sub>
    </td>
  </tr>
</table>

---
