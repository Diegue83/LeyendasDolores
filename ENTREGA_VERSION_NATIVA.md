# Leyendas de Dolores Hidalgo · Versión nativa y editable

Proyecto: `C:\Users\52418\LeyendasDolores_Nativo` (Unity 6000.6.1f1)
Original intacto: `C:\Users\52418\LeyendasDoloresv4` (no se modificó nada durante la migración)

## 1. Qué cambió (y qué no)

El juego es el mismo: las mismas 15 vistas, 20 acertijos y soluciones, 94 líneas de diálogo, objetos, combinaciones, sonidos,
transiciones, puntuación por nivel e interfaz. Lo que cambió es **cómo se guarda el contenido**:

| Antes (original) | Ahora (nativa) |
|---|---|
| El menú *Construir* armaba la escena por código; al dar Play, `Arranque` y `Juego.Awake` **reconstruían** lo que faltaba. | Todo está guardado en escenas `.unity` y prefabs. Al jugar **no se construye nada**. |
| Una sola escena con las 15 vistas. | `Juego.unity` (interfaz y gestor) + una escena por nivel con sus vistas. Al jugar se cargan de forma aditiva. |
| Los 20 acertijos se dibujaban por código cada vez que se abrían. | Cada acertijo es un **prefab editable**; la lógica se engancha a sus piezas y respeta lo que edites (posición, tamaño, sprite, color). |
| `Vista`, `Capa`, `ZonaInteractiva`, `Paralaje` y las animaciones estaban en archivos con otro nombre: **Unity no los guardaba** en la escena (quedaban como "script faltante") y el juego dependía de reconstruirlas. | Cada componente está en su propio archivo con su nombre: se guardan en escenas y prefabs. |

## 2. Estructura

```
Assets/
  Escenas/
    Juego.unity                  ← ábrela para jugar (cámara, interfaz, gestor, EventSystem)
    Niveles/
      N1_Parroquia.unity         ← atrio, sacristía, nave, torre, campanario
      N2_Plaza.unity             ← plaza, kiosko, nevería, monumento
      N3_Mercado.unity           ← pasillo, talavera, bodega
      N4_CasaHidalgo.unity       ← sala, biblioteca, patio
  Resources/
    Prefabs/Acertijos/Acertijo_<id>.prefab   ← 20 acertijos editables
    Arte/ (Escenas, Capas, Acertijos, Iconos, Objetos, Retratos, UI)  ← todos los gráficos como assets
    Sonido/, Fuentes/, DatosJuego.asset (textos, códigos, pistas, tiempos)
  Scripts/
    Leyendas.asmdef              ← lógica del juego (Juego, Niveles, Acertijos, Logica, Datos, Base)
    Editor/Leyendas.Editor.asmdef← menú Leyendas, importador de arte
    Editor/Migracion/            ← HERRAMIENTA TEMPORAL (Constructor + MigracionNativa). Se puede borrar.
  Tests/
    EditMode/  PlayMode/         ← pruebas automáticas (Window > General > Test Runner)
FuentesArte/                     ← generador del ARTE (fuera de Assets; solo para crear imágenes nuevas)
```

### Cómo editar
- **Una vista:** abre `Assets/Escenas/Niveles/N2_Plaza.unity` (por ejemplo). En el Canvas `Nivel 2 · Plaza Principal > Pantalla > Escenario > Vistas`
  cada vista tiene `PlanosAtras`, `Fondo`, `Capas`, `Zonas`, `Objetos` y `PlanosDelante`. Solo la primera vista de cada nivel está activa:
  activa la que quieras editar con la casilla del Inspector (al jugar, el juego las administra solo).
- **Ver una vista con la interfaz:** abre `Juego.unity` y arrastra la escena de nivel a la Hierarchy (carga aditiva).
- **Un acertijo:** doble clic en `Assets/Resources/Prefabs/Acertijos/Acertijo_<id>.prefab`. Mueve, escala o cambia sprites y colores.
  Para ocultar una pieza, **desactívala** (no la borres: la lógica la busca por su lugar en la lista `PlantillaUI`).
