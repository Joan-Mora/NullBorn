# GDD v0 — NullBorn (borrador vivo)

## Visión en una frase
Action-RPG 3D en tercera persona donde el jugador es un **Thread** que debe contener **Memory Leaks** gestionando **ciclos de CPU** y ejecutando **Garbage Collection**, para enseñar gestión de recursos computacionales sin quizzes.

## Alcance del Vertical Slice
- 1 arena 3D jugable
- 1 enemigo: Memory Leak
- 2 recursos: Integridad del Sistema + Ciclos de CPU
- 2 acciones: Depurar + Garbage Collection
- Condiciones de victoria/derrota claras

## Loop central (Core Loop)
1. El Leak drena integridad y genera fragmentos de memoria
2. El jugador decide: atacar (CPU) o limpiar con GC (más CPU, restaura integridad)
3. Si malgasta CPU, queda vulnerable
4. Si ignora el Leak, el sistema colapsa

## Controles (PC Windows)
- WASD mover, Mouse mirar
- Click/F depurar
- E/G Garbage Collection
- R reiniciar

## Fuera de alcance (por ahora)
Narrativa completa, inventario, múltiples niveles, multijugador, arte final.
