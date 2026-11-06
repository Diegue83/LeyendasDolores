# Leyendas de Dolores Hidalgo · El misterio de la campana

Aventura 2D de apuntar y tocar con acertijos de lógica, hecha en Unity. Es la madrugada del 15 de septiembre y alguien se robó la campana del Grito: Ana recorre la Parroquia, la Plaza Principal, el Mercado Hidalgo y la Casa de Hidalgo, resuelve 20 acertijos, recupera las cuatro piezas de la campana y descubre quién es *El Campanero*.

## Equipo

- **Juan Diego Juárez Cruz**
- **Zayda Fernanda Vargas Vargas**

Universidad Tecnológica del Norte de Guanajuato · Ingeniería en Desarrollo y Gestión de Software · 2026

## Contenido

- 4 niveles con escenarios ilustrados en capas y efecto de profundidad (paralaje).
- 20 acertijos (candado romano, rompecabezas deslizante, anillos del rosetón, candelabro, partitura de campanas, nieves de Don Chuy, papel picado, jarras, cronología, criptex, canal de talavera, piñata, balanza, ollas, símbolos, carta cifrada, libros, caja fuerte, ruta insurgente y el caso de El Campanero) con pistas graduales.
- Mochila con objetos que se combinan, diario de pistas y evidencias, mapa del pueblo.
- Menú principal, 3 partidas guardadas, pausa con «Reiniciar este lugar», ajustes (volumen y velocidad del texto) y créditos.
- Puntuación por lugar (tiempo, errores y pistas) con estrellas y récords.

## Cómo abrirlo

1. Instala **Unity 6000.6.1f1** desde Unity Hub (con *Android Build Support* si vas a compilar para Android).
2. En Unity Hub: **Add project from disk** y elige esta carpeta. La primera vez Unity importa el arte; tarda unos minutos.
3. Abre `Assets/Escenas/Juego.unity` y presiona **Play**.
   También se puede dar Play desde una escena de nivel (`Assets/Escenas/Niveles/N1_Parroquia.unity`, etc.): el juego empieza en ese nivel.

## Estructura

| Carpeta | Qué contiene |
|---|---|
| `Assets/Escenas` | `Juego.unity` (interfaz, menú, mapa y ventanas) y una escena por nivel con sus vistas. |
| `Assets/Resources/Arte` | Escenarios, capas, objetos, retratos e interfaz. `vistas.json` dice dónde va cada capa y cada zona. |
| `Assets/Resources/Prefabs/Acertijos` | Un prefab por acertijo con sus piezas. |
| `Assets/Resources/DatosJuego.asset` | Textos, diálogos, pistas, soluciones y créditos (se editan desde el Inspector). |
| `Assets/Scripts/Juego` | Flujo del juego: vistas, diálogo, mochila, mapa, menú, partidas y guion de los niveles. |
| `Assets/Scripts/Acertijos` | La interfaz de cada acertijo, agrupada por nivel. |
| `Assets/Scripts/Logica` | Reglas de los acertijos en C# puro. |
| `Assets/Scripts/Base` | Componentes de interfaz, animaciones y sonido. |
| `Assets/Scripts/Editor` | Menú *Leyendas* (datos, escenas y compilación). |
| `Assets/Tests` | Pruebas automáticas (EditMode y PlayMode). |

Más detalle técnico en [`ENTREGA_VERSION_NATIVA.md`](ENTREGA_VERSION_NATIVA.md).

## Compilar

- **PC (Windows):** menú *Leyendas > Compilar > PC (Windows)*.
- **Android (APK):** menú *Leyendas > Compilar > Android (APK)*.

Las compilaciones se guardan fuera del proyecto, en `C:/Users/<usuario>/LeyendasDolores_Builds` (se cambia en `Assets/Scripts/Editor/Compilar.cs`).

## Pruebas

*Window > General > Test Runner* → **EditMode** y **PlayMode** → *Run All*. Incluyen el recorrido completo del juego, de la portada al final, tocando las zonas reales.
Nota: las pruebas borran las partidas guardadas en esa computadora.

## Créditos y herramientas

- Hecho con Unity.
- El arte, los textos, el código y los sonidos se hicieron con ayuda de Claude (Anthropic).
- Tipografías: Lilita One (Juan Montoreano) y Atkinson Hyperlegible (Braille Institute), con licencia SIL Open Font License.
- Gracias a Dolores Hidalgo, Cuna de la Independencia Nacional, Guanajuato.

## Sobre el historial de este repositorio

El juego se desarrolló antes de usar git. El historial de commits se reconstruyó después a partir de ese desarrollo, por etapas (base, cada nivel, pruebas y documentación), con los autores y las fechas aproximadas de cada etapa. Los archivos que comparten todos los niveles (datos, diálogos, lógica y la escena principal) aparecen en su versión final desde el primer commit.
