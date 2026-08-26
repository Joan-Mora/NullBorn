# Plan de pivot — NullBorn Open World RPG

**Versión:** 1.0  
**Fecha:** 2026-08-26  
**Estado:** Planificación (sin implementación de código)  
**Proyecto:** NullBorn — Action-RPG educativo (Unity 6 + URP)  
**Contexto académico:** UNIMINUTO Villavicencio — Técnicas de Modelamiento de Software

---

## Resumen ejecutivo

NullBorn dispone hoy de un **vertical slice lineal** (`Demo_10min`: Boot → Heap → Kernel) con un loop de combate sólido basado en **isomorfismo lúdico** (Integridad, CPU, Memory Leaks, GC). El pivot abandona el demo de 10 minutos como visión principal y apunta a un **RPG de mundo abierto con sensación de vida**, manteniendo el ADN pedagógico.

Este documento define visión, diseño de mundo, mecánicas, arquitectura técnica, fases de implementación y actualizaciones de documentación. **No sustituye** [`GDD_v1.md`](GDD_v1.md) hasta que se publique `GDD_v2`; sirve como hoja de ruta del pivot.

### Inventario del codebase actual (baseline)

| Área | Existe | Detalle |
|---|---|---|
| Movimiento 3ª persona | ✅ | `ThirdPersonController`, `ThirdPersonCamera`, `CharacterController` |
| Recursos core | ✅ | `SystemIntegrity` (HP), `CpuCycles` (stamina) |
| Combate base | ✅ | `DebugStrikeAbility`, `GarbageCollectionAbility` |
| Enemigos | ✅ | `MemoryLeakEnemy` (Basic/Spawner/Boss + ENRAGE), `MemoryFragment` |
| Sesión / estados | ✅ | `GameSession` (Playing/Victory/Defeat), pausa Esc |
| Checkpoints | ✅ | `CheckpointSystem`, `Checkpoint` — respawn local con R |
| HUD | ✅ | `CanvasHUD` (runtime uGUI): Integridad, CPU, leak activo, tutorial |
| Tutorial contextual | ✅ | `TutorialDirector`, `TutorialVolume`, `ZoneTrigger` |
| Flujo demo lineal | ✅ | `DemoFlowController`, `DemoZoneId`, generador de escena |
| Bootstrap | ✅ | `DemoBootstrap`, `PrototypeBootstrap` |
| Arte procedural | ✅ | `ThreadAvatarBuilder`, `LeakAvatarBuilder`, materiales URP |
| Prefabs | ✅ | Player_Thread, Leak_Basic/Spawner/Boss |
| Escenas | ✅ | `Demo_10min.unity`, `Prototype.unity` |
| ScriptableObjects de juego | ❌ | Solo assets URP; sin datos de quests/NPCs/items |
| Mundo abierto / terreno | ❌ | Greybox con primitivas y suelos planos |
| NPCs / diálogo | ❌ | — |
| Quests / misiones | ❌ | — |
| Inventario / economía | ❌ | GDD v1 lo excluye explícitamente |
| Progresión / XP | ❌ | — |
| Save/load persistente | ❌ | Solo reload de escena |
| Fast travel | ❌ | — |
| Barra RAM dedicada | 🟡 | RF-07/RF-15 pendientes; fragmentos implican presión |
| API / PostgreSQL | ❌ | Roadmap académico en REQUIREMENTS |

**Scripts:** 34 archivos C# en `Assets/Scripts/` organizados en Bootstrap, Core, Player, Enemies, Demo, UI, Art.  
**Convención:** namespace `NullBorn.*`, textos al jugador en español.

---

## A. Visión

### A.1 De demo lineal a RPG de mundo abierto

**Antes (ejemplo descartado como visión principal):** 3 zonas lineales, ~10 minutos, victoria al matar Leak Boss.

