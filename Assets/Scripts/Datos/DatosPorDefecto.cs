// Valores iniciales de DatosJuego. El menú "Leyendas > 1. Construir proyecto editable" los copia al asset
// Assets/Resources/DatosJuego.asset; a partir de ahí se editan en el Inspector.
using System.Collections.Generic;

namespace Leyendas
{
    public static class DatosPorDefecto
    {
        public static void Llenar(DatosJuego d)
        {
            d.niveles = Niveles();
            d.vistas = Vistas();
            d.acertijos = Acertijos();
            d.objetos = Objetos();
            d.combinaciones = Combinaciones();
            d.dialogos = Dialogos();
            d.version = DatosJuego.VERSION;
        }

        static List<NivelDatos> Niveles()
        {
            return new List<NivelDatos>
            {
                new NivelDatos { id = "n1", nombre = "Parroquia de Nuestra Señora de los Dolores", pieza = "corona", nombrePieza = "la corona", vistaInicial = "n1_atrio", tiempoPar = 900,
                    objetivo = "Alguien se robó la campana del Grito. Encuentra la CORONA y busca pistas de El Campanero. La placa del atrio da la primera.",
                    cierre = "La corona es la parte de arriba de la campana, donde se sujeta al yugo." },
                new NivelDatos { id = "n2", nombre = "Plaza Principal", pieza = "yugo", nombrePieza = "el yugo", vistaInicial = "n2_plaza", tiempoPar = 1020,
                    objetivo = "Encuentra el YUGO y sigue el rastro de El Campanero. Don Chuy, el nevero, sabe lo que pasa en la plaza.",
                    cierre = "El yugo es la viga de madera de la que cuelga la campana." },
                new NivelDatos { id = "n3", nombre = "Mercado Hidalgo · zona de dulces", pieza = "badajo", nombrePieza = "el badajo", vistaInicial = "n3_pasillo", tiempoPar = 900,
                    objetivo = "Encuentra el BADAJO. Doña Lupita y el alfarero necesitan ayuda... y alguien escondió algo en la piñata.",
                    cierre = "El badajo es el péndulo de hierro que golpea la campana por dentro." },
                new NivelDatos { id = "n4", nombre = "Casa de Hidalgo", pieza = "cuerpo", nombrePieza = "el cuerpo", vistaInicial = "n4_sala", tiempoPar = 780,
                    objetivo = "Encuentra el CUERPO de la campana, descubre quién es El Campanero y arma la campana en la vitrina de la sala.",
                    cierre = "El cuerpo es la parte que vibra y suena. El caso de la campana robada está resuelto." },
            };
        }

        static List<VistaDatos> Vistas()
        {
            return new List<VistaDatos>
            {
                new VistaDatos { id = "n1_atrio", nombre = "Atrio de la Parroquia", padre = "" },
                new VistaDatos { id = "n1_sacristia", nombre = "Sacristía", padre = "n1_atrio" },
                new VistaDatos { id = "n1_nave", nombre = "Nave de la Parroquia", padre = "n1_atrio" },
                new VistaDatos { id = "n1_torre", nombre = "Base de la torre", padre = "n1_atrio" },
                new VistaDatos { id = "n1_campanario", nombre = "Campanario", padre = "n1_torre" },
                new VistaDatos { id = "n2_plaza", nombre = "Plaza Principal", padre = "" },
                new VistaDatos { id = "n2_kiosko", nombre = "Dentro del kiosko", padre = "n2_plaza" },
                new VistaDatos { id = "n2_neveria", nombre = "Nevería", padre = "n2_plaza" },
                new VistaDatos { id = "n2_estatua", nombre = "Base de la estatua", padre = "n2_plaza" },
                new VistaDatos { id = "n3_pasillo", nombre = "Pasillo de dulces", padre = "" },
                new VistaDatos { id = "n3_talavera", nombre = "Puesto de talavera", padre = "n3_pasillo" },
                new VistaDatos { id = "n3_bodega", nombre = "Bodega", padre = "n3_pasillo" },
                new VistaDatos { id = "n4_sala", nombre = "Sala", padre = "" },
                new VistaDatos { id = "n4_biblioteca", nombre = "Biblioteca", padre = "n4_sala" },
                new VistaDatos { id = "n4_patio", nombre = "Patio de la morera", padre = "n4_sala" },
            };
        }

        static AcertijoDatos A(string id, string titulo, string inst, float par, string resp, string[] pistas, string[] lineas = null, string[] opciones = null, int semilla = 7)
        {
            return new AcertijoDatos { id = id, titulo = titulo, instrucciones = inst, tiempoPar = par, respuesta = resp, pistas = pistas, lineas = lineas ?? new string[0], opciones = opciones ?? new string[0], semilla = semilla };
        }

