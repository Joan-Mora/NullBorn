# Política de seguridad

NullBorn es un **proyecto académico de código abierto** (UNIMINUTO — Tecnología en Desarrollo de Software). Esta política describe cómo reportar vulnerabilidades y qué expectativas de seguridad aplican al prototipo.

## Alcance

| En alcance | Fuera de alcance |
|---|---|
| Repositorio Unity del cliente | Infraestructura de producción no incluida en el repo |
| Scripts C# del juego y herramientas editor | Servicios de terceros (Unity Hub, GitHub) |
| Documentación y builds de demostración | Ataques que requieran acceso físico al laboratorio |

## Versiones soportadas

Solo la rama principal y el último tag de demo (`Demo_10min`) reciben correcciones de seguridad de forma best-effort académica. No hay SLA comercial.

## Reportar una vulnerabilidad

1. **No** abras un issue público con detalles explotables.
2. Envía un correo a los mantenedores del repositorio (GitHub: [@Joan-Mora](https://github.com/Joan-Mora), [@MrRoar154](https://github.com/MrRoar154)) con:
   - Descripción del problema
   - Pasos para reproducir
   - Impacto estimado
   - Versión/commit afectado
3. Espera confirmación en un plazo razonable (objetivo: 5 días hábiles en periodo de clase).
4. Coordinaremos un fix y publicaremos un aviso en el CHANGELOG cuando corresponda.

## Prácticas del proyecto

### Datos sensibles
- **No** subas contraseñas, tokens, cadenas de conexión PostgreSQL ni correos reales de estudiantes al repositorio.
- Usa archivos `.env` locales (listados en `.gitignore`) para credenciales de laboratorio.
- El correo institucional del estudiante **no** debe mostrarse completo en el HUD (RF-02, RNF-04).

### Cliente Unity
- El build de Windows es un ejecutable de demostración; ejecútalo solo en entornos de confianza (laboratorio UNIMINUTO).
- No habilites permisos de red innecesarios en el prototipo.

### API y base de datos (roadmap)
- La persistencia planificada (Unity → API → PostgreSQL) debe usar HTTPS en despliegues reales, contraseñas hasheadas y consultas parametrizadas.
- Los eventos `EventoGC` de sesiones evaluadas no deben eliminarse (RNF-13).

### Dependencias
- Mantén Unity y paquetes URP actualizados dentro de la versión del curso.
- Revisa advisories de GitHub Dependabot cuando se habilite CI.

## Divulgación responsable

Agradecemos la divulgación responsable. Los reportes válidos se acreditarán en la documentación del proyecto si el reportero lo desea.

## Contacto

- **Repositorio:** issues generales (no seguridad) vía GitHub Issues
- **Seguridad:** contacto directo a mantenedores vía GitHub