**Ahora:** Un **mapa explorable continuo** donde el jugador (Thread) patrulla un ecosistema de procesos corruptos. El aprendizaje sigue siendo por **isomorfismo**, no por quizzes: cada mecánica nueva se introduce en contexto (POI, NPC o encuentro), no en un tutorial lineal obligatorio.

El mundo no es fantasy medieval: es el **interior estilizado de un sistema operativo bajo estrés** — circuitos, torres de stack, celdas RAM, nodos de red, estructuras que evocan infraestructura digital con lectura visual clara (siluetas, emisión URP, niebla por región).

### A.2 Definición de "mundo vivo"

| Sistema | Definición en NullBorn | Prioridad MVP | Prioridad visión completa |
|---|---|---|---|
| **NPCs** | Otros *threads* y *servicios* (Daemon, Logger, Scheduler) con rutinas simples: idle, caminar entre waypoints, reaccionar al jugador | Alta — hub + 3–5 NPCs clave | 15+ NPCs con horarios |
| **Día / noche** | Metáfora: **ciclos de carga del sistema** (Idle → Peak → Maintenance). Cambia iluminación, densidad de spawns y diálogos, no un reloj real 24 h | Media — 2 fases (normal/pico) | 4 fases + eventos |
| **Wildlife** | "Procesos menores": fragmentos errantes, micro-bugs (Null Sprites, Packet Moths) que no son combate principal sino ambiente + recursos | Media — 2 tipos pasivos | Ecosistema por bioma |
| **Sistemas ambientales** | Presión de RAM regional, zonas corruptas que expanden niebla/emisión, terminales que parpadean, audio ambiente por bioma | Alta | Clima de corrupción dinámico |
| **Quests** | *Tickets de mantenimiento* / *bug reports* entregados por NPCs; recompensan XP, ítems de caché o desbloqueos | Alta — 5–8 misiones | Cadena principal + secundarias |
| **Economía** | **Créditos de CPU** (moneda) + **slots de caché** (capacidad de inventario). Tiendas en el hub = "Runtime Market" | Media — 1 vendedor | Crafting + mercado dinámico |

**Principio:** "Vivo" no significa simulación AAA; significa que el jugador encuentra **actividad reconocible** al explorar (NPCs, eventos ambientales, encuentros opcionales) sin depender de un guion lineal.

### A.3 Enfoque visual: stylized-real

| Aspecto | Directriz |
|---|---|
| **Estilo** | Low-poly / stylized con PBR URP ligero; emisión cian (sano) vs magenta (corrupto) — hereda Art Bible de GDD v1 |
| **Escala** | Personaje ~1,8 m; edificios hub 8–25 m; torres de datos hasta 40 m como landmarks |
| **Assets** | Mix: kits modulares (KayKit, Quaternius, Synty) + props hero propios en Blender (≤15 piezas identitarias) |
| **Vegetación** | Metafórica: "árboles" = árboles de parseo / clusters de nodos; "bosque" = región Heap con densidad de fragmentos |
| **Cielo** | Skybox procedural o HDRI suave; en regiones corruptas, cielo con glitch/scanlines (shader simple) |
| **Pipeline** | [`BLENDER_PIPELINE.md`](BLENDER_PIPELINE.md) para personajes; [`Assets/Art/Environment/README.md`](../Assets/Art/Environment/README.md) para entorno |

---

## B. Diseño de mundo

### B.1 Recomendación de estructura: **Hub + biomas periféricos**

Se recomienda **no** un mapa plano único ni un mundo gigante tipo AAA. La estructura óptima para tesis + MVP:

```
                    ┌─────────────────┐
                    │  Kernel Heights │  (bioma alto, boss/endgame)
                    └────────┬────────┘
                             │
              ┌──────────────┼──────────────┐
              │              │              │
     ┌────────┴────────┐ ┌───┴───┐ ┌────────┴────────┐
     │  Stack Ridge    │ │ HEAP  │ │  I/O Wastes     │
     │  (montañoso)    │ │ Wilds │ │  (desértico)    │
     └────────┬────────┘ └───┬───┘ └────────┬────────┘
              │              │              │
              └──────────────┼──────────────┘
                             │
                    ┌────────┴────────┐
                    │  RUNTIME PLAZA  │  ← Hub principal (NPCs, quests, tienda)
                    │  (Boot Sector)  │
                    └─────────────────┘
```