        // Pistas: la primera orienta (en qué fijarse) y la segunda enseña una técnica; ninguna dice la respuesta.
        static List<AcertijoDatos> Acertijos()
        {
            return new List<AcertijoDatos>
            {
                // ---------- Nivel 1 ----------
                A("candado", "Candado romano", "El candado pide un DÍA y un MES, con dos cifras cada uno. La placa del atrio tiene más de una fecha: ¿cuál es la del llamado?", 60, "1609",
                    new[] { "En números romanos, I = 1, V = 5, X = 10. Si un número chico va ANTES de uno grande, se resta.", "El año de la fachada no sirve aquí: el candado pide el día y el mes en que se llamó a la libertad." }),
                A("deslizante", "El cuadro de la sacristía", "Desliza las piezas hacia el hueco hasta formar la pintura. La imagen de la derecha es la guía.", 150, "",
                    new[] { "No intentes acomodar todo a la vez: termina la fila de arriba y no la vuelvas a mover.", "Las dos últimas piezas de una fila se colocan juntas: pon una en la esquina, la otra debajo, y gíralas en bloque." }, semilla: 11),
                A("anillos", "Los anillos del rosetón", "Deja las tres franjas doradas apuntando hacia arriba. Los anillos están engranados en círculo: el de afuera arrastra un paso al de en medio, el de en medio arrastra un paso al de adentro en sentido contrario, y el de adentro arrastra DOS pasos al de afuera.", 120, "",
                    new[] { "Ningún anillo se mueve solo. Antes de girar, cuenta cuántos pasos le faltan a cada franja.", "Busca combinaciones: si giras un anillo hacia un lado y su vecino hacia el otro, ¿qué anillos quedan igual?" },
                    new[] { "8", "1,-1,2" }, semilla: 5),
                A("candelabro", "El candelabro de nueve velas", "Tu vela solo alcanza para prender UNA vela. Las demás se prenden con la última que encendiste, y solo si un brazo de bronce las une. Enciende las nueve en un solo recorrido.", 90, "",
                    new[] { "Una vela con un solo brazo solo puede ser la primera o la última en encenderse.", "Una vela con dos brazos recibe la flama por uno y la pasa por el otro: los dos brazos se usan." },
                    new[] { "0-4,1-2,1-5,3-4,3-6,4-6,4-7,4-8,5-8,6-7,7-8" }, semilla: 3),
                A("campanas", "El llamado de las campanas", "La partitura del campanero dice cómo sonaban las campanas. Prueba cada campana y, cuando estés lista, toca la melodía de izquierda a derecha.", 90, "",
                    new[] { "En la partitura, las notas de arriba son agudas y las de abajo, graves.", "El tamaño de una campana cambia su sonido: escucha bien a la grande y a la chica." },
                    new[] { "1,0,2,2,1,0,1,2" }, semilla: 9),
                // ---------- Nivel 2 ----------
                A("nieves", "Las nieves de Don Chuy", "Pon un sabor en cada vaso siguiendo las pistas de Don Chuy. Toca un vaso para cambiar su sabor y luego «Servir».", 150, "0,1,2,3,4",
                    new[] { "Empieza por las pistas que hablan de extremos y de «más a la izquierda»: juntas fijan un sabor.", "Cuando dos sabores van juntos («justo a la derecha»), búscales un hueco de dos vasos seguidos." },
                    new[] { "La de camarón está en un extremo.", "La de chicharrón está justo a la derecha de la de mole.", "La de tequila está junto a la de camarón.", "La de aguacate no está junto a la de tequila.", "La de aguacate está más a la izquierda que la de camarón." },
                    new[] { "Aguacate", "Mole", "Chicharrón", "Tequila", "Camarón" }),
                A("nonograma", "El papel picado", "Recorta las casillas. Los números dicen cuántas casillas seguidas van recortadas en cada fila y en cada columna, en ese orden; entre dos grupos hay al menos una sin recortar.", 180, "",
                    new[] { "Empieza por el número más grande: si un grupo ocupa casi toda la fila, las casillas del centro van recortadas sí o sí.", "Cuando una fila ya tiene todos sus grupos, las casillas que faltan van sin recortar. Usa eso para las columnas." },
                    new[] { "..###..", ".##.##.", ".#...#.", ".#...#.", "##...##", "#######", "...#..." }),
                A("jarras", "Agua de jamaica", "Deja 4 litros en la jarra de 8 y 4 litros en la de 5. Toca una jarra y luego otra para vaciar de la primera a la segunda.", 120, "",
                    new[] { "No se puede medir «a ojo»: cada vertida termina cuando una jarra se vacía o la otra se llena.", "Fíjate en lo que SOBRA cuando una jarra llena se vacía en otra más chica." },
                    new[] { "8,5,3", "8,0,0", "4,4,-1" }),
                A("cronologia", "Septiembre de 1810", "Ordena los hechos: el primero arriba y el último abajo. Toca dos tarjetas para cambiarlas de lugar y luego «Comprobar».", 90, "",
                    new[] { "Cada hecho es causa del siguiente: nadie puede traer una noticia que todavía no se ha enviado.", "Las campanas se tocaron para reunir al pueblo; solo después salió el ejército." },
                    new[] { "Josefa Ortiz de Domínguez manda avisar que la conspiración fue descubierta.", "Juan Aldama llega a Dolores con la noticia.", "Hidalgo, Allende y Aldama deciden no esperar más.", "Las campanas de la parroquia llaman al pueblo.", "El ejército insurgente sale hacia Atotonilco." }, semilla: 4),
                A("criptex", "El criptex", "Gira los seis anillos para formar la palabra secreta. Las fichas de barro de la plaza tienen sus letras, pero revueltas.", 90, "ALDAMA",
                    new[] { "Las seis letras forman un apellido que ya leíste hoy en la plaza.", "Piensa en quién llevó la noticia a Dolores." }),
                // ---------- Nivel 3 ----------
                A("canal", "El canal de talavera", "Toca los azulejos para girarlos y lleva el agua desde la fuente (izquierda) hasta la jarra (derecha).", 120, "1,2",
                    new[] { "Trabaja de los dos lados: desde la fuente hacia adelante y desde la jarra hacia atrás.", "Un azulejo recto solo tiene dos posiciones útiles; las curvas, cuatro. Decide primero los rectos." },
                    new[] { "TLLL", "-L|T", "L+L-", "-T.L" }, semilla: 4),
                A("pinata", "¡Dale, dale, dale!", "Pega cuando la piñata pase por la franja dorada. Con tres golpes se rompe... ¡pero cada vez va más rápido!", 40, "3",
                    new[] { "Mira el ritmo de dos o tres vueltas antes de pegar." }),
                A("balanza", "La cocada hueca", "Una de las 9 cocadas está hueca y pesa menos. Toca una cocada para pasarla al plato izquierdo, al derecho o regresarla. Puedes pesar 2 veces; luego toca «Elegir».", 150, "",
                    new[] { "Cada pesada tiene tres resultados posibles. Aprovecha los tres: no pongas todas las cocadas en los platos.", "Si la balanza se equilibra, la hueca no está en ningún plato." }, semilla: 13),
                A("hanoi", "Las ollas del abuelo", "Pasa las cuatro ollas al poste de la derecha. Toca un poste para tomar la olla de arriba y otro para dejarla. Nunca pongas una olla grande sobre una chica.", 150, "4",
                    new[] { "Para mover la olla más grande, las otras tres tienen que estar juntas en un solo poste.", "Mover tres ollas es lo mismo que mover dos, mover la tercera y volver a poner las dos encima." }),
                A("simbolos", "El candado de símbolos", "Gira las tres ruedas de la cajita. La tapa tiene grabado: «Del día a la noche, y de la noche a quien la acompaña».", 60, "sol,luna,estrella",
                    new[] { "Los dibujos que van en el candado son los que aparecieron en el piso de la bodega.", "La frase de la tapa recorre un día completo: empieza por la mañana." },
                    null, new[] { "sol", "luna", "estrella", "corazon", "flor", "gota" }),
                // ---------- Nivel 4 ----------
                A("cesar", "La carta cifrada", "Gira la rueda de adentro. Cada letra de la carta (anillo de afuera) se lee en la letra de adentro que queda debajo. Busca la clave con la que el mensaje tiene sentido.", 120, "4",
                    new[] { "La carta dice cuánto girar, pero con números romanos y con una operación.", "Prueba con la primera palabra corta del mensaje: si se lee bien, toda la carta se leerá bien." },
                    new[] { "LOS LIBROS GUARDAN LA COMBINACION ORDENALOS POR SU AÑO" }),
                A("libros", "Los libros de Hidalgo", "Ordena los libros por su año, del más antiguo (izquierda) al más reciente (derecha). Los años están en números romanos. Toca dos libros para cambiarlos y luego «Comprobar».", 120, "150918",
                    new[] { "Todos empiezan con MDCC (1700): compara lo que viene después.", "C = 100, L = 50, X = 10, V = 5, I = 1. Una letra chica antes de una grande se resta." },
                    new[] { "MDCCLIII · Nace en Corralejo", "MDCCLXV · Estudia en Valladolid", "MDCCLXXVIII · Se ordena sacerdote", "MDCCXC · Rector de San Nicolás", "MDCCCIII · Llega a Dolores", "MDCCCX · El Grito" }, semilla: 8),
                A("caja", "La caja fuerte", "Gira el disco y marca tres números: a la DERECHA, a la IZQUIERDA y otra vez a la DERECHA.", 90, "15,9,18",
                    new[] { "Los seis números dorados de los libros forman tres números de dos cifras.", "Cada vez que marcas, el disco debe girar hacia el otro lado para el siguiente número." },
                    new[] { "40", "1,-1,1" }),
                A("ruta", "La ruta insurgente", "Toca los pueblos en el orden en que pasó el ejército insurgente después del Grito, hasta llegar a la Alhóndiga. Empieza en Dolores.", 90, "0,1,2,3,4",
                    new[] { "No todos los pueblos del mapa están en la ruta: dos son distractores.", "En el primer pueblo tomaron el estandarte de la Virgen de Guadalupe; la campaña de septiembre terminó en la Alhóndiga de Granaditas." },
                    null, new[] { "Dolores:150,46", "Atotonilco:166,96", "San Miguel el Grande:184,128", "Celaya:268,182", "Guanajuato:70,156", "Querétaro:296,118", "Valladolid:128,214" }),
                A("caso", "¿Quién es El Campanero?", "Repasa las evidencias. Toca a un sospechoso y luego «Acusar»: el culpable tiene que encajar con TODAS las evidencias.", 90, "2",
                    new[] { "Toma una evidencia a la vez y descarta a quien no encaje con ella.", "El hilo azul no descarta a nadie de los que quedan al final: lo que decide está en la carta." },
                    new[] {
                        "Don Tomás, el sacristán|tomas|llave,pie,azul|Tiene llave de la torre · Es bajito · Estola azul · Enciende las velas con la derecha",
                        "Don Chuy, el nevero|chuy|llave|Guarda la llave de su abuelo · Calza del 29 · Ropa blanca y turquesa",
                        "Doña Lupita, la dulcera|lupita|llave,pie,azul,zurdo|Tiene llave (arregla las flores) · Es bajita · Rebozo azul · Acomoda las cocadas con la izquierda",
                        "Don Ramiro, el alfarero|alfarero|pie,azul|No tiene llave de la torre · Camisa azul" },
                    new[] { "llave:Tenía llave de la torre: la puerta no estaba forzada.", "pie:Dejó una huella chica de huarache junto a la estatua.", "azul:Un hilo azul quedó en la cuerda de la piñata.", "zurdo:La carta la escribió alguien zurdo." }),
                A("final", "La campana del Grito", "Arma la campana en el orden correcto: primero lo que la sostiene y al final lo que la hace sonar.", 45, "0,1,2,3",
                    new[] { "Piensa en cómo cuelga una campana de verdad: de arriba hacia abajo.", "Lo que golpea para hacerla sonar va por dentro: no puede ponerse antes que el cuerpo." },
                    null, new[] { "yugo", "corona", "cuerpo", "badajo" }),
            };
        }

