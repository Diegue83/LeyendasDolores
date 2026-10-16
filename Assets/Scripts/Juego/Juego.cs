// Controlador principal: vistas, mochila, diálogo, acertijos, puntuación, tarjetas y guardado.
// La lógica de cada nivel (qué pasa al tocar cada cosa) está en Niveles.cs.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Leyendas
{
    public partial class Juego : MonoBehaviour
    {
        public static Juego I;

        [Header("Pantalla")]
        public RectTransform pantalla, escenario, vistasRaiz, chispas, volar, confeti;
        public CanvasGroup transicion;
        public Button bVolver;
        [Header("Barra superior")]
        public Text titulo, lugar, puntos;
        public Image[] progreso = new Image[4];
        public Image[] piezasRanura = new Image[4], piezasIco = new Image[4];
        public Button bMapa, bDiario, bPista, bSonido;
        public Image iconoSonido;
        [Header("Diálogo")]
        public RectTransform dialogoPanel, indicador;
        public Image retrato;
        public Text hablante, textoDialogo;
        [Header("Mochila")]
        public Image[] ranuras = new Image[8], iconos = new Image[8];
        public Text ayuda;
        [Header("Acertijo")]
        public GameObject modal, resultado;
        public RectTransform modalPanel, modalContenido;
        public Text modalTitulo, modalInstr, modalMensaje, modalReloj, modalPar;
        public Button modalCerrar, modalPista;
        public Text resTitulo, resTiempo, resPuntos, resRecord;
        public Image[] resEstrellas = new Image[3];
        public Button resContinuar;
        [Header("Tarjeta")]
        public GameObject tarjeta;
        public RectTransform tarjetaPanel, tarjetaBotones;
        public Text tarjetaTitulo, tarjetaTexto;
        public Image tarjetaImagen;
        public ScrollRect tarjetaScroll;
        [Header("Mapa")]
        public GameObject mapa;
        public RectTransform mapaPanel;
        public Button[] mapaLugares = new Button[4];   // marcador de cada lugar sobre el mapa (en orden de niveles)
        public Image[] mapaMarcas = new Image[4];
        public Text[] mapaEtiquetas = new Text[4];
        public Text mapaNombre, mapaInfo;
        public Button mapaViajar, mapaCerrar;
        [Header("Portada")]
        public GameObject portada;
        public Text portadaTitulo, portadaSub;
        public Button bJugar, bNueva;

        [Header("Escenas de nivel (se cargan de forma aditiva; sus vistas se pasan al escenario)")]
        public string[] escenasNiveles = { "N1_Parroquia", "N2_Plaza", "N3_Mercado", "N4_CasaHidalgo" };

        [Header("Ajustes")]
        [Tooltip("Letras por segundo del texto del diálogo")] public float velocidadTexto = 55f;
        [Tooltip("Duración del cambio de vista")] public float duracionTransicion = .35f;

        public Estado E;
        public DatosJuego D { get { return DatosJuego.D; } }
        readonly Dictionary<string, Vista> vistas = new Dictionary<string, Vista>();
        Vista vistaActual;
        bool enTransicion;
        string seleccionado;
        static readonly string[] PIEZAS = { "corona", "yugo", "badajo", "cuerpo" };

        // ======================================================= Inicio
        // true cuando ya se cargaron las escenas de nivel y el juego está listo para jugarse.
        public bool Listo { get; private set; }

        void Awake()
        {
            I = this;
            Tw.Host(this);
            vistas.Clear();
            RegistrarVistas();
        }

        void RegistrarVistas()
        {
            foreach (var v in vistasRaiz.GetComponentsInChildren<Vista>(true)) vistas[v.id] = v;
        }

        // Carga cada escena de nivel (si no está abierta ya en el Editor) y pasa sus vistas al escenario del juego.
        // Las vistas ya existen guardadas en esas escenas: aquí no se construye nada.
        IEnumerator CargarNiveles()
        {
            foreach (var nombre in escenasNiveles)
            {
                if (string.IsNullOrEmpty(nombre)) continue;
                var esc = SceneManager.GetSceneByName(nombre);
                if (!esc.isLoaded)
                {
                    var op = SceneManager.LoadSceneAsync(nombre, LoadSceneMode.Additive);
                    if (op == null) { Debug.LogError("Leyendas: la escena " + nombre + " no está en Build Profiles."); continue; }
                    while (!op.isDone) yield return null;
                    esc = SceneManager.GetSceneByName(nombre);
                }
                foreach (var raiz in esc.GetRootGameObjects())
                {
                    var nivel = raiz.GetComponent<NivelEscena>();
                    if (nivel == null) continue;
                    foreach (var v in nivel.GetComponentsInChildren<Vista>(true))
                    {
                        v.transform.SetParent(vistasRaiz, false);
                        v.gameObject.SetActive(false);
                    }
                    raiz.SetActive(false);
                }
            }
            RegistrarVistas();
            if (vistas.Count == 0) Debug.LogError("Leyendas: no se encontró ninguna vista. Revisa las escenas de Assets/Escenas/Niveles.");
        }

        IEnumerator Start()
        {
            yield return CargarNiveles();
            E = Estado.Cargar();
            Sonido.Activo = E.sonido;
            PrepararMenu();
            bMapa.onClick.AddListener(MostrarMapa);
            for (int i = 0; i < mapaLugares.Length; i++) { int k = i; if (mapaLugares[i] != null) mapaLugares[i].onClick.AddListener(() => ElegirLugar(k)); }
            if (mapaViajar != null) mapaViajar.onClick.AddListener(ViajarAlLugar);
            if (mapaCerrar != null) mapaCerrar.onClick.AddListener(CerrarMapa);
            bDiario.onClick.AddListener(MostrarDiario);
            bPista.onClick.AddListener(PistaGeneral);
            bSonido.onClick.AddListener(CambiarSonido);
            bVolver.onClick.AddListener(Volver);
            modalCerrar.onClick.AddListener(() => CerrarAcertijo(false));
            modalPista.onClick.AddListener(PedirPista);
            resContinuar.onClick.AddListener(() => CerrarAcertijo(true));
            bJugar.onClick.AddListener(Jugar);
            bNueva.onClick.AddListener(NuevaPartida);
            for (int i = 0; i < ranuras.Length; i++)
            {
                int k = i;
                var b = F.Asegurar<Button>(ranuras[i].gameObject);
                b.onClick.AddListener(() => TocarRanura(k));
            }
            var ad = F.Asegurar<AlTocar>(dialogoPanel.gameObject);
            ad.accion = SiguienteLinea;
            var anaN = Arte.S("Retratos/ana_normal");
            var parp = retrato.GetComponent<Parpadeo>();
            if (parp != null) { parp.abierto = anaN; parp.cerrado = Arte.S("Retratos/ana_parpadeo"); }
            portadaTitulo.text = D.titulo; portadaSub.text = D.subtitulo; titulo.text = D.titulo;
            // La barra muestra el reloj del nivel y los puntos de la partida.
            var icoBarra = puntos.transform.parent.Find("Estrella");
            if (icoBarra != null) icoBarra.GetComponent<Image>().sprite = Arte.S("Iconos/reloj");
            puntos.resizeTextForBestFit = true; puntos.resizeTextMinSize = 11; puntos.resizeTextMaxSize = 18;
            puntos.horizontalOverflow = HorizontalWrapMode.Wrap;
            modal.SetActive(false); tarjeta.SetActive(false);
            if (Arranque.NivelDePrueba >= 0) IniciarPrueba(Arranque.NivelDePrueba);
            else MostrarPortada();
            Listo = true;
        }

        // Play desde una escena de nivel: empieza en ese nivel, con las piezas de los anteriores y sin guardar la partida.
        bool modoPrueba;
        void IniciarPrueba(int n)
        {
            modoPrueba = true;
            E = new Estado { sonido = E.sonido, empezada = true, nivelMax = n };
            for (int i = 0; i < n; i++) { E.piezas.Add(D.Nivel(i).pieza); E.completos.Add(i); }
            Sonido.Activo = E.sonido;
            portada.SetActive(false);
            Sonido.Ambiente(true);
            EntrarNivel(n, true);
        }

        void MostrarPortada()
        {
            portada.SetActive(true);
            F.TextoBoton(bJugar, E.empezada ? "Continuar" : "Jugar");
            bNueva.gameObject.SetActive(E.empezada && bPartidas == null);   // con el menú, «Nueva partida» está en Partidas
            PintarPortada();
            Tw.Aparecer(portadaTitulo.transform, .6f);
            Tw.Aparecer(bJugar.transform, .4f, .25f);
            ActualizarSonido();
            Sonido.Ambiente(true);
        }

        void Jugar()
        {
            var cg = F.Asegurar<CanvasGroup>(portada.gameObject);
            Tw.Run(OcultarPortada(cg));
        }

        IEnumerator OcultarPortada(CanvasGroup cg)
        {
            yield return Tw.Alfa(cg, 1, 0, .4f);
            portada.SetActive(false); cg.alpha = 1;
            if (!E.empezada)
            {
                E.empezada = true;
                EntrarNivel(0, true);
                Decir("bienvenida");
                Decir("intro_n1");
                E.Poner("intro_n1");
            }
            else EntrarNivel(E.nivel, false);
        }

        void NuevaPartida()
        {
            bool son = E.sonido;
            var records = E.records;
            Estado.Borrar();
            E = new Estado { sonido = son, records = records };
            Deseleccionar();
            Jugar();
        }

        // ======================================================= Niveles
        public void EntrarNivel(int n, bool desdeInicio)
        {
            E.nivel = n;
            E.nivelMax = Mathf.Max(E.nivelMax, n);
            var nv = D.Nivel(n);
            string destino = !desdeInicio && !string.IsNullOrEmpty(E.vista) && E.vista.StartsWith(nv.id + "_") && vistas.ContainsKey(E.vista) ? E.vista : nv.vistaInicial;
            limpiarDialogo();
            // Foto del nivel al entrar por primera vez (antes de su introducción): «Reiniciar este lugar» vuelve a ella.
            if (!E.Completo(n) && string.IsNullOrEmpty(E.inicio[n]) && !E.B("intro_" + nv.id)) E.inicio[n] = E.Foto();
            IrA(destino, false);
            if (!E.B("intro_" + nv.id)) { E.Poner("intro_" + nv.id); Decir("intro_" + nv.id); }
            ActualizarHUD();
            Guardar();
        }

        public int NivelDe(string vistaId) { return Mathf.Clamp((vistaId.Length > 1 ? vistaId[1] - '1' : 0), 0, 3); }

        // ======================================================= Vistas
        public void IrA(string id, bool animar = true)
        {
            Vista v;
            if (!vistas.TryGetValue(id, out v)) { Debug.LogWarning("Leyendas: no existe la vista " + id); return; }
            Deseleccionar();
            if (animar && vistaActual != null && isActiveAndEnabled) { Tw.Run(Transicion(v)); return; }
            Mostrar(v);
        }

        IEnumerator Transicion(Vista v)
        {
            enTransicion = true;
            transicion.blocksRaycasts = true;
            Sonido.Tocar("puerta", 1.1f, .6f);
            yield return Tw.Alfa(transicion, 0, 1, duracionTransicion * .8f);
            Mostrar(v);
            var rt = (RectTransform)v.transform;
            Tw.Escala(rt, Vector3.one * 1.06f, Vector3.one, duracionTransicion * 1.6f);
            yield return Tw.Alfa(transicion, 1, 0, duracionTransicion);
            transicion.blocksRaycasts = false;
            enTransicion = false;
        }

        void Mostrar(Vista v)
        {
            if (vistaActual != null) vistaActual.gameObject.SetActive(false);
            vistaActual = v;
            v.gameObject.SetActive(true);
            v.transform.localScale = Vector3.one;
            E.vista = v.id;
            Refrescar(false);
            var vd = D.Vista(v.id);
            bVolver.gameObject.SetActive(!string.IsNullOrEmpty(vd.padre));
            lugar.text = D.Nivel(NivelDe(v.id)).nombre + " · " + vd.nombre;
            AlEntrar(v.id);
            Guardar();
        }

        public string VistaActual { get { return vistaActual != null ? vistaActual.id : ""; } }

        void Volver()
        {
            if (vistaActual == null || enTransicion) return;
            var p = D.Vista(vistaActual.id).padre;
            if (!string.IsNullOrEmpty(p)) IrA(p);
        }

        // Muestra u oculta parches, recortes y objetos según el estado.
        public void Refrescar(bool animar = true)
        {
            if (vistaActual == null) return;
            foreach (var c in vistaActual.capas)
            {
                if (c == null) continue;
                bool ver = CapaVisible(vistaActual.id, c);
                if (ver == c.gameObject.activeSelf) continue;
                c.gameObject.SetActive(ver);
                if (ver && animar)
                {
                    var cg = F.Asegurar<CanvasGroup>(c.gameObject);
                    Tw.Alfa(cg, 0, 1, .6f);
                    if (c.esObjeto) Tw.Aparecer(c.transform, .45f, .2f);
                }
            }
        }

        public Capa CapaDe(string nombre) { return vistaActual != null ? vistaActual.Capa(nombre) : null; }

        // ======================================================= Toques
        public void TocarZona(string vista, string zona, ZonaInteractiva z)
        {
            if (enTransicion || modal.activeSelf || tarjeta.activeSelf || MapaAbierto || MenuAbierto || portada.activeSelf) return;
            if (vistaActual == null || vista != vistaActual.id) return;
            Interrumpir();
            Sonido.Tocar("toque", 1f, .5f);
            // la "cámara" se inclina un poco hacia lo que se tocó (solo en vistas con planos de profundidad)
            var par = vistaActual.GetComponent<Paralaje>();
            if (par != null && z != null)
            {
                var rz = (RectTransform)z.transform;
                Vector2 p = (Vector2)escenario.InverseTransformPoint(rz.TransformPoint(rz.rect.center)) - escenario.rect.center;
                par.Empujar(new Vector2(p.x / (escenario.rect.width * .5f), p.y / (escenario.rect.height * .5f)));
            }
            if (zona.StartsWith("obj_")) { Deseleccionar(); Recoger(vista, zona, z); return; }
            if (seleccionado != null)
            {
                var sel = seleccionado;
                Deseleccionar();
                if (Necesita(vista, zona) != sel) { Decir("no_sirve"); return; }
            }
            Accion(vista, zona);
        }

        // ======================================================= Mochila
        public void TocarRanura(int i)
        {
            if (modal.activeSelf || tarjeta.activeSelf || MapaAbierto || MenuAbierto || enTransicion) return;
            if (i >= E.mochila.Count) { Deseleccionar(); return; }
            Interrumpir();
            var obj = E.mochila[i];
            if (seleccionado == null)
            {
                seleccionado = obj;
                PintarMochila();
                Tw.Brinco(ranuras[i].transform);
                var od = D.Objeto(obj);
                DecirTexto("ana", "pensando", "<b>" + od.nombre + "</b>\n" + od.descripcion);
                return;
            }
            if (seleccionado == obj)
            {
                Deseleccionar();
                UsarSolo(obj);
                return;
            }
            var a = seleccionado;
            Deseleccionar();
            Combinar(a, obj);
        }

        void Deseleccionar() { seleccionado = null; PintarMochila(); }

        void PintarMochila()
        {
            for (int i = 0; i < ranuras.Length; i++)
            {
                bool hay = i < E.mochila.Count;
                iconos[i].enabled = hay;
                if (hay) iconos[i].sprite = Arte.S("Objetos/" + E.mochila[i]);
                ranuras[i].color = hay && E.mochila[i] == seleccionado ? Pal.MAIZ : Color.white;
            }
        }

        void Combinar(string a, string b)
        {
            var c = D.combinaciones.Find(x => (x.a == a && x.b == b) || (x.a == b && x.b == a));
            if (c == null) { Decir("no_combina"); Sonido.Tocar("error", 1f, .6f); return; }
            if (c.resultado.StartsWith("@")) { CombinacionEspecial(c.resultado.Substring(1)); return; }
            if (!c.conservarA) QuitarObjeto(c.a);
            if (!c.conservarB) QuitarObjeto(c.b);
            E.Poner("combo_" + D.Nivel(E.nivel).id + "_" + c.resultado);
            DarObjeto(c.resultado, null);
            if (!string.IsNullOrEmpty(c.mensaje)) DecirTexto("ana", "feliz", c.mensaje);
            AlCombinar(c.resultado);
            Sonido.Tocar("acierto", 1.2f);
        }

        public void DarObjeto(string id, RectTransform desde)
        {
            if (E.mochila.Contains(id)) return;
            if (E.mochila.Count >= ranuras.Length) { Decir("mochila_llena"); return; }
            E.mochila.Add(id);
            int i = E.mochila.Count - 1;
            Sonido.Tocar("objeto");
            PintarMochila();
            iconos[i].enabled = false;
            Tw.Run(Volar(Arte.S("Objetos/" + id), desde, ranuras[i].rectTransform, () => { iconos[i].enabled = i < E.mochila.Count; Tw.Brinco(ranuras[i].transform, .25f); PintarMochila(); }));
            Guardar();
        }

        public void QuitarObjeto(string id)
        {
            if (E.mochila.Remove(id)) PintarMochila();
            if (seleccionado == id) seleccionado = null;
        }

        public bool Lleva(string id) { return E.Lleva(id); }

        // Vuelo de un objeto desde el escenario (o el centro) hasta su ranura.
        IEnumerator Volar(Sprite s, RectTransform desde, RectTransform hasta, Action alLlegar, float tam = 120)
        {
            var im = F.Img("Volando", volar, null);
            im.sprite = s; im.preserveAspect = true;
            var rt = im.rectTransform;
            F.Centro(rt, 0, 0, tam, tam);
            Vector2 a = desde != null ? (Vector2)volar.InverseTransformPoint(desde.TransformPoint(desde.rect.center)) : (Vector2)volar.InverseTransformPoint(escenario.TransformPoint(escenario.rect.center));
            Vector2 b = volar.InverseTransformPoint(hasta.TransformPoint(hasta.rect.center));
            rt.anchoredPosition = a;
            yield return Tw.Valor(.25f, k => { if (rt) rt.localScale = Vector3.one * (.6f + .7f * k); }, Tw.Rebote);
            yield return new WaitForSecondsRealtime(.15f);
            yield return Tw.Arco(rt, a, b, 140, .65f, .62f);
            Destroy(im.gameObject);
            alLlegar?.Invoke();
        }

        void Recoger(string vista, string capa, ZonaInteractiva z)
        {
            var obj = capa.Substring(4);
            E.Poner("rec_" + vista + "_" + capa);
            var rt = (RectTransform)z.transform;
            if (Array.IndexOf(PIEZAS, obj) >= 0) { z.gameObject.SetActive(false); ObtenerPieza(obj, rt); return; }
            if (E.mochila.Count >= ranuras.Length) { Decir("mochila_llena"); E.banderas.Remove("rec_" + vista + "_" + capa); return; }
            DarObjeto(obj, rt);
            z.gameObject.SetActive(false);
            var od = D.Objeto(obj);
            DecirTexto("ana", "feliz", "Encontré: <b>" + od.nombre + "</b>.\n" + od.descripcion);
            AlRecoger(obj);
        }

        // ======================================================= Piezas de la campana
        void ObtenerPieza(string pieza, RectTransform desde)
        {
            if (E.piezas.Contains(pieza)) return;
            E.piezas.Add(pieza);
            int i = Array.IndexOf(PIEZAS, pieza);
            Sonido.Tocar("campana_grande");
            Confeti.Lluvia(confeti, Vector2.zero);
            Tw.Run(Volar(Arte.S("Objetos/" + pieza), desde, piezasRanura[i].rectTransform, () => { ActualizarHUD(); Tw.Brinco(piezasRanura[i].transform, .4f, .5f); }, 200));
            // Cada pieza pertenece a su nivel (aunque se consiga estando en otro lugar, p. ej. abriendo la cajita
            // después de viajar con el mapa). Los niveles 1 a 3 terminan con su pieza; el 4, al armar la campana.
            int n = i;
            if (n < 3) CompletarNivel(n);
            Guardar();
            Decir("fin_" + D.Nivel(n).id, () => { if (n < 3) TarjetaNivel(n); });
        }

        // ======================================================= HUD
        public void ActualizarHUD()
        {
            for (int i = 0; i < 4; i++)
            {
                bool hecho = E.piezas.Contains(D.Nivel(i).pieza);
                progreso[i].color = hecho ? Pal.MAIZ : i == E.nivel ? Pal.ROSA : i <= E.nivelMax ? Pal.PAPEL2 : Pal.BORDE;
                progreso[i].rectTransform.localScale = Vector3.one * (i == E.nivel ? 1.25f : 1f);
                bool tiene = E.piezas.Contains(PIEZAS[i]);
                piezasIco[i].color = tiene ? Color.white : Pal.A(Color.white, .2f);
            }
            PintarReloj(true);
            PintarMochila();
        }

        // Reloj del nivel actual en la barra (corre mientras se juega el nivel y se detiene al terminarlo).
        int segBarra = -1;
        void PintarReloj(bool forzar = false)
        {
            if (E == null) return;
            int n = E.nivel;
            int s = Mathf.FloorToInt(E.tiempo[n]);
            if (!forzar && s == segBarra) return;
            segBarra = s;
            float par = ParNivel(n);
            puntos.text = Reloj(E.tiempo[n]) + "  ·  " + E.Total.ToString("N0") + " pts";
            puntos.color = E.Completo(n) ? Pal.OK : E.tiempo[n] <= par ? Pal.PAPEL : E.tiempo[n] <= par * 1.5f ? Pal.MAIZ : Pal.MAL;
        }

        void CambiarSonido()
        {
            E.sonido = !E.sonido;
            Sonido.Activo = E.sonido;
            Sonido.Ambiente(E.sonido);
            ActualizarSonido();
            Guardar();
        }

        void ActualizarSonido() { iconoSonido.sprite = Arte.S(E.sonido ? "Iconos/sonido" : "Iconos/mudo"); }

        public void Guardar() { if (E != null && !modoPrueba) E.Guardar(); }

        void OnApplicationPause(bool p) { if (p) Guardar(); }
        void OnApplicationQuit() { Guardar(); }

        // ======================================================= Diálogo
        class Linea { public string quien, expr, texto; public Action despues; }
        readonly Queue<Linea> cola = new Queue<Linea>();
        Coroutine escribiendo;
        Linea lineaActual;
        bool completa = true;

        public void Decir(string clave, Action despues = null)
        {
            var l = D.Dialogo(clave);
            if (l.Count == 0) { despues?.Invoke(); return; }
            for (int i = 0; i < l.Count; i++)
                Encolar(new Linea { quien = l[i].quien, expr = l[i].expresion, texto = l[i].texto, despues = i == l.Count - 1 ? despues : null });
        }

        public void DecirTexto(string quien, string expr, string texto, Action despues = null)
        {
            Encolar(new Linea { quien = quien, expr = expr, texto = texto, despues = despues });
        }

        void Encolar(Linea l)
        {
            cola.Enqueue(l);
            if (lineaActual == null || (completa && lineaActual.despues == null && cola.Count == 1)) SiguienteLinea();
        }

        void limpiarDialogo() { cola.Clear(); lineaActual = null; completa = true; }

        // Un toque nuevo del jugador reemplaza al instante lo que se está escribiendo y lo que estaba en espera,
        // para que el cuadro siempre hable de lo último que se tocó. No se corta una secuencia que tiene una acción
        // pendiente al terminar (abrir un acertijo, la tarjeta de fin de nivel...).
        void Interrumpir()
        {
            if (lineaActual != null && lineaActual.despues != null) return;
            if (cola.Any(l => l.despues != null)) return;
            if (escribiendo != null) { StopCoroutine(escribiendo); escribiendo = null; }
            limpiarDialogo();
            indicador.gameObject.SetActive(false);
        }

        void SiguienteLinea()
        {
            if (!completa && lineaActual != null)
            {
                // Termina de escribir de golpe.
                if (escribiendo != null) StopCoroutine(escribiendo);
                textoDialogo.text = lineaActual.texto;
                completa = true;
                indicador.gameObject.SetActive(cola.Count > 0 || lineaActual.despues != null);
                // Sigue avanzando solo (antes, al saltar la última línea, su acción —p. ej. la tarjeta de fin de nivel— esperaba otro toque).
                escribiendo = StartCoroutine(AutoAvanzar(lineaActual));
                return;
            }
            var termino = lineaActual;
            if (termino != null && termino.despues != null) { var d = termino.despues; termino.despues = null; d(); if (lineaActual != termino) return; }
            if (cola.Count == 0) { indicador.gameObject.SetActive(false); return; }
            lineaActual = cola.Dequeue();
            if (escribiendo != null) StopCoroutine(escribiendo);
            escribiendo = StartCoroutine(Escribir(lineaActual));
        }

        IEnumerator Escribir(Linea l)
        {
            completa = false;
            indicador.gameObject.SetActive(false);
            string quien = string.IsNullOrEmpty(l.quien) ? "ana" : l.quien;
            string spr = quien == "ana" ? "Retratos/ana_" + (string.IsNullOrEmpty(l.expr) ? "normal" : l.expr) : quien == "campanero" ? "Retratos/pista" : "Retratos/" + quien;
            retrato.sprite = Arte.S(spr);
            var parp = retrato.GetComponent<Parpadeo>();
            if (parp != null) { parp.enabled = quien == "ana"; parp.abierto = retrato.sprite; }
            hablante.text = quien == "ana" ? "Ana" : quien == "chuy" ? "Don Chuy" : quien == "lupita" ? "Doña Lupita" : quien == "alfarero" ? "Don Ramiro" : quien == "tomas" ? "Don Tomás" : quien == "campanero" ? "El Campanero" : "Pista";
            Tw.Brinco(retrato.transform, .08f, .25f);
            HablaPersonaje(quien);
            Sonido.Tocar("papel", 1.1f, .35f);
            // Escribe respetando las etiquetas <b>...</b>.
            string t = l.texto;
            float acum = 0; int vis = 0;
            while (vis < t.Length)
            {
                acum += Time.unscaledDeltaTime * velocidadTexto;
                while (acum >= 1 && vis < t.Length)
                {
                    if (t[vis] == '<') { int f = t.IndexOf('>', vis); vis = f < 0 ? t.Length : f + 1; }
                    else { vis++; acum -= 1; }
                }
                textoDialogo.text = Cortar(t, vis);
                yield return null;
            }
            textoDialogo.text = t;
            completa = true;
            indicador.gameObject.SetActive(cola.Count > 0 || l.despues != null);
            yield return AutoAvanzar(l);
        }

        // Pasa sola a la siguiente línea (o ejecuta la acción pendiente) después de un momento.
        IEnumerator AutoAvanzar(Linea l)
        {
            if (cola.Count > 0) { yield return new WaitForSecondsRealtime(1.6f + l.texto.Length * .02f); if (lineaActual == l && completa) SiguienteLinea(); }
            else if (l.despues != null) { yield return new WaitForSecondsRealtime(1.2f); if (lineaActual == l && completa) SiguienteLinea(); }
        }

        static string Cortar(string t, int n)
        {
            var s = t.Substring(0, n);
            int ab = s.LastIndexOf("<b>"), ce = s.LastIndexOf("</b>");
            if (ab > ce) s += "</b>";
            return s;
        }

        void HablaPersonaje(string quien)
        {
            if (vistaActual == null) return;
            var c = vistaActual.Capa("pj_" + quien);
            if (c != null) { var r = c.GetComponent<Respirar>(); if (r != null) r.Hablar(1.6f); }
        }

        // ======================================================= Chispas (pistas visuales)
        public void Chispa(RectTransform zona, float duracion = 3f)
        {
            if (zona == null) return;
            var im = F.Img("Chispa", chispas, "UI/chispa");
            var rt = im.rectTransform;
            F.Centro(rt, 0, 0, 90, 90);
            rt.anchoredPosition = (Vector2)chispas.InverseTransformPoint(zona.TransformPoint(zona.rect.center));
            im.gameObject.AddComponent<Rotar>().gradosPorSegundo = 60;
            var b = im.gameObject.AddComponent<Brillo>(); b.minimo = .4f; b.velocidad = 6;
            Tw.Aparecer(rt, .3f);
            Destroy(im.gameObject, duracion);
        }

        public IEnumerator Luego(float s, Action a) { yield return new WaitForSecondsRealtime(s); a(); }
        public void Despues(float s, Action a) { StartCoroutine(Luego(s, a)); }
    }
}