**Por qué hub + biomas (y no solo pueblo + wilderness genérico):**

1. **Pedagogía:** Cada bioma mapea a una región del SO (Boot, Heap, Stack, I/O, Kernel) — refuerza isomorfismo.
2. **Producción:** El hub concentra NPCs y servicios; los biomas reutilizan tiles/modular kits con tint por material.
3. **Tesis:** Demostrable en capturas ("plaza + 3 regiones explorables") sin necesitar 10 km².

**Alternativa descartada para MVP:** mapa único sin hub — dificulta quests, economía y fast travel pedagógico.

### B.2 Puntos de interés (POI)

| POI | Bioma | Función |
|---|---|---|
| Runtime Plaza | Boot / Hub | NPCs, quest board, terminal fast travel, tienda |
| GC Shrine | Boot | Tutorial opcional avanzado de GC |
| Corrupted Heap Cave | Heap Wilds | Dungeon instanciado #1 — leaks + puzzles de fragmentos |
| Stack Overflow Chasm | Stack Ridge | Dungeon #2 — plataformas + enemigos Spawner |
| I/O Relay Tower | I/O Wastes | Side quest escolta / reparación |
| Kernel Gate | Kernel Heights | Arena boss + victoria de arco principal |
| Checkpoints / Terminales | Todos | Save + respawn + fast travel desbloqueable |

### B.3 Dungeons / instancias

- **MVP:** 2 dungeons cerrados (escenas aditivas o sub-escenas con portal de entrada).
- **Criterio:** Mantener combate core; introducir mecánica nueva por dungeon (ej. Heap Cave → gestión de RAM límite; Stack Chasm → presión de CPU constante).
- **No bloqueante:** El overworld sigue siendo explorable sin completar dungeons.

### B.4 Escala recomendada (tesis)

| Alcance | Tamaño jugable | Contenido | Uso |
|---|---|---|---|
| **MVP tesis** | ~600 × 600 m (hub ~120 m diámetro + 3 biomas) | 5–8 quests, 2 dungeons, 1 boss, 5 NPCs, fast travel básico | Entrega principal de grado |
| **Visión extendida** | ~1,5 × 1,5 km, 5 biomas | 20+ quests, 4 dungeons, múltiples bosses, economía completa | Post-tesis / portfolio |
| **Fuera de alcance** | >3 km², streaming masivo, multijugador | — | No planificar ahora |

**Regla práctica:** Si caminar de hub a bioma más lejano toma **2–4 minutos** a pie, la escala es correcta para MVP.

---

## C. Mecánicas RPG core (priorizadas)

### C.1 Matriz de prioridad

| # | Mecánica | Prioridad | Reutiliza NullBorn | Notas |
|---|---|---|---|---|
| 1 | **Combate extendido** | P0 | ✅ Debug Strike, GC, Leaks | Combo ligero, i-frames, tipos de bug nuevos |
| 2 | **Progresión / XP** | P0 | 🟡 Integridad como base | "Prioridad de Thread" (nivel); sube max CPU/Integridad |
| 3 | **Quest system** | P0 | 🟡 TutorialDirector | ScriptableObjects + journal HUD |
| 4 | **Save / load** | P0 | 🟡 CheckpointSystem | JSON local → API (RF-17) |
| 5 | **Mundo abierto + navegación** | P0 | 🟡 ZoneTrigger → RegionTrigger | Terreno, minimapa, brújula |
| 6 | **Diálogo NPC** | P1 | ❌ | Nodos simples, 3–5 líneas por NPC MVP |
| 7 | **Inventario (Memory Cache)** | P1 | 🟡 MemoryFragment | Ítems consumibles, equipamiento de "módulos" |
| 8 | **Economía (CPU Credits)** | P1 | 🟡 CpuCycles | Recompensas de quest; tienda hub |
| 9 | **Fast travel** | P1 | ✅ Checkpoints | Terminales desbloqueables |
| 10 | **Barra RAM global** | P1 | ✅ Fragmentos | RF-07, RF-15 — presión visible |
| 11 | **Día/noche (ciclos carga)** | P2 | ❌ | Post-MVP si hay tiempo |
| 12 | **Crafting** | P3 | ❌ | Visión extendida |
| 13 | **Selección de nivel / auth** | P2 académico | 🟡 GameSession | RF-01, RF-13 — API |