        static ObjetoDatos O(string id, string nombre, string desc) { return new ObjetoDatos { id = id, nombre = nombre, descripcion = desc }; }

        static List<ObjetoDatos> Objetos()
        {
            var l = new List<ObjetoDatos>
            {
                O("vara", "Vara con gancho", "Una vara larga con gancho de metal. Sirve para alcanzar cosas altas."),
                O("vela", "Vela", "Una vela de cera, apagada."),
                O("cerillos", "Cerillos", "Una cajita de cerillos. Se pueden combinar con algo."),
                O("vela_encendida", "Vela encendida", "La vela ya tiene flama. ¡Cuidado!"),
                O("llave_campanario", "Llave del campanario", "Una llave grande de bronce."),
                O("partitura", "Partitura de campanas", "Notas escritas en tres renglones: unas arriba (agudas) y otras abajo (graves)."),
                O("llave_kiosko", "Llave del kiosko", "Una llave verde, como la reja del kiosko."),
                O("palo", "Palo de piñata", "Un palo forrado con listones de colores."),
                O("cajita", "Cajita de madera", "Tiene un candado con tres ruedas de dibujos. Tócala otra vez para abrirla."),
                O("llave_bodega", "Llave de la bodega", "La llave de la bodega de Doña Lupita."),
                O("carta", "Carta en blanco", "Una hoja que huele a limón... ¿y si se calienta?"),
                O("carta_revelada", "Carta revelada", "«Clave: XVI entre IV». Abajo hay letras revueltas. Combínala con la rueda de cifrado."),
                O("rueda", "Rueda de cifrado", "Dos discos de cartón con el alfabeto. Combínala con un mensaje cifrado."),
                O("llave_cofre", "Llave antigua", "Una llave de hierro con forma de campana."),
                O("corona", "Corona de la campana", "La parte de arriba de la campana."),
                O("yugo", "Yugo", "La viga de madera de la que cuelga la campana."),
                O("badajo", "Badajo", "El péndulo de hierro que golpea la campana."),
                O("cuerpo", "Cuerpo de la campana", "La parte que vibra y suena."),
            };
            // Las fichas solo dicen su letra (no su lugar): el criptex se deduce con lo que se aprendió en la plaza.
            string letras = "ALDAMA";
            for (int i = 0; i < 6; i++) l.Add(O("ficha" + (i + 1), "Ficha de barro «" + letras[i] + "»", "Ficha de barro con la letra " + letras[i] + ". No dice en qué lugar va."));
            return l;
        }

