# Requerimientos — NullBorn

Resumen extraído de **Técnicas de Modelamiento de Software — Tablas RQF/RQNF** (UNIMINUTO Villavicencio, agosto 2026) y mapeo al estado actual del prototipo Unity.

**Fuente:** Tablas RQF/RQNF del proyecto de grado (UNIMINUTO Villavicencio, agosto 2026). El documento original no se versiona en el repo; este archivo es la referencia canónica.

**Alcance del documento académico:** Prototipo Action-RPG 3D para gamificar conceptos básicos de desarrollo de software mediante isomorfismo lúdico (Integridad = HP, CPU = stamina, enemigos = fugas, GC = cura). Arquitectura objetivo: **Unity → API → PostgreSQL OLTP**.

---

## Leyenda de estado

| Estado | Significado |
|---|---|
| ✅ Hecho | Implementado en el repo actual |
| 🟡 Parcial | Existe versión simplificada o solo en cliente offline |
| ❌ Pendiente | No implementado en este slice |

---

## Requerimientos funcionales (RF)

| Código | Nombre | Descripción (resumida) | Estado NullBorn | Notas |
|---|---|---|---|---|
| RF-01 | Identificación del estudiante | Registro/login con correo institucional | ❌ Pendiente | Sin pantalla de auth; entidad `Jugador` no modelada |
| RF-02 | Gestión de perfil | Consultar/editar perfil; correo oculto en HUD | ❌ Pendiente | HUD no muestra datos personales (alineado al criterio) |
| RF-03 | Inicio de sesión de juego | Crear `Sesion` al comenzar partida | 🟡 Parcial | `GameSession` local sin persistencia ni ID único en BD |
| RF-04 | Bucle Action-RPG | Personaje 3D, exploración y combate | ✅ Hecho | `Demo_10min`: movimiento, combate, GC, 3 zonas |
| RF-05 | HUD integridad | Integridad 0–100 en tiempo real | ✅ Hecho | `CanvasHUD` + `SystemIntegrity` |
| RF-06 | HUD ciclos CPU | Stamina como CPU | ✅ Hecho | `CpuCycles` + barra HUD |
| RF-07 | Indicador RAM | RAM usada vs total del nivel | 🟡 Parcial | Fragmentos y crecimiento de leaks implican presión de RAM; **sin barra RAM dedicada** |
| RF-08 | Enemigos bugs/fugas | Catálogo `TipoBug` → `InstanciaBug` | 🟡 Parcial | `MemoryLeakEnemy` (Basic/Spawner/Boss); sin API ni entidades OLTP |
| RF-09 | Daño a integridad | Fugas y saturación reducen integridad | ✅ Hecho | Drain por leak + fragmentos |
| RF-10 | Garbage Collection | Curación + libera RAM; consume CPU | ✅ Hecho | `GarbageCollectionAbility`; sin `EventoGC` en BD |
| RF-11 | Derrota (crash) | Integridad = 0 → fin de partida | ✅ Hecho | `GameSession.State.Defeat` + overlay HUD |
| RF-12 | Victoria | Objetivos de nivel completados | ✅ Hecho | Derrotar Leak Boss → `NotifyVictory()` |
| RF-13 | Selección de nivel | Elegir `Nivel` del catálogo | ❌ Pendiente | Un solo nivel demo hardcodeado |
| RF-14 | Dificultad por nivel | Frecuencia/daño según dificultad | 🟡 Parcial | Parámetros por variante de leak en escena, no desde BD |
| RF-15 | Alerta RAM crítica | AV cuando RAM ≥ umbral | ❌ Pendiente | Sin umbral visual/sonoro global de RAM |
| RF-16 | Pausa | Pausar sin perder estado | ✅ Hecho | Esc + `Time.timeScale = 0` |
| RF-17 | Persistencia progreso | API → PostgreSQL | ❌ Pendiente | Solo estado en memoria |
| RF-18 | EventoGC transaccional | Registro ACID por GC | ❌ Pendiente | GC solo en cliente |
| RF-19 | Historial de partidas | Consulta sesiones vía API | ❌ Pendiente | — |
| RF-20 | Cierre de sesión | Finalizar y persistir | 🟡 Parcial | Fin local victoria/derrota; sin cierre en BD |