### C.2 Detalle por mecánica clave

#### Combate (extensión)

**Existente:** melee a corto alcance (OverlapSphere), GC en área, leaks con drain + fragmentos + boss ENRAGE.

**Añadir:**
- Lock-on opcional (Tab) hacia leak más cercano.
- 2–3 enemigos nuevos como variantes pedagógicas:
  - `NullReferenceBug` — teletransporte corto (referencia nula).
  - `StackOverflowBrute` — lento, mucho daño de contacto.
  - `RaceConditionTwin` — dos entidades sincronizadas.
- Feedback: hit-stop breve, VFX por impacto, sonido placeholder.

#### Progresión / XP

- **Nombre diegético:** Prioridad de Thread (PT).
- Fuentes: quests, leaks derrotados, POI descubiertos.
- Subidas de nivel: +max Integridad, +max CPU, desbloqueo de habilidad (ej. GC mejorado nivel 3).
- **Isomorfismo:** subir prioridad = thread más importante para el scheduler.

#### Inventario — Memory Cache

- Slots limitados (8–16 MVP).
- Ítems: `DebugPatch` (cura), `CpuBoost` (regeneración acelerada), `RamExpander` (aumenta umbral RAM), `Module` equipable.
- UI: panel Tab o inventario rápido.

#### Quests

- Estructura: `QuestDefinition` (SO) → estados (NotStarted, Active, Completed, Failed).
- Tipos MVP: Kill X leaks, Collect fragments, Talk to NPC, Reach POI, Clear dungeon.
- Quest board en Runtime Plaza + seguimiento en HUD.

#### Diálogo

- Sistema de nodos lineales con ramas mínimas (2–3 opciones que afectan recompensa, no final múltiple).
- Reutilizar estilo de `TutorialDirector` para bocadillos.

#### Save system

- **Fase 1:** JSON en `Application.persistentDataPath` (posición, PT, quests, inventario, checkpoints desbloqueados).
- **Fase 2:** sync con API → entidades `Sesion`, `ProgresoSesion` (REQUIREMENTS).

#### Fast travel

- Terminales en hub + checkpoints descubiertos.
- Coste: CPU o cooldown diegético ("latencia de red").

### C.3 Isomorfismo ampliado (tabla objetivo)

| Mecánica RPG | Concepto software |
|---|---|
| Prioridad de Thread (XP/nivel) | Prioridad del hilo en el scheduler |
| Memory Cache (inventario) | Caché / memoria de trabajo limitada |
| CPU Credits (moneda) | Presupuesto de cómputo asignable |
| Quest / Ticket | Issue de Jira / bug report |
| NPC Daemon | Proceso en background del SO |
| Fast travel | Migración de proceso / socket |
| Día/noche (carga) | Patrones de tráfico del sistema |
| Dungeon Heap | Región de memoria fragmentada |
| Mapa / minimapa | Monitor de recursos del SO |

---

## D. Arquitectura técnica

### D.1 Estructura de carpetas propuesta