- **Textos, códigos, pistas, tiempos:** `Assets/Resources/DatosJuego.asset`.
- **Jugar:** abre `Juego.unity` y Play.

## 3. Inventario de lo migrado

| Escena | Vistas | Elementos (cada uno su GameObject) | Estados | Recortes / objetos | Zonas | Planos de profundidad |
|---|---|---|---|---|---|---|
| N1_Parroquia | n1_atrio, n1_sacristia, n1_nave, n1_torre, n1_campanario | 43 | 9 | 11 | 18 | 1 (campanario) |
| N2_Plaza | n2_plaza, n2_kiosko, n2_neveria, n2_estatua | 58 | 5 | 6 | 12 | 5 (plaza 3, kiosko 1, monumento 1) |
| N3_Mercado | n3_pasillo, n3_talavera, n3_bodega | 25 | 2 | 8 | 10 | 1 |
| N4_CasaHidalgo | n4_sala, n4_biblioteca, n4_patio | 26 | 5 | 4 | 13 | 2 |

**Todos los niveles van por elementos:** el fondo de cada vista es solo el cascarón (cielo, muros, piso) y cada torre, puerta, árbol,
farol, puesto, mueble o decoración es su propio GameObject dentro de `Capas`, con su imagen y su posición. Los **estados** (puerta
abierta, campanas encendidas, placa resuelta...) son hijos del elemento al que pertenecen. Los elementos de un plano de profundidad
están dentro de su contenedor `Plano_<nombre>`. El atrio conserva exactamente su diseño: se partió en 15 piezas sin redibujarlo.

- **Juego.unity:** barra superior (título, reloj del nivel y puntos, progreso, piezas, Mapa/Diario/Pista/Sonido), marco del escenario,
  diálogo con retrato, mochila de 8 ranuras, ventana de acertijos con resultado, tarjetas (mapa, diario, fin de nivel, final), portada, cámara, EventSystem.
- **20 prefabs de acertijos:** candado, deslizante, anillos, candelabro, campanas, nieves, nonograma, jarras, cronologia, criptex,
  canal, pinata, balanza, hanoi, simbolos, cesar, libros, caja, ruta, final (entre 8 y 67 piezas cada uno).
- **Datos:** 20 acertijos con soluciones, 94 líneas de diálogo (80 claves), 24 objetos, 3 combinaciones, 4 niveles, 15 vistas.
- **Arte:** 21 imágenes de escena/planos, 47 capas, 59 de acertijos, 24 objetos, 22 íconos, 9 retratos, 10 de interfaz, portada. **Sonido:** 14.
- **Lógica sin cambios:** Niveles (qué pasa al tocar cada cosa), Logica (reglas de los acertijos), diálogo, mochila, guardado, puntuación.

## 4. Pruebas realizadas (todas pasan)