**Resumen RF:** 8 ✅ · 7 🟡 · 5 ❌ (de 20)

---

## Requerimientos no funcionales (RNF)

| Código | Nombre | Prioridad | Estado NullBorn | Notas |
|---|---|---|---|---|
| RNF-01 | Usabilidad | Alta | ✅ Hecho | HUD etiquetado, tutorial integrado, controles en ≤5 min |
| RNF-02 | Rendimiento 60 FPS | Alta | 🟡 Parcial | Slice ligero; no perfilado formal en laboratorio |
| RNF-03 | API < 3 s | Alta | ❌ Pendiente | Sin API en repo |
| RNF-04 | Seguridad datos estudiante | Alta | 🟡 Parcial | Sin auth aún; ver [`SECURITY.md`](../SECURITY.md) |
| RNF-05 | Disponibilidad servicio | Media | ❌ Pendiente | — |
| RNF-06 | Portabilidad Windows | Alta | ✅ Hecho | Build Windows vía menú editor |
| RNF-07 | Mantenibilidad | Media | ✅ Hecho | Scripts modulares, GDD, esta doc |
| RNF-08 | ACID / integridad datos | Alta | ❌ Pendiente | Aplica a capa API/BD |
| RNF-09 | Respaldos PostgreSQL | Alta | ❌ Pendiente | — |
| RNF-10 | Escalabilidad OLTP | Media | ❌ Pendiente | — |
| RNF-11 | Compatibilidad tecnológica | Alta | ✅ Hecho | Unity 6 + URP + primitivas; PostgreSQL planificado |
| RNF-12 | Claridad isomorfismo | Alta | ✅ Hecho | HUD y mensajes usan términos del isomorfismo |
| RNF-13 | Trazabilidad académica | Media | ❌ Pendiente | Sin auditoría `EventoGC` persistente |
| RNF-14 | Despliegue laboratorio | Media | 🟡 Parcial | README + build; falta guía API/BD |

**Resumen RNF:** 5 ✅ · 4 🟡 · 5 ❌ (de 14)

---

## Entidades de persistencia (objetivo académico)

| Entidad | Rol | En Unity actual |
|---|---|---|
| `Jugador` | Estudiante autenticado | — |
| `Sesion` | Partida | `GameSession` (local) |
| `Nivel` | Catálogo de niveles | Zonas hardcodeadas en escena |
| `ProgresoSesion` | integridad, cpu, ram_usada | Stats en runtime del player |
| `TipoBug` | Catálogo de tipos de bug | `LeakVariant` enum |
| `InstanciaBug` | Bug en partida | `MemoryLeakEnemy` en escena |
| `EventoGC` | Traza de cada GC | — |

---

## Prioridades sugeridas (siguiente sprint)

1. **Barra RAM en HUD** (RF-07, RF-15) — derivar de fragmentos activos / leaks vivos.
2. **Capa API mínima** + esquema PostgreSQL (RF-17, RF-18, RNF-08).
3. **Pantalla de identificación** (RF-01, RF-03).
4. **Selección de nivel** (RF-13) — segundo escenario o dificultad desde BD.

---

## Trazabilidad demo ↔ zonas pedagógicas

| Zona demo | Objetivo pedagógico (GDD) | RF relacionados |
|---|---|---|
| Boot Sector | Controles, CPU, GC, primer leak | RF-04, RF-05, RF-06, RF-10, RNF-01, RNF-12 |
| Heap Corrupto | Tradeoff depurar vs GC | RF-09, RF-10, RF-14 |
| Kernel Core | Boss, presión bajo ENRAGE | RF-11, RF-12, RF-08 |

---

*Documento generado para el proyecto NullBorn — Tecnicas de Modelamiento de Software, UNIMINUTO Villavicencio.*
