# NullBorn — Performance & Quality Baseline

Meta de tesis (PC Windows, Unity 6 + URP): **≥55 FPS estables** en escenas jugables, con look stylized legible (no “Ultra cinematográfico”).

## Cómo medir (obligatorio antes de añadir features)

1. Menú **NullBorn → Performance → Apply Baseline (Quality + URP)** (o regenerar Art Test Scene).
2. **NullBorn → Build Art Test Scene**.
3. Play → overlay **F3** (FPS / ms).
4. Game view → **Stats**: anotar FPS, Batches, Tris, Shadow casters.
5. Window → Analysis → **Profiler** 30 s: ¿CPU o GPU bound?

| Señal | Lectura |
| --- | --- |
| FPS sube mucho al bajar resolución | GPU bound |
| FPS igual al bajar resolución / Stats CPU alto | CPU bound |
| Batches disparados | demasiados materiales / props no batched |
| Sombras pixeladas / costosas | shadow map / distance / casters |

## Baseline aplicada (valores)

| Ajuste | Valor | Por qué |
| --- | --- | --- |
| MSAA | 2× | Bordes limpios sin costo 4× |
| Soft shadows | Medium | Evita sombras “cuadradas” |
| Shadow distance | 40 m | Aldea cercana; no 150 m |
| Cascades | 2 | Calidad cerca del player |
| Shadow map | 2048 | Personaje nítido |
| Additional lights / object | 2 | Límite de fill lights |
| Additional light shadows | Off | Muy caras |
| SRP Batcher | On | Draw calls URP |
| Dynamic batching | On | Props chicos |
| Foliage cast shadows | Off | Overdraw + shadowmaps |
| VSync (editor) | Off | Medir FPS real |

## Checklist antes de avanzar de contenido

- [ ] ArtTest ≥55 FPS en máquina del equipo (anotar PC + resolución)
- [ ] Batches ArtTest &lt; ~150 en reposo
- [ ] Soft shadows visibles en player (no bloque pixelado extremo)
- [ ] Texturas héroe/aldea legibles a 2–5 m (no “barro” por filtro)
- [ ] Profiler: ningún spike &gt;33 ms al caminar/rodar
- [ ] No añadir packs/features nuevas hasta cerrar esta lista

## Próximas mejoras (cuando haya mundo real)

1. **LOD Group** en árboles / casas.
2. **Occlusion culling** en aldea.
3. Unificar materiales repetidos (menos variantes = más batching).
4. Animators de NPCs lejanos con culling / update rate bajo.
5. Pool de VFX; cero `Instantiate`/`Destroy` en combate por frame.
6. Build player: default **uncapped** (`targetFrameRate = -1`, VSync off) so F3 / el player miden FPS real. Cap opcional: `RuntimePerformanceTweaks.frameCap` = 60 o 120, o marcar `vsync`.

## Menús Unity

- `NullBorn → Performance → Apply Baseline (Quality + URP)`
- `NullBorn → Performance → Log Current Stats Hint`
- `NullBorn → Build Art Test Scene` (aplica baseline + overlay FPS)
