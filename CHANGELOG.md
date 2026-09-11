# Changelog

Cambios relevantes de NullBorn. Fechas en ISO-8601.

El vertical slice de agosto de 2026 (Heap / Kernel, avatar de primitivas) se **descatalogó** el 27 de agosto. El producto actual es el rebuild stylized en la raíz del repo, Unity **6000.5.10f1** + URP.

## [0.5.0] — 2026-09-10

Slice **Cap 0** jugable en ArtTest y companion en tiempo real.

### Juego
- Personaje Male_Ranger sobre base Superhero, Hair_Buzzed, espada única (funda en pecho / grip en `hand_r`).
- Locomoción discreta Idle / Walk / Run / Backpedal / Roll (`PlayerLocomotion`).
- Combate melee de un corte (capa Attack + máscara de torso) y dummy con hit flash.
- Pausa: esquemas Con mouse / Solo teclado; relock de cursor; overlay FPS (F3) sin capar a 60.
- Baseline URP (MSAA 2×, sombras medias, SRP Batcher) y menú **Build Windows Player**.
- Highlight de interactuables (rim URP), no outline comic en el entorno.

### Companion
- Unity se conecta a `http://127.0.0.1:5080` (offline-first si la API está caída).
- Pose del jugador a ~14 Hz (deadzone 1 cm / 1.5°); un PUT en vuelo; markers a ~1 Hz.
- Minimapa Unity: pose cada `LateUpdate`.
- PWA: dibujo a 60 fps, interpolación de flecha, poll REST solo si SignalR cae.
- Icono del juego en favicon, header y `manifest.webmanifest`.

### Docs
- README de portafolio/tesis, marca en `docs/brand/`.
- Eliminado `docs/archive/` (GDD y planes del prototipo descatalogado).

## [0.4.0] — 2026-09-10

Repositorio unificado y mapa táctico desplegable.

- Unity en la raíz (`Assets/`, `Packages/`, `ProjectSettings/`); carpeta `Prototipo/` fuera.
- Companion `Server/`: ASP.NET, PostgreSQL, PWA Cartógrafo, SignalR.
- Deploy: PWA en Render, API + Postgres en local, túnel HTTPS (`DEPLOY.md`, `tools/start-tunnel.ps1`).

## [0.3.0] — 2026-08-28

- Versión de equipo fijada en **Unity 6000.5.10f1**.
- Restauración temporal del proyecto pre-reset bajo `Prototipo/` (luego absorbido en 0.4.0).

## [0.2.0] — 2026-08-27

- Reset a asset stack stylized (`ASSET_STACK.md`, `Assets/_ThirdParty/`).
- Se retira el mundo procedural / combate del slice anterior.

## [0.1.0] — 2026-08-26

- Commit inicial: vertical slice Unity 6 (visión GDD, plan open-world, fases 0–9).
- **Superseded** por 0.2.0. No forma parte del binario ni de ArtTest actuales.
