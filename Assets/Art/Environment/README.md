# Environment

Props hero y kits low-poly (KayKit / Quaternius / Asset Store) van aquí.

## Estado actual
`WorldLayoutBuilder` genera un greybox temático en Play (planos por bioma, landmarks, anillos de plaza). No requiere descargas del Asset Store.

## Estructura objetivo

| Carpeta | Contenido |
|---|---|
| `Modular/` | Kits de edificios / tiles |
| `Heap/` | Props magenta / nodos |
| `Stack/` | Torres y terrazas |
| `IO/` | Antenas, dunas de bus |
| `Hub/` | Plaza, terminales, stalls |

**Naming:** `Prop_<Biome>_<Name>` (ej. `Prop_Heap_NodeTree`). Materiales URP `Mat_` en `Assets/Art/Materials/`.

El generador de Demo_10min sigue creando un greybox lineal con materiales URP.