| Prueba | Qué comprueba | Resultado |
|---|---|---|
| EditMode · EscenasDeNivel_TienenTodoElContenidoOriginal | Cada vista, capa, zona y plano del original existe en su escena, con imagen | ✔ |
| EditMode · EscenaDelJuego_TieneSuInterfazGuardada | Interfaz completa, 8 ranuras, sin vistas sueltas, Build Profiles en orden | ✔ |
| EditMode · PrefabsDeAcertijos_Existen | Los 20 prefabs con sus piezas, sin piezas rotas | ✔ |
| EditMode · CambioManual_PersisteAlGuardarYReabrir | Mover una pieza, guardar, cerrar y reabrir la escena | ✔ |
| EditMode · ElJuegoYaNoIncluyeElGenerador | El Constructor no está en el código del juego | ✔ |
| **Reinicio de Unity** (dos ejecuciones separadas) | Cambio en escena y en prefab, se cierra Unity, se vuelve a abrir: el cambio sigue | ✔ escena y prefab |
| PlayMode · **RecorridoCompleto_DeLaPortadaAlFinal** | De la portada al final de los 4 niveles tocando las zonas reales: 45 toques en zonas (15 de ellos para recoger objetos), 4 combinaciones y la cajita, navegación con puertas y flecha de regreso, las 4 piezas, tarjetas de fin de nivel y tarjeta final. 5 acertijos se resuelven con sus **botones reales del prefab** (candado 1609, criptex ALDAMA, César clave 4, ruta, campana); los otros 15 con su lógica | ✔ (60 s) |
| PlayMode · TodasLasZonas_SeAlcanzanEnSuVista | En las 15 vistas, nada tapa ninguna zona tocable (regla de construcción) | ✔ |
| PlayMode · CadaAcertijo_SeArmaDesdeSuPrefab | Los 20 se arman desde su prefab, sin piezas extra, y cada botón tiene acción | ✔ |
| PlayMode · PlanosDeProfundidad_NoBloqueanToques_YSeMueven | Los planos no bloquean toques y se mueven con el paralaje | ✔ |
| PlayMode · AcertijosRediseñados_SeResuelvenConSusPiezas | Candelabro (flama por los brazos, con un error a propósito), campanas (partitura), anillos en ciclo y papel picado se resuelven con sus piezas reales y se cierran solos, sin pantalla de resultado | ✔ |
| PlayMode · Mapa_MuestraLosLugares_PausaElReloj_YViaja | 4 marcadores, el reloj se detiene con el mapa abierto, un lugar bloqueado no deja viajar y uno abierto sí | ✔ |
| PlayMode · Dialogo_SeReemplazaAlTocarOtroLugar | Con texto escribiéndose y otro en espera, tocar la placa cambia el diálogo al instante | ✔ |
| PlayMode · Menu_PartidasPausaReiniciarAjustesYCreditos | Nueva partida en la ranura 2, pausa (detiene el reloj), reiniciar el lugar con confirmación, ajustes (volumen y velocidad del texto), menú principal, créditos y continuar la partida guardada | ✔ |
| PlayMode · (recorrido y acertijos) El caso | Las 4 evidencias se consiguen en el recorrido; acusar a Don Tomás es error y dice qué evidencia no encaja; Doña Lupita resuelve el caso y sigue el armado de la campana | ✔ |

Cómo repetirlas: Window > General > Test Runner (EditMode / PlayMode) > Run All.
Nota: las pruebas borran la partida guardada de este proyecto (PlayerPrefs).

## 5. Diferencias y observaciones

1. **Escenas por nivel:** antes todo estaba en una escena; ahora cada nivel tiene la suya y al jugar se cargan de forma aditiva.
   En pantalla el resultado es idéntico (las vistas se pasan al mismo escenario del juego).
2. **Acertijos con cantidad de piezas fija:** si en `DatosJuego` cambias cuántos elementos tiene un acertijo (por ejemplo más sabores
   o más libros), hay que añadir esas piezas al prefab (o volver a usar la migración, que **reemplaza** escenas y prefabs). El juego avisa
   en la Console si la lógica pide más o menos piezas de las que tiene el prefab.
3. **Se quitaron los menús** *Construir proyecto editable* y *Reconstruir solo las vistas*: sobrescribían las escenas.
   Queda *Leyendas > Migración* (pide confirmación). La carpeta `Scripts/Editor/Migracion` se puede borrar sin afectar el juego.
4. **Hallazgo en el original:** `Vista`, `Capa`, `ZonaInteractiva`, `Paralaje`, `Pulsar`, `Vaiven`, `Brillo`, etc. no se guardaban en la
   escena (archivo con otro nombre). El original funciona porque los reconstruye al dar Play, pero **cualquier ajuste manual a esas piezas
   en su escena se perdía**. En la versión nativa ya está corregido.