        static List<CombinacionDatos> Combinaciones()
        {
            return new List<CombinacionDatos>
            {
                new CombinacionDatos { a = "vela", b = "cerillos", resultado = "vela_encendida", mensaje = "¡Encendí la vela con un cerillo!" },
                new CombinacionDatos { a = "vela_encendida", b = "carta", resultado = "carta_revelada", mensaje = "Al calentar la carta con la vela aparecieron letras: ¡tinta invisible de limón!" },
                new CombinacionDatos { a = "rueda", b = "carta_revelada", resultado = "@cesar", mensaje = "", conservarA = true, conservarB = true },
            };
        }

        static LineaDatos L(string clave, string quien, string expr, string texto) { return new LineaDatos { clave = clave, quien = quien, expresion = expr, texto = texto }; }

        static List<LineaDatos> Dialogos()
        {
            return new List<LineaDatos>
            {
                // Generales
                L("no_sirve", "ana", "pensando", "Eso no sirve aquí."),
                L("no_combina", "ana", "pensando", "Esas dos cosas no se combinan."),
                L("mochila_llena", "ana", "sorpresa", "¡La mochila está llena!"),
                L("bienvenida", "ana", "feliz", "¡Hola! Soy Ana. Este es mi diario de pistas. Toca los lugares para explorar y recoger objetos."),
                // ---------- Nivel 1 ----------
                L("intro_n1", "ana", "sorpresa", "Madrugada del 15 de septiembre. Esta noche es el Grito... ¡y hay una nota clavada en la puerta de la Parroquia!"),
                L("intro_n1", "campanero", "", "«La campana del Grito ya no está. La desarmé y escondí sus cuatro piezas en el pueblo. Si la quieren de vuelta antes de las 11 de la noche, sigan mis pistas.» — El Campanero"),
                L("intro_n1", "ana", "pensando", "¿Quién haría algo así? Voy a encontrar las piezas... y a El Campanero. Empiezo por la placa del atrio."),
                L("n1_placa", "ana", "normal", "La placa dice: «Esta fachada se terminó en MDCCLXXVIII. En la madrugada del XVI del mes IX del año MDCCCX, aquí se llamó a la libertad»."),
                L("n1_placa", "ana", "pensando", "Números romanos... y dos fechas distintas. El candado de la sacristía pide un día y un mes."),
                L("n1_sacristia_cerrada", "ana", "pensando", "La puerta de la sacristía tiene un candado de cuatro ruedas."),
                L("n1_candado_ok", "ana", "feliz", "¡Clic! El candado se abrió."),
                L("n1_tomas", "tomas", "", "¡Ana! ¿Tú también viste la nota? Yo pasé la noche rezando el rosario... ¡y no oí nada!"),
                L("n1_tomas", "ana", "pensando", "(Don Tomás, el sacristán, enciende una vela con la mano derecha. Es bajito y trae su estola azul.)"),
                L("n1_tomas", "tomas", "", "Llaves de la torre solo hay tres: la mía, la de Doña Lupita, que arregla las flores del altar, y la vieja de Don Chuy, que era de su abuelo campanero."),
                L("n1_tomas", "ana", "normal", "Lo anoto en mi diario. Cualquier detalle puede servir."),
                L("n1_evidencia_llave", "ana", "sorpresa", "La puerta de la torre estaba cerrada con llave y no está forzada... pero hay cera fresca en los escalones."),
                L("n1_evidencia_llave", "ana", "pensando", "Evidencia: El Campanero tenía llave de la torre. Solo hay tres llaves."),
                L("n1_roseton_fuera", "ana", "normal", "El rosetón es precioso. Desde adentro se debe ver mejor."),
                L("n1_campanas_mudas", "ana", "normal", "Las campanas están en silencio. Para llegar al campanario hay que entrar por la torre."),
                L("n1_principal_cerrada", "ana", "normal", "La puerta principal está atrancada por dentro."),
                L("n1_principal_abierta", "ana", "feliz", "Quité la tranca de la puerta principal. Ahora puedo entrar directo a la nave."),
                L("n1_torre_cerrada", "ana", "pensando", "La puerta de la torre está cerrada con llave."),
                L("n1_torre_abre", "ana", "feliz", "¡La llave del campanario abre la torre!"),
                L("n1_ropero", "ana", "normal", "Casullas bordadas y olor a incienso. Nada más."),
                L("n1_ventana", "ana", "normal", "Por la ventana se ve la madrugada sobre Dolores."),
                L("n1_cuadro", "ana", "pensando", "Las piezas de este cuadro están revueltas... ¡es un rompecabezas!"),
                L("n1_cuadro_ok", "ana", "sorpresa", "¡El cuadro se abrió como una puertita! Hay algo brillando detrás."),
                L("n1_roseton_alto", "ana", "pensando", "El rosetón tiene tres anillos que giran, pero está muy alto. Necesito algo largo."),
                L("n1_roseton_ok", "ana", "sorpresa", "¡La luz de la luna atravesó el rosetón y cayó sobre el altar! Algo brilla ahí."),
                L("n1_altar", "ana", "normal", "El altar de la Virgen de los Dolores."),
                L("n1_retablo", "ana", "normal", "Un retablo dorado lleno de flores talladas."),
                L("n1_candelero", "ana", "normal", "El candelero del sacristán. Siempre deja cerca sus cerillos."),
                L("n1_candelabro_oscuro", "ana", "pensando", "Un candelabro de nueve velas unidas por brazos de bronce. Necesito una vela encendida para prenderlas."),
                L("n1_candelabro_ok", "ana", "feliz", "¡Las nueve velas encendidas! Ya se ve la escalera."),
                L("n1_escalera_oscura", "ana", "pensando", "Está muy oscuro para subir por la escalera."),
                L("n1_campanas_sin", "ana", "pensando", "Tres campanas: grande, mediana y chica. Si tuviera las notas del campanero..."),
                L("n1_campanas_ok", "ana", "sorpresa", "¡Sonaron las tres! Algo cayó al suelo."),
                L("fin_n1", "ana", "feliz", "¡La corona de la campana! Primera pieza."),
                L("fin_n1", "campanero", "", "Junto a la corona había otra nota: «Vas bien, Ana. La siguiente pieza está donde el pueblo se reúne.» — El Campanero"),
                L("fin_n1", "ana", "normal", "Ya está amaneciendo. ¡A la Plaza Principal!"),
                // ---------- Nivel 2 ----------
                L("intro_n2", "ana", "feliz", "Ya es de mañana. La plaza se prepara para el Grito: papel picado por todos lados... ¡y ya huele a nieve de garrafa!"),
                L("intro_n2", "chuy", "", "¡Pásele, pásele! Nieves de sabores que no va a encontrar en otro lado."),
                L("intro_n2", "ana", "pensando", "Don Chuy sabe todo lo que pasa en la plaza. A lo mejor sabe algo de la campana."),
                L("n2_chuy_1", "chuy", "", "¿La nota de El Campanero? ¡Qué susto! Yo toda la noche cargué hielo. Con estas patotas del 29 ni en lo oscuro me tropiezo."),
                L("n2_chuy_1", "chuy", "", "Vi a alguien de madrugada junto a la estatua... Pero primero ayúdeme: ¡se me revolvieron los vasos de muestra!"),
                L("n2_chuy_ok", "chuy", "", "¡Así mero! Tenga la llave del kiosko. Y ya abrí la nevería; pásele cuando quiera."),
                L("n2_chuy_despues", "chuy", "", "Esta mañana amanecieron fichas de barro con letras regadas por la plaza. ¿Serán de El Campanero? Forman el nombre de un valiente... pero revuelto."),
                L("n2_kiosko_cerrado", "ana", "pensando", "La reja del kiosko tiene candado. Don Chuy debe tener la llave."),
                L("n2_kiosko_abre", "ana", "feliz", "¡La llave abrió la reja del kiosko!"),
                L("n2_neveria_cerrada", "ana", "normal", "La nevería está cerrada. El letrero dice: «Pregunte a Don Chuy»."),
                L("n2_papel", "ana", "pensando", "Un papel picado sin recortar, con números en las orillas."),
                L("n2_papel_ok", "ana", "sorpresa", "¡Recorté una campana! Detrás del papel había una ficha."),
                L("n2_jarras_ok", "ana", "feliz", "¡Cuatro y cuatro! Debajo de la jarra había una ficha."),
                L("n2_garrafas", "ana", "normal", "Garrafas de nieve de limón, chocolate, fresa y mango. ¡Qué antojo!"),
                L("n2_letreros", "ana", "normal", "Sabores rarísimos: mole, camarón, tequila, aguacate... ¡Don Chuy es todo un inventor!"),
                L("n2_placa", "ana", "pensando", "La placa dice SEPTIEMBRE · 1810 y tiene cinco huecos. Hay que ordenar lo que pasó."),
                L("n2_placa_ok", "ana", "sorpresa", "¡Se abrió un compartimento en la base! Hay un cilindro con letras: un criptex."),
                L("n2_placa_ok", "ana", "pensando", "Y en el polvo, junto a la base, hay una huella chiquita de huarache. Evidencia: El Campanero tiene el pie chico."),
                L("n2_compartimento", "ana", "normal", "Una puertita de piedra con una campana grabada. No se abre así nada más."),
                L("n2_criptex_ok", "ana", "sorpresa", "¡ALDAMA! Dentro del criptex estaba el yugo de la campana."),
                L("n2_salida", "ana", "normal", "Desde el kiosko se ve toda la plaza."),
                L("fin_n2", "ana", "feliz", "¡El yugo! Ya tengo dos piezas."),
                L("fin_n2", "campanero", "", "Dentro del criptex venía una nota: «Lo que sigue es dulce... y se rompe a golpes.» — El Campanero"),
                L("fin_n2", "ana", "normal", "¿Dulce y a golpes? ¡Al Mercado Hidalgo!"),
                // ---------- Nivel 3 ----------
                L("intro_n3", "ana", "feliz", "El Mercado Hidalgo: cajeta, cocadas, jamoncillos..."),
                L("intro_n3", "lupita", "", "¡Ay, mija, qué bueno que llegas! Alguien me revolvió una cocada hueca entre las buenas."),
                L("intro_n3", "ana", "pensando", "Y en la piñata del pasillo algo suena adentro..."),
                L("n3_lupita_1", "lupita", "", "Ayúdame a encontrar la cocada hueca. Nomás te dejo usar la báscula dos veces, ¿eh?"),
                L("n3_lupita_1", "ana", "pensando", "(Doña Lupita acomoda las cocadas con la mano izquierda, rapidísimo.)"),
                L("n3_lupita_ok", "lupita", "", "¡Esa era! Gracias, mija. Toma la llave de la bodega."),
                L("n3_lupita_ok", "lupita", "", "Y si encuentras una cajita con candado de dibujos... dicen que la clave está en la bodega."),
                L("n3_lupita_despues", "lupita", "", "La bodega está al fondo del pasillo, mija."),
                L("n3_pinata_sin", "ana", "pensando", "La piñata está muy alta. Necesito un palo."),
                L("n3_pinata_ok", "ana", "sorpresa", "¡Se rompió! Entre los dulces cayó una cajita de madera."),
                L("n3_pinata_ok", "ana", "pensando", "En la cuerda de la piñata quedó enganchado un hilo azul. Evidencia: El Campanero llevaba algo azul."),
                L("n3_bodega_cerrada", "ana", "pensando", "La bodega tiene llave. Doña Lupita debe tenerla."),
                L("n3_bodega_abre", "ana", "feliz", "¡La llave abrió la bodega!"),
                L("n3_alfarero_1", "alfarero", "", "Buenas. El canal de mi mural quedó todo chueco. Si me lo arregla, le presto mi palo de piñata."),
                L("n3_canal_ok", "alfarero", "", "¡Ya corre el agua! Tenga el palo... y no le pegue a mis platos."),
                L("n3_alfarero_despues", "alfarero", "", "La talavera se hace igual que hace doscientos años: barro, esmalte y mucha paciencia."),
                L("n3_ollas_ok", "ana", "sorpresa", "Al mover las ollas aparecieron tres símbolos pintados en el piso."),
                L("n3_costales", "ana", "normal", "Costales de azúcar y coco para las cocadas."),
                L("n3_simbolos_ok", "ana", "sorpresa", "¡Clic! Dentro de la cajita estaba el badajo de la campana."),
                L("fin_n3", "ana", "feliz", "¡El badajo! Solo falta una pieza."),
                L("fin_n3", "campanero", "", "Pegada a la cajita: «La última pieza está en la casa del que llamó al pueblo. Ahí sabrás quién soy.» — El Campanero"),
                L("fin_n3", "ana", "normal", "La casa del que llamó al pueblo... ¡la Casa de Hidalgo!"),
                // ---------- Nivel 4 ----------
                L("intro_n4", "ana", "pensando", "La Casa de Hidalgo. Aquí vivió el cura Miguel Hidalgo."),
                L("intro_n4", "ana", "sorpresa", "Hay una vitrina vacía con el contorno de una campana. Aquí la voy a armar... cuando sepa quién es El Campanero."),
                L("n4_vitrina_falta", "ana", "pensando", "Para armar la campana necesito las cuatro piezas."),
                L("n4_cajon", "ana", "sorpresa", "¡En el cajón hay una rueda de cartón con letras!"),
                L("n4_cajon_vacio", "ana", "normal", "El cajón ya está vacío."),
                L("n4_candelero", "ana", "pensando", "Un candelero sin vela. En la biblioteca vi una."),
                L("n4_carta_info", "ana", "pensando", "La carta dice: «Clave: XVI entre IV». Abajo hay letras sin sentido."),
                L("n4_evidencia_zurdo", "ana", "sorpresa", "¡Un momento! La tinta de la carta está corrida hacia la derecha: la escribió alguien zurdo, que arrastró la mano sobre la tinta."),
                L("n4_evidencia_zurdo", "ana", "pensando", "Es la misma letra de las notas. Evidencia: El Campanero es zurdo."),
                L("n4_cesar_ok", "ana", "sorpresa", "«Los libros guardan la combinación: ordénalos por su año». ¡A la biblioteca!"),
                L("n4_libros_antes", "ana", "pensando", "Libros sobre la vida de Hidalgo, todos revueltos. Cada uno tiene un año en números romanos y un número dorado."),
                L("n4_libros_ok", "ana", "feliz", "Ordenados por año, los números dorados quedaron en fila. Los voy a anotar."),
                L("n4_caja_ok", "ana", "sorpresa", "¡Se abrió! Hay una llave antigua y un mapa del camino insurgente."),
                L("n4_cofre_cerrado", "ana", "pensando", "Un cofre bajo la morera, con una cerradura antigua."),
                L("n4_cofre_ruta", "ana", "pensando", "La llave entra, pero hay otro seguro: botones con nombres de pueblos. El mapa de piedra dice el camino."),
                L("n4_mapa", "ana", "normal", "Un mapa de piedra con los pueblos de la Intendencia de Guanajuato."),
                L("n4_ruta_ok", "ana", "sorpresa", "¡El cofre se abrió! Adentro está el cuerpo de la campana."),
                L("n4_morera", "ana", "normal", "La morera. Hidalgo enseñó aquí a criar gusanos de seda."),
                L("n4_estantes", "ana", "normal", "Libros de agricultura, de música y de teología."),
                L("fin_n4", "ana", "feliz", "¡El cuerpo de la campana! Ya tengo las cuatro piezas."),
                L("fin_n4", "ana", "pensando", "Antes de armarla tengo que resolver el caso. Con las cuatro evidencias ya puedo saber quién es El Campanero."),
                L("fin_n4", "ana", "normal", "A la vitrina de la sala."),
                L("caso_antes", "ana", "pensando", "Repasemos el caso: cuatro evidencias y cuatro sospechosos."),
                L("confesion", "lupita", "", "Ay, mija... me descubriste."),
                L("confesion", "lupita", "", "Cada año suena la campana y nadie sabe por qué suena. Quise que alguien la buscara como la buscaron en 1810."),
                L("confesion", "lupita", "", "Las notas, las fichas, la cajita... todo lo escondí yo. Y tú encontraste todo. Ándale: ármala y tócala tú."),
                L("confesion", "ana", "feliz", "¡Todavía llegamos al Grito!"),
                L("final", "ana", "sorpresa", "¡La campana está completa!"),
                L("final", "ana", "feliz", "Dolores vuelve a escuchar su campana. ¡Leyenda resuelta!"),
            };
        }
    }
}
