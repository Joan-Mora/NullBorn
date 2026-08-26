# Guía Blender → Unity (Player_Thread)

## Objetivo
Reemplazar el placeholder cian del demo por un FBX propio.

## Pasos recomendados
1. Modelar humanoide simple (proporciones claras, pocas caras).
2. Exportar a Mixamo → Auto-Rigger → animaciones Idle / Walking / Punch.
3. Descargar FBX for Unity (With Skin).
4. Importar en `Assets/Art/Characters/Player_Thread/`.
5. Rig: Humanoid. Crear Animator Controller `AC_PlayerThread`.
6. Sustituir el hijo `Visual` del prefab `Player_Thread` por el modelo.
7. Mantener CharacterController + scripts en la raíz del prefab.

## Placeholder actual
El demo genera un avatar compuesto (cápsula + “núcleo” cian) vía `ThreadAvatarBuilder` hasta que exista el FBX.