5. **Pantallas 4:3 (iPad):** con el escalado actual (1280×720, mezcla 0.5) la interfaz se recorta ~85 puntos por lado y las puertas del
   borde izquierdo (nave, nevería, bodega, biblioteca) quedan fuera. Pasa igual en el original. En teléfonos horizontales (16:9 o más) se ve completa.
6. **Diálogo:** si el jugador actúa muy rápido, las líneas se encolan (en la prueba llegaron a 22) y el texto va detrás de la acción.
   El motor de diálogo es idéntico al original; solo se documenta.
7. **Arte:** las imágenes son assets normales en `Assets/Resources/Arte`. `FuentesArte/` (fuera de Assets) es el generador para crear arte
   nuevo; no se necesita para abrir, editar ni jugar.
8. **Luz de la torre (n1_torre):** la oscuridad (`penumbra`) y la luz cálida (`luz_calida`, `escalera_luz`) son elementos propios;
   `Niveles.cs` muestra uno u otro según si ya se encendió el candelabro. Antes era una sola imagen de toda la vista encendida.
9. **Viñeta:** el oscurecido de las orillas de cada vista de los niveles 1 y 2 es el elemento `vineta` (el último de `Capas`); se puede
   ajustar o apagar. No recibe toques.
10. **Definiciones de ensamblado** (`Leyendas.asmdef`, `Leyendas.Editor.asmdef`): necesarias para las pruebas; compilan más rápido.

## 6. Pendiente / sugerido
- Revisar en el Editor, a ojo, cada escena de nivel (las pruebas validan contenido, posiciones guardadas y toques, no la estética).
- Decidir si se ajusta el escalado para 4:3.


## 7. Versión 8 · acertijos, pistas, mapa y diálogo

**Cómo se aplicó:** menú *Leyendas > Migración > Aplicar v8* (o `-executeMethod Leyendas.Editor.ActualizacionV8.AplicarDesdeConsola`):
datos v8, ventana del Mapa en `Juego.unity` y prefabs nuevos de candelabro, campanas y papel picado.

- **Sin valoración al terminar cada acertijo.** Ya no aparece la pantalla de resultado ni el confeti: el acertijo se cierra solo y sigue la
  historia. Los errores y pistas de cada acertijo se guardan (`Estado.acertijos`) y se muestran, junto con tiempo y estrellas, al
  terminar el nivel. El Diario ya no muestra errores ni pistas a mitad de nivel.
- **Diálogo inmediato.** Tocar un lugar u objeto reemplaza al instante lo que se estaba escribiendo y lo que esperaba. No se corta una
  secuencia que tiene una acción al terminar (abrir un acertijo, tarjeta de fin de nivel).
- **Acertijos rediseñados (coherencia):**

  | Acertijo | Antes | Ahora |
  |---|---|---|
  | Candelabro | Tocar una vela cambiaba a sus vecinas (sin sentido) | La flama pasa de la última vela encendida a otra unida por un brazo de bronce; un solo recorrido (verificado), deducible por las velas de un solo brazo |
  | Campanas | Simón dice; la partitura no se usaba | Se toca la melodía de la partitura; hay que deducir que la campana grande es grave y la chica aguda |
  | Papel picado | 5×5 con forma de cruz (el diálogo decía «campana») | 7×7 con forma de campana; solución única por lógica de filas y columnas (verificado) |
  | Criptex | Cada ficha decía su letra y su lugar | Las fichas solo dicen la letra; el apellido se deduce de los hechos de la plaza |

- **Más difíciles:** anillos engranados en ciclo (ningún anillo se mueve solo); nieves con 5 pistas mínimas de solución única y sin
  «3 de 5 bien»; libros con años en números romanos; placa del atrio con dos fechas; símbolos de la bodega revueltos y sin números
  (el orden lo da la tapa); cronología y libros sin «X en su lugar».
