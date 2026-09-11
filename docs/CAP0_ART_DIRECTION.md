# NullBorn — Cap 0 Fantasy + 4 mundos (dirección de arte)

**Fecha:** 2026-09-10  
**Estado:** borrador de trabajo para prompts Cursor / Unity  
**Repo:** Joan-Mora/NullBorn

## Identidad en una frase

Action-RPG educativo tipo Split Fiction: empiezas en fantasy estable; cada falla de código te tira a otro género; al final entras al Runtime y parchás bugs. Las habilidades se ganan con mini-parches de código (sem 1–2), no con quizzes que reemplazan el combate.

## Los 4 mundos (lock propuesto)

| # | Mundo | Género / look | Concepto código | Habilidad que desbloquea |
|---|--------|---------------|-----------------|--------------------------|
| 1 | **Reino de Arranque** | Fantasy stylized aldea/naturaleza (packs actuales) | Variables + tipos | Sprint / buffer de movimiento |
| 2 | **Protocolo de Fuego** | Shooter 3ª persona stylized (contraste neón vs humo) | `if` / `else` | Aim assist lite / dodge condicional (si telegraph → i-frames) |
| 3 | **Bucle Vertical** | Plataformero 3D chunky (formas grandes, poca textura ruido) | `for` / `while` | Multi-salto o multi-hit (iterar N veces) |
| 4 | **Build Rota** | Indie **glitch-papel** (collage UI, tipografía mono, recortes) — no “indie genérico” | Funciones + return | Onda de Purga / llamar función limpia zona |

**Por qué “Build Rota” en vez de solo “Indie”:** Indie no es un estilo; glitch-papel sí se dirige, se promptéa y se diferencia del fantasy/shooter/platformer.

**Runtime (epílogo, no cuenta como 5º mundo de producción MVP):** cian sano / magenta bug, nodos y stacks. Se construye después de que el loop jugar→romper→parche→habilidad funcione una vez.

## Cap 0 Fantasy — reglas de arte (unificar YA)

### Outline (decisión)

- **Entorno Cap 0: SIN outline comic negro** en meshes de Nature / Village / props.
- **Outline solo en interactuables** (loot, NPC, enemigo, puerta) vía Free Highlight Shader URP del stack — rim fino, no trazo manga.
- Motivo: en ArtTest conviven pino sin outline + árbol/arbusto con outline grueso → se ve “packs pegados”. El bulk de Nature/Village MegaKit es soft stylized sin trazo; pelear por outline global = pelear contra los packs.

### Personaje

- Mismo mesh base en los 4 mundos.
- Cap 0: **outfit fantasy** del pack Modular Character Outfits (no ropa interior en builds de playtest).
- Sin cambio de silueta entre mundos; cambian outfit + VFX + post (color grade / shader).

### Paleta Cap 0 (roles, hex tentativos a validar en escena)

| Rol | Hex | Uso |
|-----|-----|-----|
| Hierba | `#4F7A3E` | suelo / follaje base |
| Hoja clara | `#7CAF4A` | highlights vegetación |
| Tierra | `#6B4E32` | paths, madera oscura |
| Madera | `#A67C52` | carts, casas |
| Cielo | `#A8C8E8` | sky / niebla lejana |
| Acento héroe | `#3D6B8C` | tela / detalle outfit (legible contra verde) |
| UI sano | `#5EC8C0` | tip cian suave HUD |
| Reserva bug (aún no dominante) | `#C84B8A` | solo glitches / foreshadow |

### Material / luz

- URP stylized, no cinematic.
- Soft shadows Medium, shadow distance ~40 m (PERFORMANCE.md).
- Evitar mezclar cel-shade hard outline con PBR suave en la misma toma.
- Texturas legibles a 2–5 m; no “barro” por filtro.

