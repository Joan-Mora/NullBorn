# Brief — Mundo real NullBorn (qué necesito de ti)

El overworld actual es **procedural** (primitivas + texturas noise). Sirve para probar mecánicas, **no** es el arte final. Para un mundo creíble necesitamos assets reales y decisiones tuyas.

## Problemas actuales (ya conocidos)

| Problema | Causa |
|---|---|
| Suelos dobles / clipping | Terreno mesh + discos de bioma + props a Y=0 |
| 15–20 FPS | Muchas mallas + **8 Point Lights** de farolas + densidades altas |
| Caídas del mapa | Altura del mesh vs spawn |

## Camino recomendado (tesis, realista)

### Opción A — Kit low-poly (más rápido, recomendado)
Importas packs **gratis/CC0** y yo (o el agente) los cableo al `World_Main`.

Packs sugeridos (elige 1–2, no todos):
1. **Kenney Nature Kit** — https://kenney.nl/assets/nature-kit  
2. **KayKit** (Medieval / Dungeon / City) — https://kaylousberg.itch.io/  
3. **Quaternius** Ultimate Modular — https://quaternius.com/  
4. Skybox: **AllSky Free** o HDRI Poly Haven  

### Opción B — Unity Terrain + props
Tú esculpes el Terrain en el editor; yo dejo solo gameplay (spawns, regions, NPCs).

### Opción C — Blender (más trabajo)
Modeláis 1 hub + 3 biomas como escenas FBX. Mejor calidad, más tiempo.

---

## Checklist — qué necesito de ti

### 1. Decisión de estilo (obligatorio)
Responde con una línea:
- [ ] Low-poly colorido (Hades / Zelda Wind Waker vibe digital)
- [ ] Sci-fi limpio (Tron / Control lite)
- [ ] Ruinas corruptas oscuras (Signalis / Hollow)

### 2. Assets (obligatorio para “mundo real”)
Haz **una** de estas:
- **A)** Descarga e importa a `Assets/Art/Environment/ThirdParty/` los packs elegidos y avísame  
- **B)** Pásame enlaces Unity Asset Store (los que ya tengas en la cuenta)  
- **C)** Sube FBX propios a `Assets/Art/Environment/`

Mínimo útil:
- 5–10 árboles / plantas  
- 5–10 edificios o módulos  
- 1 suelo / hierba / camino (textura o tile)  
- 1 skybox  

### 3. Trabajo en Unity (tú, 30–60 min)
1. Cierra Play.  
2. Menú **NullBorn → Build World Main** (regenera limpio).  
3. Si aún ves restos: Hierarchy → borra todo excepto Main Camera/Light si sobran → vuelve a Build.  
4. Play y mira FPS (Stats en Game view).

### 4. Lo que yo hago cuando tengas assets
- Prefabs por bioma (`Prop_Heap_*`, `Prop_Plaza_*`, …)  
- Sustituir `EnvironmentKit` por `Instantiate(prefab)`  
- Un solo suelo (Terrain o mesh) sin capas duplicadas  
- LOD / sin Point Lights dinámicas (baked o emission)  
- Spawns/quests intactos  

### 5. Lo que NO hace falta aún
- MMO / streaming de chunks  
- Fotorealismo  
- API PostgreSQL para el arte  

---

## Mientras tanto (parche técnico)

El código se ajusta para:
- Un solo suelo (sin discos de bioma encima)
- Menos árboles/rocas
- Farolas **sin** Point Light (solo emisión)
- Regenerar mundo destruyendo `WorldRoot` previo

Cuando completes el checklist §1–2, dime **“assets listos”** y seguimos con el cableado real.
