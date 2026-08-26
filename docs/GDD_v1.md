# GDD v1 — NullBorn Mini-Demo (~10 min)

## Visión
Action-RPG 3D en tercera persona. El jugador es un **Thread** que estabiliza un sistema operativo corrupto gestionando **ciclos de CPU** y ejecutando **Garbage Collection**. Aprendizaje por isomorfismo lúdico (sin quizzes).

## Isomorfismo (mapa pedagógico)

| Juego | Concepto de software |
|---|---|
| Integridad del Sistema | Estabilidad / “HP” del OS |
| Ciclos de CPU | Presupuesto de cómputo por acción |
| Memory Leak | Fuga de memoria (enemigo) |
| Memory Fragment | Bloque de RAM no liberada |
| Garbage Collection (E/G) | Liberar memoria + restaurar integridad |
| Depurar (Click/F) | Eliminar código corrupto (cuesta CPU) |
| Jugador | Hilo de ejecución (Thread) |
| Boot / Heap / Kernel | Zonas del “proceso” mental del estudiante |

## Art Bible (1 página)

**Tema:** interior de un SO bajo estrés (circuitos, salas de kernel, celdas RAM). No fantasy medieval.

| Elemento | Silueta / color |
|---|---|
| Player_Thread | Humanoide claro, acento **cian** `#40C0FF` |
| Memory Leak | Blob inestable, **magenta/rojo** `#E6285A` → `#B010E0` |
| Fragments | Cubos RAM **naranja** `#FF8C1A` |
| Entorno sano | Oscuro `#1A1E28` + emission cian |
| Entorno corrupto | Oscuro + emission magenta |

**Paleta URP:** Lit + emission suave. Fog ligero por zona.

## Flow del demo (~10 min)

```
Boot Sector (tutorial) → Heap Corrupto (tradeoff) → Kernel Core (boss) → System Stabilized
```

| Zona | ~Tiempo | Meta |
|---|---|---|
| 1 Boot Sector | 2.5 min | Controles, 1 leak débil, aprender GC |
| 2 Heap Corrupto | 4 min | Varios leaks + presión de CPU |
| 3 Kernel Core | 3.5 min | Mini-boss por fases |

**Victoria:** derrotar al Leak Boss del Kernel.  
**Derrota:** Integridad = 0.  
**Checkpoint:** al entrar a cada zona.

## Assets: packs vs propios

### Packs / placeholders (rápido)
- Modular floors/walls low-poly (primitivas temáticas o KayKit/Quaternius cuando se importen)
- Materiales URP en `Assets/Art/Materials/`
- VFX mínimos: flash GC, muerte leak (escala + color)

### Propios Blender (prioridad, reemplazan placeholders)
1. `Player_Thread` (+ Mixamo walk/idle/attack)
2. `Enemy_MemoryLeak` (+ idle/pulse/death)
3. Props hero (≤6): panel registros, torre stack, celda RAM, portal zona, consola kernel, antena bus

### Carpeta Unity
```
Assets/Art/Characters/
Assets/Art/Environment/
Assets/Art/Materials/
Assets/Art/VFX/
Assets/Prefabs/
Assets/Scenes/Demo_10min.unity
```

## Controles (PC Windows)
WASD mover · Mouse mirar · Click/F depurar · E/G GC · R reiniciar · Esc cursor/pausa

## Fuera de alcance
Multijugador, inventario RPG, open world, HDRP, más de 1 boss, narrativa cinemática larga.
