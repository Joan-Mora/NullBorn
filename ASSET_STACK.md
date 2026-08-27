# NullBorn — Asset Stack (reset 2026)

Proyecto reiniciado desde cero. Este archivo es la lista **aprobada** de assets a descargar/importar antes de reconstruir el juego (Unity 6 + URP, tono aldea/naturaleza stylized).

Cuando los tengas importados, escribe en el chat: **assets importados**.

---

## Descargar / comprar (sí)

### Mundo y personajes
| # | Asset | Uso |
|---|--------|-----|
| 1 | **Stylized Nature MegaKit** (July 2024) | Bosques, plantas, rocas, biomas |
| 2 | **Medieval Village MegaKit** (January 2025) | Aldea modular / hub |
| 3 | **Modular Character Outfits – Fantasy** (Nov 2025) | Ropa modular del héroe |
| 3b | **Universal Base Characters** (si el pack de outfits lo exige aparte) | Mesh base humanoid |
| 4 | **Universal Animation Library** (March 2025) | Locomotion, combate base, emotes |
| 5 | **Universal Animation Library 2** (January 2026) | Combos melee, parkour, extras |
| 6 | **30 Monster Stylized Fantasy Vol 01** (PixeliusVita) | Enemigos con rig + anim |
| 7 | **Ultimate Animated Animal Pack** (July 2021) | Fauna / vida en el mapa |
| 8 | **Furniture** (stylized low-poly, ~840 props) | Interiores de casas / tienda |
| 9 | **STYLIZED Fantasy Armory** | Armas, escudos, forja, racks |
| 10 | **5 Fantasy Skyboxes** | Cielo |

### Agua / shaders URP
| # | Asset | Uso |
|---|--------|-----|
| 11 | **URP Shaders Collection for Unity** | Agua stylized, fog, toon, extras |

### VFX (URP / gratis)
| # | Asset | Uso |
|---|--------|-----|
| 12 | **Free Fire VFX – Unity URP** | Fuego ambiental / combate |
| 13 | **Zap VFX – Unity URP** | Rayos / hits / “corrupción” |
| 14 | **Free Highlight Shader for Unity URP** | Outline interactuables / objetivo |

### Audio
| # | Asset | Uso |
|---|--------|-----|
| 15 | **610+ Tracks Mega Music Bundle [No AI]** | Música + SFX ambiente |
| 16 | **Medieval Weapons SFX** | Combate (espada, hits, whoosh) |
| 17 | **Medieval Armor SFX** | Pasos / armadura |

### UI
| # | Asset | Uso |
|---|--------|-----|
| 18 | **UI User Interface MEGA PACK** | HUD / menús (incluye Medieval) |

---

## No usar (explícitamente descartados)

- VFX EffectBlocks (**Godot**)
- Water Infinite Ocean (**Godot**)
- Trails VFX (**Built-in**, no URP) → usar `TrailRenderer` nativo
- Cozy Tunes (redundante con Mega Music Bundle)
- Sword Combat SFX Free (redundante con Medieval Weapons)
- Medieval Sounds (redundante)
- UI Medieval 32×32 y UI Medieval Mega (redundantes con UI MEGA PACK)
- FREE Low Poly Weapon Pack / Medieval Weapons 15 items (sustituidos por Fantasy Armory)

---

## Orden de importación sugerido

1. Nature + Village + Skyboxes  
2. Character Base + Outfits + Anim Library 1 (+ 2)  
3. Armory + Furniture  
4. Monsters + Animals  
5. URP Shaders + Fire/Zap/Highlight  
6. Music Bundle + Weapons/Armor SFX  
7. UI MEGA PACK  

## Notas

- Motor: **Unity 6000.x + URP**, Windows PC.  
- Nadar = gameplay (trigger + anim), no requiere ocean pack.  
- Armas = props estáticos parentados a la mano; animación = Universal Anim Library.  
- Carpeta sugerida al importar: `Assets/ThirdParty/<PackName>/`.
