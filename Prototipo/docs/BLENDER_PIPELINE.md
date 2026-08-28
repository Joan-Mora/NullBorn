# Guía Blender → Unity (Player_Thread)

## Objetivo
Reemplazar el placeholder cian del demo por un FBX propio.

## Pasos recomendados
1. Modelar humanoide simple (proporciones claras, pocas caras).
2. Exportar a Mixamo → Auto-Rigger → animaciones Idle / Walking / Punch / Dodge.
3. Descargar FBX for Unity (With Skin).
4. Importar en `Assets/Art/Characters/Player_Thread/`.
5. Rig: Humanoid. Crear Animator Controller `AC_PlayerThread` con parámetros `Speed`, `Attack`, `Dash`.
6. Sustituir el hijo `Visual_ThreadAvatar` del prefab `Player_Thread` por el modelo.
7. Asignar el Animator a `PlayerAnimBridge` (ya está en el jugador). Hasta entonces el puente usa **punch procedural** (escala/brazos).
8. Mantener CharacterController + scripts en la raíz del prefab.

## Props de entorno (arte)

El overworld ya tiene **entorno procedural** (`EnvironmentKit` + terreno continuo + vegetación + edificios). Para subir a assets reales:

1. Kits modulares low-poly (KayKit / Quaternius / Synty / Kenney) → `Assets/Art/Environment/Modular/`.
2. 10–15 hero props en Blender (torre stack, celda RAM, terminal, antena) según Art Bible.
3. Opcional: Unity Terrain 512–1024 sustituyendo `EnvironmentKit.GroundMesh`.
4. Reemplazar `EnvironmentKit.Tree/Building` por `Instantiate(prefab)` en `WorldLayoutBuilder`.

## Placeholder actual
`ThreadAvatarBuilder` + `PlayerAnimBridge` procedural hasta que exista el FBX Mixamo.