- **Pistas:** las 20 reescritas. La primera orienta y la segunda enseña una técnica; ninguna da la respuesta. Las notas del Diario
  tampoco revelan soluciones.
- **Mapa:** ventana propia (*Pantalla > Mapa*): ilustración del centro de Dolores (`UI/mapa_dolores`), un marcador por lugar con su
  estado (listo con estrellas, «Estás aquí», abierto, bloqueado), ficha del lugar y botón para viajar. El reloj se detiene mientras está
  abierto. La Parroquia frente al Jardín y la Casa de Hidalgo en Morelos e Hidalgo son referencias reales; el Mercado Hidalgo está en
  una ubicación aproximada.
- **Textos de los prefabs de acertijos:** la apariencia (fuente, tamaño, color, lugar) se edita en el prefab; el **contenido** del texto
  sale de `DatosJuego` (antes el prefab conservaba el texto viejo y no se veían los cambios de pistas o enunciados).
- Respaldo del arte anterior: `C:\Users\52418\LeyendasDolores_Nativo_respaldo_v8`.


## 8. Versión 9 · menú, partidas y el caso de la campana robada

**Cómo se aplicó:** menú *Leyendas > Migración > Aplicar v9* (o `-executeMethod Leyendas.Editor.ActualizacionV9.AplicarDesdeConsola`).

- **Menú principal** (sobre la portada): Continuar/Jugar, Partidas, Ajustes, Créditos y Salir (Salir solo en computadora).
  Debajo de Continuar se ve la partida en uso.
- **Partidas:** 3 ranuras. Cada una muestra lugar, piezas, tiempo, estrellas y fecha de guardado, con Continuar, Nueva partida /
  Empezar de nuevo (pide confirmación) y Borrar. La partida que ya existía quedó en la ranura 1.
- **Botón «Menú»** en la barra (en lugar del de sonido): pausa con Continuar, **Reiniciar este lugar**, Ajustes y Menú principal.
  La pausa detiene el reloj. Reiniciar vuelve a la foto que se toma al entrar por primera vez a un lugar (objetos, acertijos,
  tiempo, errores y pistas como al llegar); un lugar ya terminado no se reinicia. Las partidas guardadas antes de la v9 no tienen
  esa foto en el lugar en curso: ahí el botón dice «No se puede reiniciar» hasta pasar al siguiente lugar.
- **Ajustes** (valen para las 3 partidas): volumen de música, volumen de efectos, velocidad del texto y pantalla completa.
- **Créditos:** el texto está en `DatosJuego > Créditos`. **Falta poner el nombre del autor** (hay un texto que lo indica).
- **Objetivo nuevo: la campana robada.** El Campanero se roba la campana y deja notas; Ana busca las 4 piezas y 4 evidencias
  (llave de la torre, huella chica, hilo azul, carta zurda). Sospechosos: Don Tomás (sacristán, personaje nuevo), Don Chuy,
  Doña Lupita y Don Ramiro (el alfarero). En la vitrina se resuelve primero el **tablero del caso** (acertijo `caso`), luego la
  confesión y al final el armado de la campana. El Diario lleva la sección «El caso de la campana robada».
  Los escenarios no cambiaron; solo diálogos, notas, el retrato de Don Tomás y el acertijo del caso.
- **Correcciones de escenarios:**
  - Las puertas laterales abiertas ya no tapan lo del muro del fondo: su hoja queda pegada a su muro (nevería, bodega,
    biblioteca, sacristía y nave).
  - La puerta de la sacristía hacia la nave está ahora a la **izquierda**, y la de la nave hacia la sacristía a la **derecha**
    (la sacristía está a la derecha de la Parroquia). En la nave, las bancas quedan delante de la puerta.
  - La piñata del pasillo cuelga más alto y ya no tapa la puerta ni el letrero de la bodega.
- Respaldo del arte anterior: `C:\Users\52418\LeyendasDolores_Nativo_respaldo_v9`.
