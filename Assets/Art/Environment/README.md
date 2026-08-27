# Environment

## Estado actual (arte procedural de entorno)

`WorldLayoutBuilder` genera en Play una **Ciudad-Runtime** con:

- Terreno continuo con altura Perlin (~220 m)
- Biomas con suelos texturizados (noise/grid runtime)
- Árboles vivos / muertos (`EnvironmentKit`)
- Edificios modulares con ventanas y techos
- Antenas I/O, farolas con point lights, rocas
- Caminos y puentes entre regiones
- Skybox cubemap degradado + fog + sol con soft shadows

Scripts clave:
- `Assets/Scripts/Art/EnvironmentKit.cs`
- `Assets/Scripts/Art/ProceduralTextures.cs`
- `Assets/Scripts/Art/RuntimeMaterials.cs`

## Siguiente nivel (assets externos)

Cuando puedas importar packs CC0 / Asset Store:

| Carpeta | Contenido |
|---|---|
| `Modular/` | KayKit / Quaternius / Synty buildings |
| `Nature/` | Kenney Nature Kit (árboles FBX) |
| `Heap/` / `Stack/` / `IO/` / `Hub/` | Props hero Blender |

**Naming:** `Prop_<Biome>_<Name>`. Sustituir llamadas de `EnvironmentKit` por `Instantiate(prefab)` sin tocar gameplay (leaks, regions, quests).

## Cómo regenerar

1. Abrir `World_Main.unity` vacío → Play, o
2. Menú **NullBorn → Build World Main**
