# Changelog

Todos los cambios notables de este proyecto se documentan en este archivo.

El formato sigue [Keep a Changelog](https://keepachangelog.com/es-ES/1.0.0/), y el versionado intenta alinearse con [Semantic Versioning](https://semver.org/lang/es/).

## [Unreleased]

### Added
- **Fases 0–9 implementadas** (plan [`docs/WORLD_RPG_PLAN.md`](docs/WORLD_RPG_PLAN.md)): docs + combate 0b, overworld greybox, bootstrap/regiones, combate en mundo, progresión/save, quests/diálogo, inventario/economía, dungeons/boss, fast travel/polish, API mock/stub.
- **Fase 0b combate:** sprint, dash con i-frames, buffer de input, `TraceCutCombo` (L-L-H), `PurgeWaveAbility`, `RestoreAbility`, `LockOnController`, `CombatFeedback` (hitstop/flash/shake/SFX), `PlayerAnimBridge` procedural, escena `Combat_Arena`.
- **Mundo:** `World_Main` greybox (Runtime Plaza + Heap / Stack / I/O / Kernel), `WorldBootstrap`, `RegionTrigger`, `RegionDefinition`, `WorldLayoutBuilder`.
- **RPG:** `PlayerProgression`, `QuestManager` (6 tickets), `DialogueRunner`, 4 NPCs, `Inventory`, `Shop`, `CpuCredits`.
- **Persistencia:** `SaveManager` JSON local; F5/F9; auto-save en terminales.
- **Dungeons:** portales aditivos + greybox in-world Heap Cave / Stack Chasm; Kernel Gate boss.
- **Fast travel + minimapa stub** y barra `RamPressure` (RF-07/15).
- **API stub:** `INullBornApi` + `MockNullBornApi` + `NullBornApiClient` (RF-17–20 parcial).
- Menús editor **NullBorn → Build Combat Arena / Build World Main / Build Dungeon Scenes**.
- **`docs/GDD_v2.md`** — visión Guardián del Runtime.
- Plan de pivot [`docs/WORLD_RPG_PLAN.md`](docs/WORLD_RPG_PLAN.md).

### Changed
- HUD: etiquetas **Vitalidad / Energía**; objetivo de combate; overlays de quest/RAM/inventario.
- `MemoryLeakEnemy`: FSM Idle → Telegraph → Attack → Recover; drain pasivo reducido; variante `NullRef`.
- `Demo_10min` conserva GC en E/G; gana combo/dash si el bootstrap añade el kit v2.
- **`docs/GDD_v1.md`** marcado como superseded por GDD v2.
- **`docs/REQUIREMENTS.md`** — RF-07/13/15/17–20 y RF-21–24 actualizados.

### Fixed
- GUID inválido en `SimpleBobAnim.cs.meta` (Unity ignoraba el script → error CS0246 en compilación).
- Multiplicación inválida `Vector3 * (float, float, float)` en `LeakAvatarBuilder.CreateSpike` (corregido con `new Vector3(...)`).
- Referencias de script `SimpleBobAnim` en prefabs (`Player_Thread`, `Leak_*`) y escena `Demo_10min.unity` alineadas con el GUID del `.meta`.
- HUD Canvas con texto vertical ilegible (RectTransform, CanvasScaler, overflow).
- Barra de leak mostrando **LEAK BOSS** en Boot Sector antes de llegar al Kernel.
- Carteles de zona y siluetas placeholder más legibles.

## [0.2.0] — 2026-08-26

### Added
- Mini-demo **Demo_10min**: 3 zonas lineales (Boot / Heap / Kernel).
- Canvas HUD uGUI con barras de Integridad, CPU y leak activo.
- `TutorialDirector` con mensajes pedagógicos en español.
- Variantes de enemigo: Basic, Spawner, Boss (Leak Boss + ENRAGE).
- Sistema de checkpoints y respawn (tecla R).
- Materiales URP temáticos y avatares procedurales (`ThreadAvatarBuilder`, `LeakAvatarBuilder`).
- Menú editor **NullBorn → Generar Demo 10min** y build Windows.

## [0.1.0] — 2026-08 (prototipo inicial)

### Added
- Prototipo `Prototype.unity` con loop básico Action-RPG.
- Isomorfismo lúdico: Integridad (HP), CPU (stamina), Memory Leak, GC.
- Control en tercera persona, cámara orbit, habilidades Debug Strike y Garbage Collection.
- Migración a Unity 6 + URP.
- GDD v0/v1 en `docs/`.

[Unreleased]: https://github.com/Joan-Mora/NullBorn/compare/v0.2.0...HEAD
[0.2.0]: https://github.com/Joan-Mora/NullBorn/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/Joan-Mora/NullBorn/releases/tag/v0.1.0