```
Assets/
├── Art/                          # (existente) + biomas, skyboxes, props
├── Data/                         # NUEVO — ScriptableObjects
│   ├── Quests/
│   ├── NPCs/
│   ├── Items/
│   ├── Enemies/
│   └── Regions/
├── Prefabs/
│   ├── Characters/
│   ├── Enemies/
│   ├── Environment/            # NUEVO — árboles, edificios, props
│   ├── NPCs/                     # NUEVO
│   └── Systems/                  # NUEVO — terminales, quest board
├── Scenes/
│   ├── World_Main.unity          # NUEVO — overworld principal
│   ├── Dungeon_HeapCave.unity    # NUEVO — aditiva
│   ├── Dungeon_StackChasm.unity  # NUEVO — aditiva
│   ├── Demo_10min.unity          # MANTENER — referencia / tutorial embebido
│   └── Prototype.unity             # MANTENER — pruebas aisladas
├── Scripts/
│   ├── Bootstrap/
│   │   ├── WorldBootstrap.cs     # NUEVO — reemplaza DemoBootstrap en world
│   │   └── DemoBootstrap.cs      # MANTENER
│   ├── Core/                     # (existente) + RamPressure.cs, SaveManager.cs
│   ├── Player/                   # (existente) + PlayerProgression.cs
│   ├── Enemies/                  # (existente) + nuevas variantes
│   ├── World/                    # NUEVO
│   │   ├── RegionTrigger.cs
│   │   ├── WorldMapController.cs
│   │   ├── AmbientSpawnDirector.cs
│   │   └── DayNightCycle.cs      # P2
│   ├── RPG/                      # NUEVO
│   │   ├── Quest/
│   │   ├── Dialogue/
│   │   ├── Inventory/
│   │   └── Economy/
│   ├── NPC/                      # NUEVO
│   │   ├── NpcController.cs
│   │   └── NpcPatrol.cs
│   ├── Persistence/              # NUEVO
│   │   ├── SaveData.cs
│   │   └── LocalSaveProvider.cs
│   ├── UI/                       # extender CanvasHUD → WorldHUD
│   ├── Demo/                     # MANTENER — legacy demo
│   └── Art/
└── Editor/
    ├── BuildWorldScene.cs        # NUEVO
    └── BuildDemo10MinScene.cs    # MANTENER
```

### D.2 ScriptableObjects clave

| SO | Campos principales |
|---|---|
| `RegionDefinition` | id, displayName, biomeMaterial, ambientColor, musicId, ramPressure |
| `QuestDefinition` | id, title, description, objectives[], rewards, prerequisiteQuests[] |
| `NpcDefinition` | id, displayName, dialogueTree, shopInventory?, patrolRoute |
| `ItemDefinition` | id, name, type, cpuCost?, integrityRestore?, icon |
| `EnemyDefinition` | variant, baseStats, spawnRegions[], pedagogicalTip |
| `GameBalanceConfig` | xpCurve, cpuRegen, ramThresholds, economyRates |

### D.3 Estrategia de escenas

| Enfoque | MVP recomendado | Evolución |
|---|---|---|
| **Escena única overworld** | `World_Main.unity` con Terrain + colliders + POI colocados | Menor complejidad, ideal tesis |
| **Dungeons aditivas** | `LoadSceneMode.Additive` al entrar en portal; unload al salir | Aísla carga y performance |
| **Streaming** | No en MVP | Si mapa >1 km²: Addressables + scene sectors |

**Decisión MVP:** Una escena overworld + 2 escenas dungeon aditivas. `Demo_10min` permanece como escena de tutorial opcional accesible desde el hub ("Simulación de arranque").

### D.4 Pipeline de assets para entorno a escala

