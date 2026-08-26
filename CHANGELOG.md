# Changelog

Todos los cambios notables de este proyecto se documentan en este archivo.

El formato sigue [Keep a Changelog](https://keepachangelog.com/es-ES/1.0.0/), y el versionado intenta alinearse con [Semantic Versioning](https://semver.org/lang/es/).

## [Unreleased]

### Fixed
- GUID inválido en `SimpleBobAnim.cs.meta` (Unity ignoraba el script → error CS0246 en compilación).
- Multiplicación inválida `Vector3 * (float, float, float)` en `LeakAvatarBuilder.CreateSpike` (corregido con `new Vector3(...)`).
- Referencias de script `SimpleBobAnim` en prefabs (`Player_Thread`, `Leak_*`) y escena `Demo_10min.unity` alineadas con el GUID del `.meta`.
- HUD Canvas con texto vertical ilegible (RectTransform, CanvasScaler, overflow).
- Barra de leak mostrando **LEAK BOSS** en Boot Sector antes de llegar al Kernel.
- Carteles de zona y siluetas placeholder más legibles.

### Changed
- Requisitos RQF/RQNF movidos de `.pptx` a [`docs/REQUIREMENTS.md`](docs/REQUIREMENTS.md); archivos `.pptx` eliminados del repo y añadidos a `.gitignore`.

### Added
- `SimpleBobAnim.cs` como script independiente.
- `WorldZoneSign` para señalización 3D de zonas.
- Documentación: `ARCHITECTURE.md`, `SECURITY.md`, `docs/REQUIREMENTS.md`.
- Plan de pivot a **open world RPG**: [`docs/WORLD_RPG_PLAN.md`](docs/WORLD_RPG_PLAN.md) — hoja de ruta desde el vertical slice lineal (`Demo_10min`) hacia mundo abierto hub + biomas, quests, save local y arquitectura `World/` / `RPG/` / `Persistence/` (sin implementación de código aún).

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
