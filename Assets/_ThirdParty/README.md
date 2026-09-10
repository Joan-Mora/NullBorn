# Third-party assets (Unity-ready)

Packs Unity-ready bajo `Assets/_ThirdParty/`. Inbox local de descargas: `_Inbox/SourcePacks/`. Solo formatos Unity (FBX + texturas); se excluyeron glTF/OBJ/Blender/Godot/Unreal y `__MACOSX`.

| Carpeta | Pack |
|---------|------|
| `Nature/` | Stylized Nature MegaKit — `FBX (Unity)` + texturas |
| `Village/` | Medieval Village MegaKit — FBX + texturas |
| `Characters/Base` | Universal Base Characters |
| `Characters/Hairstyles` | Pelos / cejas (rig a head bone) |
| `Characters/Outfits` | Modular Character Outfits – Fantasy |
| `Characters/Animations` | Universal Animation Library 1 y 2 |
| `Armory/` | STYLIZED Fantasy Armory (FBX + TEXTURE) |
| `Animals/` | Ultimate Animated Animal Pack |
| `Water/` | URP Shaders Collection |
| `UI/` | UIBundleFree (revisar: extracto actual parece solo previews) |
| `VFX/` | Fire / Zap `.unitypackage` + Highlight Shader |

## Tras abrir Unity

1. Espera a que importe todo (barra inferior).
2. Doble clic en `VFX/Vefects_*.unitypackage` e Import All.
3. Doble clic en `VFX/Free Highlight Shader.../*.unitypackage` e Import All.
4. Monstruos: Package Manager → My Assets (si aún no están).

Los originales comprimidos viven en `_Inbox/` / `Original/` (ignorados por git).

## Errores conocidos — cómo solucionarlos

### Shader Graph — “newer version of this node”
**Fix aplicado:** nodos actualizados en los `.shadergraph` / `.shadersubgraph` (vía `tools/upgrade_shader_graph_nodes.py`).

Si vuelve a aparecer tras copiar assets de nuevo, en Unity:
**NullBorn → Fix Asset Warnings → Upgrade Water Shader Graph Nodes**

### Alpaca — self-intersecting polygon
El mesh del pack trae 1 polígono inválido; Unity lo descarta pero avisa en cada reimport.

**Fix en Unity:** **NullBorn → Fix Asset Warnings → Fix Alpaca Mesh**
- Guarda mesh limpio en `Animals/Meshes/Alpaca_Mesh.asset`
- Extrae animaciones a `Animals/Animations/Alpaca/`
- Archiva el FBX en `_Inbox/Original/Animals/` (fuera del import activo)

O ejecuta **Run All Fixes** para ambos.

Si ves errores de `Water/URP` o `BubbleShader` tras archivar demos:
**NullBorn → Fix Asset Warnings → Clean Water Demo Artifacts**

(No uses el menú antiguo de Shader Graph por reflexión — ahora usa Python automáticamente.)

### Water/URP duplicado — `URPAsset_Renderer inconsistent`
El pack traía un pipeline URP de **demo** en `Water/URP/` que choca con el proyecto (`Assets/Settings/URP_Pipeline`). Esa carpeta fue archivada fuera del pipeline activo (p. ej. `_Inbox` / Archive).

### BubbleShader — Reflection Probe deprecated
Shader **demo** del pack (no lo usa NullBorn). Archivado fuera del pipeline activo. Agua del juego = `Materials/Surfaces/ToonWater.mat`.