1. **Terreno:** Unity Terrain 512×512 o 1024×1024, 3–4 TerrainLayers por bioma (tint albedo + normal suave).
2. **Modular kits:** importar 1 pack consistente; prefabs en `Prefabs/Environment/Modular/`.
3. **Props hero:** 10–15 modelos Blender (torre stack, celda RAM, terminal, antena) según Art Bible.
4. **Vegetación:** prefabs low-poly "node trees" instanciados con reglas por bioma (manual + `AmbientSpawnDirector`).
5. **Colisiones:** mesh colliders simplificados en props; terrain collider en suelo.
6. **Iluminación:** 1 Directional Light + baked o mixed donde sea posible; probes en hub.
7. **Niebla por región:** `RenderSettings.fog` ajustado por `RegionTrigger` (ya hay precedente en demo).

### D.5 Refactor de sistemas demo → world

| Componente demo | Acción |
|---|---|
| `DemoFlowController` | Reemplazar por `WorldMapController` + `QuestManager` para victoria |
| `DemoZoneId` | Migrar a `RegionId` enum + `RegionDefinition` SO |
| `ZoneTrigger` | Generalizar a `RegionTrigger` (mantiene checkpoint + tutorial opcional) |
| `DemoBootstrap` | `WorldBootstrap` registra todos los subsistemas RPG |
| `CanvasHUD` | Extender a `WorldHUD`: minimapa, quest tracker, RAM bar, inventario |
| `TutorialDirector` | Mantener; invocar desde quests y POI |

### D.6 Objetivos de rendimiento

| Métrica | Target MVP | Notas |
|---|---|---|
| FPS | ≥ 60 @ 1080p en GPU integrada media | RNF-02 |
| Draw calls | < 400 en overworld | Batching / GPU instancing en vegetation |
| Triángulos en pantalla | < 500k | LOD en props a >30 m |
| Memoria | Pool de fragmentos y VFX | Evitar Instantiate en combate intenso |
| Carga inicial | < 15 s | Escena única sin Addressables en MVP |

**Técnicas:** Occlusion Culling bake, LOD Groups en props, object pooling (`MemoryFragment`), URP Forward+ si hay muchas luces puntuales en hub.

---

## E. Fases de implementación

### Resumen de fases

| Fase | Nombre | Scope | Criterio de done |
|---|---|---|---|
| 0 | Documentación y GDD v2 | Docs, no código | GDD_v2 borrador aprobado por equipo |
| 1 | Fundación del mundo | Terreno, biomas, sky, iluminación, hub greybox | Exploración libre 5 min sin combate |
| 2 | Bootstrap world + regiones | WorldBootstrap, RegionTrigger, migración HUD básico | HUD muestra región; checkpoints funcionan |
| 3 | Combate extendido + enemigos | Lock-on, nuevos bugs, RAM bar | Combate en overworld estable |
| 4 | Progresión + save local | XP, niveles, SaveManager JSON | Cerrar y reabrir conserva progreso |
| 5 | Quests + diálogo | 5 quests, 3 NPCs, journal | Arco principal completable |
| 6 | Inventario + economía | Memory Cache, tienda hub | Comprar ítem y usar en combate |
| 7 | Dungeons + boss | 2 instancias + Kernel Gate boss | Victoria de arco RPG |
| 8 | Fast travel + polish | Terminales, audio, VFX, balance | Build Windows estable 60 FPS |
| 9 | Capa académica API | Auth, sesión, EventoGC (RF-17–20) | Demo con login institucional |

### Detalle por fase

#### Fase 0 — Documentación (1 semana)

- **Archivos:** `GDD_v2.md`, actualizar ARCHITECTURE, README, CHANGELOG, REQUIREMENTS.
- **Done:** Equipo alineado; Demo_10min re-etiquetado como "tutorial legacy".
- **Agente sugerido:** Plan / reasoning-heavy (decisiones de diseño, tablas de isomorfismo).

#### Fase 1 — Fundación del mundo (2–3 semanas)

- **Sistemas:** Terrain, skybox, fog por bioma, props modulares, Runtime Plaza layout.
- **Archivos nuevos:** `World_Main.unity`, `BuildWorldScene.cs`, prefabs Environment.
- **Done:** Jugador camina hub → 3 biomas; landmarks visibles; sin soft-lock.
- **Agente sugerido:** Implement + iteración artística (mucho trabajo en editor Unity).

