// Menú principal (Continuar, Partidas, Ajustes, Créditos, Salir), partidas en 3 ranuras, ajustes, créditos,
// pausa dentro del juego (Continuar, Reiniciar lugar, Ajustes, Menú principal) y la ventana de confirmación.
// Las ventanas están en Juego.unity (Hierarchy: Pantalla > Pausa, Partidas, Ajustes, Creditos, Confirmar);
// aquí solo se conectan los botones y se pintan los textos.
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Leyendas
{
    public partial class Juego
    {
        [Header("Menú principal")]
        public Button bPartidas, bAjustes, bCreditos, bSalir;
        public Text portadaPartida;
        [Header("Pausa")]
        public Button bMenu;
        public GameObject pausa;
        public RectTransform pausaPanel;
        public Text pausaLugar;
        public Button pausaContinuar, pausaReiniciar, pausaAjustes, pausaMenu;
        [Header("Partidas")]
        public GameObject partidas;
        public RectTransform partidasPanel;
        public Text[] ranuraTitulo = new Text[Estado.RANURAS], ranuraInfo = new Text[Estado.RANURAS];
        public Image[] ranuraFondo = new Image[Estado.RANURAS];
        public Button[] ranuraJugar = new Button[Estado.RANURAS], ranuraNueva = new Button[Estado.RANURAS], ranuraBorrar = new Button[Estado.RANURAS];
        public Button partidasCerrar;
        [Header("Ajustes")]
        public GameObject ajustes;
        public RectTransform ajustesPanel;
        public Image ajMusicaBarra, ajEfectosBarra;
        public Text ajMusicaValor, ajEfectosValor, ajTextoValor, ajPantallaValor;
        public Button ajMusicaMenos, ajMusicaMas, ajEfectosMenos, ajEfectosMas, ajTextoMenos, ajTextoMas, ajPantalla, ajCerrar;
        [Header("Créditos")]
        public GameObject creditos;
        public RectTransform creditosPanel;
        public Text creditosTexto;
        public Button creditosCerrar;
        [Header("Confirmar")]
        public GameObject confirmar;
        public RectTransform confirmarPanel;
        public Text confTitulo, confTexto;
        public Button confSi, confNo;
        Action alConfirmar;

        // Ventanas del menú abiertas: detienen el reloj del nivel y los toques del escenario.
        public bool MenuAbierto { get { return (pausa != null && pausa.activeSelf) || (partidas != null && partidas.activeSelf) || (ajustes != null && ajustes.activeSelf) || (creditos != null && creditos.activeSelf) || (confirmar != null && confirmar.activeSelf); } }

        void PrepararMenu()
        {
            Ajustes.Cargar();
            AplicarAjustes();
            if (bMenu == null) return;   // escena sin la v9 aplicada
            E.sonido = true; Sonido.Activo = true;
            bMenu.onClick.AddListener(AbrirPausa);
            bPartidas.onClick.AddListener(AbrirPartidas);
            bAjustes.onClick.AddListener(AbrirAjustes);
            bCreditos.onClick.AddListener(AbrirCreditos);
            bSalir.onClick.AddListener(Salir);
            bSalir.gameObject.SetActive(Ajustes.HayPantallaCompleta);
            pausaContinuar.onClick.AddListener(CerrarPausa);
            pausaReiniciar.onClick.AddListener(PreguntarReiniciar);
            pausaAjustes.onClick.AddListener(AbrirAjustes);
            pausaMenu.onClick.AddListener(VolverAlMenu);
            for (int i = 0; i < Estado.RANURAS; i++)
            {
                int r = i;
                ranuraJugar[i].onClick.AddListener(() => JugarRanura(r));
                ranuraNueva[i].onClick.AddListener(() => NuevaEnRanura(r));
                ranuraBorrar[i].onClick.AddListener(() => BorrarRanura(r));
            }
            partidasCerrar.onClick.AddListener(() => Ventana(partidas, false));
            ajMusicaMenos.onClick.AddListener(() => { Ajustes.Musica = Mathf.Clamp01(Ajustes.Musica - .1f); CambioAjustes(); });
            ajMusicaMas.onClick.AddListener(() => { Ajustes.Musica = Mathf.Clamp01(Ajustes.Musica + .1f); CambioAjustes(); });
            ajEfectosMenos.onClick.AddListener(() => { Ajustes.Efectos = Mathf.Clamp01(Ajustes.Efectos - .1f); CambioAjustes(); Sonido.Tocar("toque"); });
            ajEfectosMas.onClick.AddListener(() => { Ajustes.Efectos = Mathf.Clamp01(Ajustes.Efectos + .1f); CambioAjustes(); Sonido.Tocar("toque"); });
            ajTextoMenos.onClick.AddListener(() => { Ajustes.Texto = Mathf.Max(0, Ajustes.Texto - 1); CambioAjustes(); });
            ajTextoMas.onClick.AddListener(() => { Ajustes.Texto = Mathf.Min(2, Ajustes.Texto + 1); CambioAjustes(); });
            ajPantalla.onClick.AddListener(() => { Screen.fullScreen = !Screen.fullScreen; Despues(.1f, PintarAjustes); });
            ajPantalla.transform.parent.gameObject.SetActive(Ajustes.HayPantallaCompleta);
            ajCerrar.onClick.AddListener(() => Ventana(ajustes, false));
            creditosCerrar.onClick.AddListener(() => Ventana(creditos, false));
            confSi.onClick.AddListener(() => { var a = alConfirmar; alConfirmar = null; Ventana(confirmar, false); a?.Invoke(); });
            confNo.onClick.AddListener(() => { alConfirmar = null; Ventana(confirmar, false); });
            foreach (var v in new[] { pausa, partidas, ajustes, creditos, confirmar }) v.SetActive(false);
        }

        void Ventana(GameObject v, bool abrir)
        {
            v.SetActive(abrir);
            if (!abrir) return;
            var panel = v.transform.Find("Panel");
            if (panel != null) Tw.Aparecer(panel, .3f);
            Sonido.Tocar("papel");
        }

        void Confirmar(string titulo, string texto, string si, Action accion)
        {
            confTitulo.text = titulo; confTexto.text = texto;
            F.TextoBoton(confSi, si);
            alConfirmar = accion;
            Ventana(confirmar, true);
        }

        // ======================================================= Menú principal
        void PintarPortada()
        {
            if (bPartidas == null) return;
            F.TextoBoton(bJugar, E.empezada ? "Continuar" : "Jugar");
            portadaPartida.text = "Partida " + (Estado.Ranura + 1) + "  ·  " + Resumen(E);
        }

        string Resumen(Estado e)
        {
            if (e == null || !e.empezada) return "nueva";
            if (e.terminada) return "terminada · " + e.Total.ToString("N0") + " pts";
            return D.Nivel(e.nivel).nombre.Split('·')[0].Trim() + " · " + e.piezas.Count + " de 4 piezas · " + Reloj(e.TiempoTotal);
        }

        void Salir()
        {
            Guardar();
            Application.Quit();
        }

        // ======================================================= Partidas (3 ranuras)
        void AbrirPartidas()
        {
            PintarPartidas();
            Ventana(partidas, true);
        }

        void PintarPartidas()
        {
            for (int r = 0; r < Estado.RANURAS; r++)
            {
                bool hay = Estado.Existe(r);
                var e = hay ? (r == Estado.Ranura ? E : Estado.Cargar(r)) : null;
                bool empezada = e != null && e.empezada;
                ranuraTitulo[r].text = "Partida " + (r + 1) + (r == Estado.Ranura ? "  <color=#d2392f>(en uso)</color>" : "");
                ranuraInfo[r].text = !empezada ? "<i>Vacía</i>" :
                    Resumen(e) + "\n<size=15><color=#7a5a3a>" + e.Estrellas + " de 12 estrellas · guardada " + (string.IsNullOrEmpty(e.guardada) ? "—" : e.guardada) + "</color></size>";
                ranuraFondo[r].color = r == Estado.Ranura ? Pal.Hex("ffe9b8") : Pal.Hex("fbf3e3");
                ranuraJugar[r].gameObject.SetActive(empezada);
                F.TextoBoton(ranuraNueva[r], empezada ? "Empezar de nuevo" : "Nueva partida");
                ranuraBorrar[r].gameObject.SetActive(empezada);
            }
        }

        void UsarRanura(int r)
        {
            if (!modoPrueba) Guardar();
            Estado.Ranura = r;
            modoPrueba = false;
            E = Estado.Cargar(r);
            E.sonido = true; Sonido.Activo = true;
            Deseleccionar();
            limpiarDialogo();
        }

        void JugarRanura(int r)
        {
            UsarRanura(r);
            Ventana(partidas, false);
            PintarPortada();
            Jugar();
        }

        void NuevaEnRanura(int r)
        {
            Action nueva = () =>
            {
                var records = Estado.Existe(r) ? Estado.Cargar(r).records : new System.Collections.Generic.List<Record>();
                Estado.Ranura = r;
                modoPrueba = false;
                Estado.Borrar(r);
                E = new Estado { sonido = E.sonido, records = records };
                Deseleccionar();
                limpiarDialogo();
                Ventana(partidas, false);
                PintarPortada();
                Jugar();
            };
            if (Estado.Existe(r) && Estado.Cargar(r).empezada)
                Confirmar("¿Empezar de nuevo?", "Se borra el avance de la Partida " + (r + 1) + ". Los récords de cada lugar se conservan.", "Empezar de nuevo", nueva);
            else nueva();
        }

        void BorrarRanura(int r)
        {
            Confirmar("¿Borrar la Partida " + (r + 1) + "?", "Se borran su avance y sus récords. No se puede deshacer.", "Borrar", () =>
            {
                Estado.Borrar(r);
                if (r == Estado.Ranura) { E = new Estado { sonido = E.sonido }; PintarPortada(); }
                PintarPartidas();
            });
        }

        // ======================================================= Ajustes
        void AbrirAjustes() { PintarAjustes(); Ventana(ajustes, true); }

        void CambioAjustes() { Ajustes.Guardar(); AplicarAjustes(); PintarAjustes(); }

        void AplicarAjustes()
        {
            Sonido.Volumenes(Ajustes.Musica, Ajustes.Efectos);
            velocidadTexto = Ajustes.VelocidadTexto;
        }

        void PintarAjustes()
        {
            ajMusicaBarra.rectTransform.localScale = new Vector3(Mathf.Max(.001f, Ajustes.Musica), 1, 1);   // barra con pivote a la izquierda
            ajEfectosBarra.rectTransform.localScale = new Vector3(Mathf.Max(.001f, Ajustes.Efectos), 1, 1);
            ajMusicaValor.text = Mathf.RoundToInt(Ajustes.Musica * 100) + "%";
            ajEfectosValor.text = Mathf.RoundToInt(Ajustes.Efectos * 100) + "%";
            ajTextoValor.text = Ajustes.NOMBRE_TEXTO[Ajustes.Texto];
            ajPantallaValor.text = Screen.fullScreen ? "Sí" : "No";
        }

        // ======================================================= Créditos
        void AbrirCreditos()
        {
            creditosTexto.text = D.creditos;
            // el texto se encoge si no cabe en la ventana (los créditos se editan en DatosJuego)
            creditosTexto.verticalOverflow = VerticalWrapMode.Truncate;   // sin esto el ajuste automático no encoge
            creditosTexto.resizeTextForBestFit = true; creditosTexto.resizeTextMinSize = 12; creditosTexto.resizeTextMaxSize = 20;
            Ventana(creditos, true);
        }

        // ======================================================= Pausa
        void AbrirPausa()
        {
            if (modal.activeSelf || tarjeta.activeSelf || MapaAbierto || enTransicion || portada.activeSelf) return;
            var nv = D.Nivel(E.nivel);
            pausaLugar.text = nv.nombre + "\n<size=18>Tiempo del nivel " + Reloj(E.tiempo[E.nivel]) + "  ·  " + E.piezas.Count + " de 4 piezas</size>";
            bool puede = PuedeReiniciar(E.nivel);
            pausaReiniciar.interactable = puede;
            F.TextoBoton(pausaReiniciar, E.Completo(E.nivel) ? "Lugar terminado" : puede ? "Reiniciar este lugar" : "No se puede reiniciar");
            Ventana(pausa, true);
        }

        void CerrarPausa() { Ventana(pausa, false); }

        void VolverAlMenu()
        {
            Ventana(pausa, false);
            Guardar();
            limpiarDialogo();
            Deseleccionar();
            MostrarPortada();
        }

        // ======================================================= Reiniciar nivel
        // Se vuelve a la foto que se tomó al entrar por primera vez al nivel: objetos, acertijos, banderas, tiempo,
        // errores y pistas quedan como al llegar. Un nivel ya terminado no se reinicia (borraría lo que vino después).
        bool PuedeReiniciar(int n) { return !E.Completo(n) && !string.IsNullOrEmpty(E.inicio[n]); }

        void PreguntarReiniciar()
        {
            int n = E.nivel;
            if (!PuedeReiniciar(n)) return;
            Confirmar("¿Reiniciar este lugar?", "Volverás al inicio de «" + D.Nivel(n).nombre + "»: se pierden los objetos, acertijos y el tiempo de este lugar.", "Reiniciar", () => ReiniciarNivel(n));
        }

        public void ReiniciarNivel(int n)
        {
            if (!PuedeReiniciar(n)) return;
            var e = JsonUtility.FromJson<Estado>(E.inicio[n]);
            e.records = E.records;
            e.sonido = E.sonido;
            e.inicio = E.inicio;
            e.empezada = true;
            e.vista = "";
            e.Reparar();
            E = e;
            Ventana(pausa, false);
            limpiarDialogo();
            Deseleccionar();
            EntrarNivel(n, true);
            Sonido.Tocar("puerta", .9f);
        }
    }
}
