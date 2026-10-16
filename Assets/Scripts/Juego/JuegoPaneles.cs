// Ventana de acertijos (reloj del nivel, pistas, resultado) y tarjetas (mapa, diario, fin de nivel, final).
// La puntuación es POR NIVEL: el reloj corre todo el tiempo que se juega un lugar (explorando y en los acertijos)
// y al terminar el nivel se calculan estrellas y puntos con ese tiempo, los errores y las pistas pedidas.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Leyendas
{
    public partial class Juego
    {
        // ======================================================= Acertijos
        Acertijo acertijo;
        Action alResolver;
        int pistasDadas;
        bool acertijoTerminado;

        public void AbrirAcertijo(string id, Action resuelto)
        {
            var a = Acertijos.Crear(id);
            if (a == null) { Debug.LogWarning("Leyendas: acertijo desconocido " + id); return; }
            Deseleccionar();
            acertijo = a; alResolver = resuelto;
            a.d = D.Acertijo(id); a.j = this;
            F.Borrar(modalContenido);
            modalContenido.anchoredPosition = new Vector2(40, -158);
            // Las piezas del acertijo vienen de su prefab (Assets/Resources/Prefabs/Acertijos); la lógica se engancha a ellas.
            var prefab = Resources.Load<GameObject>("Prefabs/Acertijos/Acertijo_" + id);
            PlantillaUI plantilla = null;
            if (prefab != null)
            {
                var inst = Instantiate(prefab, modalContenido, false);
                inst.name = prefab.name;
                plantilla = inst.GetComponent<PlantillaUI>();
                a.area = (RectTransform)inst.transform;
            }
            else
            {
                Debug.LogWarning("Leyendas: falta el prefab del acertijo " + id + "; se arma con la plantilla por código.");
                a.area = modalContenido;
            }
            modalTitulo.text = a.d.titulo;
            modalInstr.text = a.d.instrucciones;
            modalMensaje.text = "";
            pistasDadas = 0; acertijoTerminado = false;
            modalPar.text = "Tiempo del nivel  ·  par " + Reloj(ParNivel(E.nivel));
            ActualizarBotonPista();
            resultado.SetActive(false);
            modal.SetActive(true);
            Tw.Aparecer(modalPanel, .38f);
            Sonido.Tocar("papel");
            if (plantilla != null) F.Usar(plantilla);
            try { a.Construir(); }
            finally
            {
                if (plantilla != null && F.PiezasUsadas != plantilla.elementos.Count)
                    Debug.LogWarning("Leyendas: el acertijo " + id + " pidió " + F.PiezasUsadas + " piezas y su prefab tiene " + plantilla.elementos.Count + ".");
                F.Terminar();
            }
        }

        void Update()
        {
            if (E == null) return;
            if (Cronometrando) { E.tiempo[E.nivel] += Time.unscaledDeltaTime; PintarReloj(); }
            if (acertijo != null && !acertijoTerminado && modal.activeSelf)
            {
                float t = E.tiempo[E.nivel], par = ParNivel(E.nivel);
                modalReloj.text = Reloj(t);
                modalReloj.color = t <= par ? Pal.PAPEL : t <= par * 1.5f ? Pal.MAIZ : Pal.MAL;
                acertijo.Actualizar(Time.unscaledDeltaTime);
            }
        }

        // ======================================================= Puntuación por nivel
        // El reloj corre mientras se juega el nivel actual (no en la portada, ni con el mapa o el diario abiertos).
        bool Cronometrando { get { return E.empezada && !portada.activeSelf && !tarjeta.activeSelf && !MapaAbierto && !MenuAbierto && !E.Completo(E.nivel); } }

        // Tiempo par del nivel: el de DatosJuego o, si es 0, la suma de los acertijos más tiempo para explorar.
        public float ParNivel(int n)
        {
            float p = D.Nivel(n).tiempoPar;
            return p > 0 ? p : AcertijosDe(n).Sum(id => D.Acertijo(id).tiempoPar) * 1.5f + 240f;
        }

        public void ContarError() { if (!E.Completo(E.nivel)) E.errores[E.nivel]++; }

        Record ultimoNivel; bool ultimoRecord;

        void CompletarNivel(int n)
        {
            if (E.Completo(n)) return;
            E.completos.Add(n);
            float par = ParNivel(n);
            var r = new Record
            {
                id = D.Nivel(n).id, segundos = E.tiempo[n], errores = E.errores[n], pistas = E.pistas[n],
                estrellas = Logica.Puntaje.EstrellasNivel(E.tiempo[n], par, E.pistas[n]),
                puntos = Logica.Puntaje.PuntosNivel(E.tiempo[n], par, E.errores[n], E.pistas[n], D.costoPista)
            };
            ultimoNivel = r;
            ultimoRecord = E.Registrar(r);
            E.nivelMax = Mathf.Max(E.nivelMax, Mathf.Min(3, n + 1));
            Guardar();
            ActualizarHUD();
        }

        static string Estrellitas(int e) { return "<color=#d99a1f>" + new string('*', e) + "</color><color=#c8b8a0>" + new string('*', 3 - e) + "</color>"; }

        public static string Reloj(float s) { int t = Mathf.FloorToInt(s); return (t / 60) + ":" + (t % 60).ToString("00"); }

        void ActualizarBotonPista()
        {
            int n = acertijo != null ? acertijo.d.pistas.Length : 0;
            modalPista.interactable = pistasDadas < n;
            F.TextoBoton(modalPista, pistasDadas < n ? "Pista (−" + D.costoPista + ")" : "Sin pistas");
        }

        void PedirPista()
        {
            if (acertijo == null || acertijoTerminado || pistasDadas >= acertijo.d.pistas.Length) return;
            var p = acertijo.d.pistas[pistasDadas];
            pistasDadas++;
            if (!E.Completo(E.nivel)) E.pistas[E.nivel]++;
            Mensaje("Pista " + pistasDadas + ": " + p, Pal.MAIZ);
            acertijo.AlPedirPista(pistasDadas - 1);
            ActualizarBotonPista();
            DecirTexto("pista", "", p);
        }

        public void Mensaje(string s, Color c)
        {
            modalMensaje.text = s;
            modalMensaje.color = c;
            Tw.Brinco(modalMensaje.transform, .08f, .25f);
        }

        public void AcertijoResuelto(Acertijo a)
        {
            if (a != acertijo || acertijoTerminado) return;
            acertijoTerminado = true;
            if (!E.Completo(E.nivel))
            {
                var r = E.acertijos.Find(x => x.id == a.d.id);
                if (r == null) { r = new Record { id = a.d.id }; E.acertijos.Add(r); }
                r.errores = a.errores; r.pistas = pistasDadas;
            }
            StartCoroutine(CerrarResuelto());
        }

        // Sin pantalla de resultado: el acertijo se cierra solo y la historia sigue.
        // Errores, pistas y estrellas se muestran juntos al terminar el nivel.
        IEnumerator CerrarResuelto()
        {
            Sonido.Tocar("acierto");
            yield return new WaitForSecondsRealtime(.8f);
            if (acertijoTerminado && modal.activeSelf) CerrarAcertijo(true);
        }

        public void CerrarAcertijo(bool resuelto)
        {
            if (acertijo != null) acertijo.Cerrar();
            var cb = resuelto ? alResolver : null;
            acertijo = null; alResolver = null;
            StartCoroutine(CerrarModal(cb));
        }

        IEnumerator CerrarModal(Action cb)
        {
            yield return Tw.Escala(modalPanel, Vector3.one, Vector3.one * .85f, .15f);
            modal.SetActive(false);
            modalPanel.localScale = Vector3.one;
            F.Borrar(modalContenido);
            ActualizarHUD();
            cb?.Invoke();
            Guardar();
        }

        public bool AcertijoAbierto { get { return modal.activeSelf; } }

        // ======================================================= Tarjetas
        void AbrirTarjeta(string titulo, string texto, Sprite imagen, params KeyValuePair<string, Action>[] botones)
        {
            tarjetaTitulo.text = titulo;
            tarjetaTexto.text = texto;
            tarjetaImagen.gameObject.SetActive(imagen != null);
            tarjetaImagen.sprite = imagen;
            var sv = (RectTransform)tarjetaScroll.transform;
            F.Pos(sv, imagen != null ? 290 : 40, 112, imagen != null ? 670 : 920, 470);
            F.Borrar(tarjetaBotones);
            foreach (var b in botones)
            {
                var accion = b.Value;
                var btn = F.Btn("Boton", tarjetaBotones, b.Key, botones.Length > 1 && b.Key == "Cerrar" ? Pal.NOCHE3 : Pal.ROSA, () => { CerrarTarjeta(); accion?.Invoke(); }, null, 22);
                var le = btn.gameObject.AddComponent<LayoutElement>();
                le.preferredWidth = Mathf.Max(200, b.Key.Length * 15 + 60); le.preferredHeight = 58;
            }
            tarjeta.SetActive(true);
            tarjetaScroll.verticalNormalizedPosition = 1;
            Tw.Aparecer(tarjetaPanel, .4f);
            if (imagen != null) Tw.Aparecer(tarjetaImagen.transform, .5f, .2f);
            Sonido.Tocar("papel");
        }

        void CerrarTarjeta() { tarjeta.SetActive(false); }

        static KeyValuePair<string, Action> Op(string t, Action a) { return new KeyValuePair<string, Action>(t, a); }

        // Resultado del nivel n: tiempo, errores, pistas, estrellas y puntos (o el avance si sigue en curso).
        string ResumenNivel(int n)
        {
            var sb = new StringBuilder();
            var ids = AcertijosDe(n);
            int hechos = ids.Count(id => B("ok_" + id));
            float par = ParNivel(n);
            var r = E.DePartida(D.Nivel(n).id); var mejor = E.Mejor(D.Nivel(n).id);
            if (r != null)
            {
                sb.Append("<b>Resultado</b>  <size=28>" + Estrellitas(r.estrellas) + "</size>   <b>" + r.puntos.ToString("N0") + " pts</b>\n");
                sb.Append("Tiempo " + Reloj(r.segundos) + " (par " + Reloj(par) + ")  ·  Errores " + r.errores + "  ·  Pistas " + r.pistas + "\n");
            }
            else if (n <= E.nivelMax)
                sb.Append("<b>En curso</b>  ·  <i>el resultado se verá al terminar este lugar</i>\n");
            if (mejor != null) sb.Append("<color=#1f6aa0>Récord: " + mejor.puntos.ToString("N0") + " pts  ·  " + mejor.estrellas + " estrellas  ·  " + Reloj(mejor.segundos) + "</color>\n");
            sb.Append("Acertijos resueltos: " + hechos + " de " + ids.Length + "\n");
            foreach (var id in ids)
            {
                sb.Append((B("ok_" + id) ? "<color=#2f6a4a>   [listo]</color>  " : "<color=#a08070>   [ -- ]</color>  ") + D.Acertijo(id).titulo);
                var a = r != null ? E.acertijos.Find(x => x.id == id) : null;   // el detalle solo con el nivel terminado
                if (a != null) sb.Append("<color=#7a5a3a>  ·  errores " + a.errores + "  ·  pistas " + a.pistas + "</color>");
                sb.Append("\n");
            }
            return sb.ToString();
        }

        void TarjetaNivel(int n)
        {
            var nv = D.Nivel(n);
            var r = E.DePartida(nv.id);
            var t = "<size=24><b>" + nv.nombre + "</b></size>\n" + nv.cierre + "\n\n" +
                    (r != null && r == ultimoNivel && ultimoRecord ? "<color=#d2392f><b>¡Nuevo récord en este lugar!</b></color>\n" : "") +
                    ResumenNivel(n) +
                    "\n<i>Puntuación de la partida: " + E.Total.ToString("N0") + " pts · " + E.Estrellas + " de 12 estrellas</i>";
            Confeti.Lluvia(confeti, new Vector2(0, -100), 80);
            if (r != null) for (int i = 0; i < r.estrellas; i++) { int k = i; Despues(.5f + k * .3f, () => Sonido.Tocar("estrella", 1f + k * .18f)); }
            AbrirTarjeta("¡Encontraste " + nv.nombrePieza + "!", t, Arte.S("Objetos/" + nv.pieza),
                Op("Siguiente lugar", () => { E.nivelMax = Mathf.Max(E.nivelMax, n + 1); EntrarNivel(n + 1, true); }));
        }

        // ======================================================= Mapa
        // Ventana propia en Juego.unity (Hierarchy: Pantalla > Mapa): ilustración del centro de Dolores con un
        // marcador por lugar. Aquí solo se pinta el estado de cada marcador y la ficha del lugar elegido.
        static readonly string[] CORTO = { "Parroquia", "Plaza Principal", "Mercado Hidalgo", "Casa de Hidalgo" };
        int mapaSel;
        public bool MapaAbierto { get { return mapa != null && mapa.activeSelf; } }

        void MostrarMapa()
        {
            if (modal.activeSelf || enTransicion) return;
            if (tarjeta.activeSelf) CerrarTarjeta();
            mapa.SetActive(true);
            Tw.Aparecer(mapaPanel, .4f);
            Sonido.Tocar("papel");
            PintarMapa();
            ElegirLugar(E.nivel);
        }

        void CerrarMapa() { mapa.SetActive(false); }

        void PintarMapa()
        {
            for (int i = 0; i < 4; i++)
            {
                var nv = D.Nivel(i);
                bool hecho = E.piezas.Contains(nv.pieza), abierto = i <= E.nivelMax, aqui = i == E.nivel;
                mapaMarcas[i].color = hecho ? Pal.MAIZ : aqui ? Pal.ROSA : abierto ? Pal.TURQUESA : Pal.A(Pal.BORDE, .9f);
                var r = E.DePartida(nv.id);
                mapaEtiquetas[i].text = "<b>" + CORTO[i] + "</b>\n" +
                    (aqui ? "Estás aquí" : r != null ? Estrellitas(r.estrellas) : hecho ? "listo" : abierto ? "abierto" : "bloqueado");
                var flota = mapaMarcas[i].GetComponent<Flotar>();
                if (flota != null) flota.enabled = aqui;
                mapaLugares[i].interactable = true;
            }
        }

        void ElegirLugar(int i)
        {
            mapaSel = i;
            var nv = D.Nivel(i);
            bool hecho = E.piezas.Contains(nv.pieza), abierto = i <= E.nivelMax, aqui = i == E.nivel;
            for (int k = 0; k < 4; k++) mapaLugares[k].transform.localScale = Vector3.one * (k == i ? 1.18f : 1f);
            mapaNombre.text = (i + 1) + ". " + nv.nombre;
            var sb = new StringBuilder();
            var r = E.DePartida(nv.id);
            if (!abierto) sb.Append("<color=#a08070><b>Bloqueado.</b></color>\nSe abre al encontrar " + D.Nivel(i - 1).nombrePieza + ".");
            else
            {
                sb.Append(aqui ? "<color=#d2392f><b>Estás aquí.</b></color>\n" : hecho ? "<color=#2f6a4a><b>Encontraste " + nv.nombrePieza + ".</b></color>\n" : "");
                if (r != null) sb.Append("<size=26>" + Estrellitas(r.estrellas) + "</size>  " + r.puntos.ToString("N0") + " pts\nTiempo " + Reloj(r.segundos) + "\n");
                else if (!string.IsNullOrEmpty(nv.objetivo)) sb.Append("\n" + nv.objetivo + "\n");
                var mejor = E.Mejor(nv.id);
                if (mejor != null) sb.Append("\n<color=#1f6aa0>Récord: " + mejor.puntos.ToString("N0") + " pts</color>");
            }
            mapaInfo.text = sb.ToString();
            mapaViajar.interactable = abierto && !aqui;
            F.TextoBoton(mapaViajar, !abierto ? "Bloqueado" : aqui ? "Estás aquí" : "Viajar aquí");
            Sonido.Tocar("tick", 1f + i * .08f, .6f);
        }

        void ViajarAlLugar()
        {
            int k = mapaSel;
            if (k > E.nivelMax || k == E.nivel) return;
            CerrarMapa();
            EntrarNivel(k, false);
        }

        void MostrarDiario()
        {
            if (modal.activeSelf || enTransicion) return;
            var nv = D.Nivel(E.nivel);
            var sb = new StringBuilder();
            sb.Append("<b>Objetivo</b>\n" + nv.objetivo + "\n\n<b>Pistas anotadas</b>\n");
            int n = 0;
            foreach (var p in NotasDiario()) { sb.Append("• " + p + "\n"); n++; }
            if (n == 0) sb.Append("<i>Todavía nada. Explora y toca todo lo que te llame la atención.</i>\n");
            var caso = NotasCaso().ToList();
            if (caso.Count > 0) { sb.Append("\n<b>El caso de la campana robada</b>\n"); foreach (var c in caso) sb.Append("• " + c + "\n"); }
            sb.Append("\n" + ResumenNivel(E.nivel));
            sb.Append("\n<b>Partida</b>: " + E.Total.ToString("N0") + " pts · " + E.Estrellas + " de 12 estrellas · piezas " + E.piezas.Count + "/4");
            AbrirTarjeta("Diario de pistas de Ana", sb.ToString(), null, Op("Cerrar", null));
        }

        void FinalDelJuego()
        {
            E.terminada = true; Guardar();
            Confeti.Lluvia(confeti, new Vector2(0, -100), 140);
            Sonido.Tocar("campana_grande");
            Despues(1.2f, () => Sonido.Tocar("campana_mediana"));
            Despues(2.2f, () => Sonido.Tocar("campana_chica"));
            var sb = new StringBuilder();
            sb.Append("La campana del Grito vuelve a sonar en Dolores Hidalgo, cuna de la Independencia.\n\n");
            float tTotal = 0;
            for (int i = 0; i < 4; i++)
            {
                var r = E.DePartida(D.Nivel(i).id);
                tTotal += E.tiempo[i];
                sb.Append("<b>" + (i + 1) + ". " + D.Nivel(i).nombre + "</b>   ");
                sb.Append(r != null ? Estrellitas(r.estrellas) + "  " + r.puntos.ToString("N0") + " pts  ·  " + Reloj(r.segundos) + "\n" : "sin terminar\n");
            }
            sb.Append("\n<size=26><b>Puntuación final: " + E.Total.ToString("N0") + " pts · " + E.Estrellas + " de 12 estrellas</b></size>\n");
            sb.Append("Tiempo total: " + Reloj(tTotal) + "\n");
            sb.Append("<i>Puedes volver a cualquier lugar desde el Mapa. Los récords se guardan.</i>");
            Decir("final", () => AbrirTarjeta("¡Leyenda resuelta!", sb.ToString(), Arte.S("Acertijos/campana1"), Op("Ver el mapa", MostrarMapa), Op("Cerrar", null)));
        }
    }
}