#### Fase 2 — Bootstrap world + regiones (1 semana)

- **Sistemas:** `WorldBootstrap`, `RegionTrigger`, `RegionDefinition` SO, extensión HUD (nombre región).
- **Done:** Entrar a bioma actualiza HUD y checkpoint; `DemoBootstrap` no se usa en World_Main.
- **Agente sugerido:** Implement (C# modular, bajo riesgo).

#### Fase 3 — Combate extendido (1–2 semanas)

- **Sistemas:** Lock-on, `RamPressure`, nuevas variantes enemigas, pooling fragmentos.
- **Done:** 3 tipos de enemigo en overworld; barra RAM + alerta RF-15; combate ≥ calidad demo actual.
- **Agente sugerido:** Implement + playtest manual.

#### Fase 4 — Progresión + save (1 semana)

- **Sistemas:** `PlayerProgression`, `SaveManager`, `SaveData` serializable.
- **Done:** Subir de nivel tras quest/kills; guardado automático en terminales y al completar quest.
- **Agente sugerido:** Implement (lógica clara, tests manuales).

#### Fase 5 — Quests + diálogo (2 semanas)

- **Sistemas:** `QuestManager`, `DialogueRunner`, SOs de quest/NPC, quest board prefab.
- **Done:** 5 quests (1 main chain + 2 secundarias); 3 NPCs con diálogo; journal en HUD.
- **Agente sugerido:** Implement + redacción español (contenido pedagógico).

#### Fase 6 — Inventario + economía (1–2 semanas)

- **Sistemas:** `Inventory`, `Shop`, CPU Credits, UI inventario.
- **Done:** Loot de quest → inventario → consumir en combate; tienda en hub.
- **Agente sugerido:** Implement.

#### Fase 7 — Dungeons + boss (2 semanas)

- **Sistemas:** Escenas aditivas, portales, boss Kernel reutilizando `MemoryLeakEnemy` Boss.
- **Done:** Arco principal: quest → dungeon → boss → victoria RPG (no solo demo lineal).
- **Agente sugerido:** Implement + level design.

#### Fase 8 — Fast travel + polish (1–2 semanas)

- **Sistemas:** Terminales, minimapa, audio placeholder, balance pass, profiling.
- **Done:** Build Windows ≥60 FPS; fast travel entre hub y checkpoints descubiertos.
- **Agente sugerido:** Polish / perf (profiler, ajustes finos).

#### Fase 9 — API académica (paralelo o post-MVP)

- **Sistemas:** REST client Unity, auth institucional, sync save, EventoGC.
- **Done:** RF-17–20 en estado ✅ o 🟡 documentado.
- **Agente sugerido:** Implement backend + integración (fuera Unity si aplica).

### Tier de agente por tipo de trabajo

| Tier | Cuándo usar | Ejemplos en NullBorn |
|---|---|---|
| **Plan / arquitectura** | Decisiones de alcance, isomorfismo, estructura docs | Fase 0, diseño de quests pedagógicas |
| **Implement** | C# gameplay, SOs, bootstrap, UI | Fases 2–7 |
| **Content / art** | Terrain sculpt, prefab placement, Blender | Fase 1, props hero |
| **Polish / perf** | Profiler, VFX, audio, balance | Fase 8 |

---

## F. Actualizaciones Git / documentación

### F.1 Documentos a crear

| Documento | Contenido |
|---|---|
| `docs/GDD_v2.md` | Visión open world, mundo vivo, mecánicas RPG, fuera de alcance actualizado |
| `docs/WORLD_RPG_PLAN.md` | Este plan (persistente) |
| `docs/QUEST_DESIGN.md` | (Opcional Fase 5) Tabla de quests con objetivos pedagógicos |

### F.2 Documentos a actualizar

| Documento | Cambios |
|---|---|
| [`ARCHITECTURE.md`](../ARCHITECTURE.md) | Diagrama World_Main, carpetas World/RPG/Persistence, flujo quests, escena strategy |
| [`README.md`](../README.md) | Estado "Open World RPG (en desarrollo)", escena principal World_Main, quitar énfasis en "10 min demo" |
| [`CHANGELOG.md`](../CHANGELOG.md) | Entrada Unreleased: "Pivot a open world RPG — plan docs/WORLD_RPG_PLAN.md" |
| [`docs/REQUIREMENTS.md`](REQUIREMENTS.md) | RF-04 ampliado (exploración open world); RF-13 (selección región/mundo); nuevos RF opcionales para quests/inventario; prioridades sprint |
| [`docs/GDD_v1.md`](GDD_v1.md) | Banner al inicio: "Superseded by GDD_v2 for world scope; retained for demo reference" |
| [`docs/BLENDER_PIPELINE.md`](BLENDER_PIPELINE.md) | Sección props de entorno + modular kit workflow |
| [`Assets/Art/Environment/README.md`](../Assets/Art/Environment/README.md) | Estructura biomas, naming conventions |

### F.3 Qué no borrar (compatibilidad)

- `Demo_10min.unity` y `BuildDemo10MinScene.cs` — útiles como tutorial embebido y regresión del combate core.
- `Prototype.unity` — sandbox de pruebas.
- Namespace y clases Core/Player/Enemies — extender, no reescribir.

### F.4 Commits sugeridos (cuando el usuario lo pida)

1. `docs: add WORLD_RPG_PLAN and GDD v2 draft`
2. `docs: update ARCHITECTURE and README for open world pivot`
3. Por fase de implementación: commits atómicos por sistema (`feat(world): add RegionTrigger and WorldBootstrap`)

---

## Apéndice A — Gap analysis rápido

```
EXISTE                          NECESARIO PARA MVP RPG
─────────────────────────────────────────────────────────
SystemIntegrity, CpuCycles  →   + PlayerProgression, RamPressure
MemoryLeakEnemy             →   + 2–3 variantes, spawn director
GC + Debug Strike           →   + lock-on, feedback VFX
CheckpointSystem            →   + SaveManager, fast travel terminals
CanvasHUD                   →   + WorldHUD (map, quests, RAM, inv)
TutorialDirector            →   + DialogueRunner
ZoneTrigger (lineal)        →   RegionTrigger (multi-región)
DemoFlowController          →   QuestManager + WorldMapController
Greybox primitivas          →   Terrain + modular environment
GameSession local           →   SaveData + (opcional) API sync
```

## Apéndice B — Riesgos y mitigaciones

| Riesgo | Impacto | Mitigación |
|---|---|---|
| Scope creep (MMO-like) | Alto | Límite 600×600 m MVP; lista P0 estricta |
| Arte insuficiente | Medio | 1 kit modular + greybox temático; Blender solo hero props |
| Performance en terrain | Medio | Profiling Fase 8; LOD; limitar spawns activos |
| Desalineación pedagógica | Alto | Cada quest mapea a 1 concepto; revisión docente |
| Pérdida del demo funcional | Bajo | Mantener Demo_10min intacto |

## Apéndice C — Referencias internas

- [`docs/GDD_v1.md`](GDD_v1.md) — Art Bible, isomorfismo, controles
- [`docs/GDD_v0.md`](GDD_v0.md) — Core loop original
- [`ARCHITECTURE.md`](../ARCHITECTURE.md) — Arquitectura actual
- [`docs/REQUIREMENTS.md`](REQUIREMENTS.md) — RF/RNF académicos
- [`CHANGELOG.md`](../CHANGELOG.md) — Historial de versiones

---

*Documento generado como plan de pivot — NullBorn Open World RPG. Implementación de código pendiente de aprobación del equipo.*
