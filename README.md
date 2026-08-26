# NullBorn

**NullBorn** es un prototipo Action-RPG 3D educativo (vertical slice) desarrollado en Unity 6 con URP. El jugador controla un **Thread** que debe estabilizar un sistema operativo corrupto mediante **Garbage Collection**, **depuración** y gestión de **ciclos de CPU**, usando **isomorfismo lúdico** para gamificar conceptos básicos de desarrollo de software.

## Estado

Vertical slice **Demo_10min** jugable en Unity 6 (Boot → Heap → Kernel). Compila sin errores en **6000.5.9f1** tras alinear GUID de `SimpleBobAnim` y corregir builders de avatar.

## Requisitos

| Componente | Versión |
|---|---|
| Unity | **6000.5.9f1** (Unity 6) |
| Plataforma objetivo | Windows 10/11 x64 |
| Render pipeline | Universal Render Pipeline (URP) |

## Cómo ejecutar el demo

1. Abre esta carpeta en **Unity Hub** con Unity **6000.5.9f1**.
2. Menú **NullBorn → Generar Demo 10min** (regenera `Assets/Scenes/Demo_10min.unity` si hace falta).
3. Abre la escena `Assets/Scenes/Demo_10min.unity`.
4. Pulsa **Play ►** y haz **click en la ventana Game** para capturar el mouse.

### Controles

| Tecla | Acción |
|---|---|
| WASD | Mover el Thread |
| Mouse | Cámara en tercera persona |
| Click / F | Depurar (cuesta CPU, daña Memory Leaks) |
| E / G | Garbage Collection (libera fragmentos, restaura integridad) |
| Esc | Pausa / liberar cursor |
| R | Respawn en checkpoint / reiniciar demo |

### Estructura del demo (~10 min)

Recorrido lineal de tres zonas:

1. **Boot Sector** — Tutorial: controles, CPU, GC y un leak débil.
2. **Heap Corrupto** — Varios leaks y tradeoff depurar vs limpiar basura.
3. **Kernel Core** — Mini-boss **Leak Boss** con fase ENRAGE.

**Victoria:** derrotar al Leak Boss.  
**Derrota:** Integridad del Sistema = 0 (SYSTEM COLLAPSE).

## Tech stack

- **Motor:** Unity 6 + URP + uGUI
- **Lenguaje:** C# (`NullBorn` namespace)
- **Arte:** Primitivas URP + materiales temáticos (`Assets/Art/Materials/`)
- **Persistencia (plan académico):** Unity → API → PostgreSQL (pendiente en este slice)
- **Documentación:** [`CHANGELOG.md`](CHANGELOG.md), [`ARCHITECTURE.md`](ARCHITECTURE.md), [`docs/GDD_v1.md`](docs/GDD_v1.md), [`docs/REQUIREMENTS.md`](docs/REQUIREMENTS.md)

## Escenas

| Escena | Propósito |
|---|---|
| `Demo_10min` | Mini-demo principal (~10 min) |
| `Prototype` | Arena mínima de pruebas |

## Build Windows

Menú **NullBorn → Build Windows (.exe)** → salida en `Builds/Windows/NullBorn.exe`

## Regenerar escena (CLI)

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.5.9f1\Editor\Unity.exe" `
  -batchmode -nographics -quit `
  -projectPath "C:\Users\joanm\Documents\GitHub\NullBorn" `
  -executeMethod NullBorn.EditorTools.BuildDemo10MinScene.BuildBatch `
  -logFile "Logs\batch-build.log"
```

## Equipo

<table>
  <tr>
    <td align="center">
      <img src="https://github.com/Joan-Mora.png" width="100px;" alt="Darwin Joan Aveiga Mora"/><br />
      <sub><b>Darwin Joan Aveiga Mora</b></sub><br />
      <a href="https://github.com/Joan-Mora">@Joan-Mora</a><br />
      <sub>Desarrollador Full Stack</sub>
    </td>
    <td align="center">
      <img src="https://github.com/MrRoar154.png" width="100px;" alt="Julian Esteban Chavez Zamora"/><br />
      <sub><b>Julian Esteban Chavez Zamora</b></sub><br />
      <a href="https://github.com/MrRoar154">@MrRoar154</a><br />
      <sub>Desarrollador Full Stack</sub>
    </td>
  </tr>
</table>

---
