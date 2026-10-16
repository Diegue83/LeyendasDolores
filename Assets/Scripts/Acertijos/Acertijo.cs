// Base de los acertijos y fábrica. Cada acertijo dibuja su interfaz dentro del área de 720 x 430 de la ventana
// y usa una clase de Logica.cs para las reglas. Coordenadas: desde la esquina superior izquierda del área.
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Leyendas
{
    public abstract class Acertijo
    {
        public AcertijoDatos d;
        public Juego j;
        public RectTransform area;
        public int errores;
        protected bool listo;

        public abstract void Construir();
        public virtual void Actualizar(float dt) { }
        public virtual void AlPedirPista(int i) { }
        public virtual void Cerrar() { foreach (var c in corrutinas) if (c != null) j.StopCoroutine(c); }

        // ---------- Mensajes ----------
        protected void Fallo(string msg)
        {
            errores++;
            j.ContarError();
            j.Mensaje(msg, Pal.MAL);
            Sonido.Tocar("error");
            Sacudir();
        }
        protected void Aviso(string msg) { j.Mensaje(msg, Pal.PAPEL); Sonido.Tocar("toque", .8f, .6f); }
        protected void Bien(string msg) { j.Mensaje(msg, Pal.OK); }
        // Al resolver no se muestra ninguna valoración (errores, pistas, "¡perfecto!"): eso se da al terminar el nivel.
        protected void Resolver(string msg = null)
        {
            if (listo) return;
            listo = true;
            j.AcertijoResuelto(this);
        }

        // ---------- Corrutinas propias (se detienen al cerrar) ----------
        readonly System.Collections.Generic.List<Coroutine> corrutinas = new System.Collections.Generic.List<Coroutine>();
        protected Coroutine Correr(IEnumerator e) { var c = j.StartCoroutine(e); corrutinas.Add(c); return c; }

        Coroutine sac; Vector2 pos0; bool hayPos0;
        protected void Sacudir()
        {
            if (!hayPos0) { pos0 = area.anchoredPosition; hayPos0 = true; }
            if (sac != null) j.StopCoroutine(sac);
            area.anchoredPosition = pos0;
            sac = j.StartCoroutine(SacudirC());
        }
        IEnumerator SacudirC()
        {
            float t = 0;
            while (t < .4f)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / .4f);
                if (area) area.anchoredPosition = pos0 + new Vector2(Mathf.Sin(k * Mathf.PI * 8) * 10 * (1 - k), 0);
                yield return null;
            }
            if (area) area.anchoredPosition = pos0;
        }

        // ---------- Utilidades de construcción ----------
        protected static int[] Enteros(string s) { return s.Split(',').Select(x => int.Parse(x.Trim())).ToArray(); }

        // Rectángulo con su esquina superior izquierda en (x, y).
        protected Image Imagen(string sprite, float x, float y, float w, float h, Color? c = null, Transform padre = null, bool raycast = false)
        {
            var im = F.Img(sprite ?? "Imagen", padre ?? area, sprite, c, raycast);
            F.Pos(im.rectTransform, x, y, w, h);
            if (sprite != null && !sprite.StartsWith("UI/")) im.preserveAspect = true;
            return im;
        }

        // Rectángulo centrado en (cx, cy).
        protected static RectTransform PosC(RectTransform rt, float cx, float cy, float w, float h)
        {
            if (F.EsDelPrefab(rt)) return rt;
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(.5f, .5f);
            rt.anchoredPosition = new Vector2(cx, -cy);
            rt.sizeDelta = new Vector2(w, h);
            return rt;
        }

        protected Text Texto(string s, float x, float y, float w, float h, int tam, Color c, TextAnchor a = TextAnchor.MiddleCenter, int estilo = 0, Transform padre = null)
        {
            var t = F.Txt("Texto", padre ?? area, s, tam, c, a, estilo);
            F.Pos(t.rectTransform, x, y, w, h);
            return t;
        }

        protected Button Boton(string texto, float x, float y, float w, float h, Color c, UnityAction a, string icono = null, int tam = 22)
        {
            var b = F.Btn(string.IsNullOrEmpty(texto) ? "Boton" : texto, area, texto, c, a, icono, tam);
            F.Pos((RectTransform)b.transform, x, y, w, h);
            return b;
        }

        // Vuelve tocable cualquier imagen.
        protected static Button Tocable(Graphic g, UnityAction a)
        {
            g.raycastTarget = true;
            var b = F.Asegurar<Button>(g.gameObject);
            b.transition = Selectable.Transition.None;
            b.onClick.AddListener(a);
            F.Asegurar<Pulsar>(g.gameObject).escala = .95f;
            return b;
        }
    }

    public static class Acertijos
    {
        public static Acertijo Crear(string id)
        {
            switch (id)
            {
                case "candado": return new AcRuedas("candado");
                case "criptex": return new AcRuedas("criptex");
                case "simbolos": return new AcRuedas("simbolos");
                case "deslizante": return new AcDeslizante();
                case "anillos": return new AcAnillos();
                case "candelabro": return new AcCandelabro();
                case "campanas": return new AcSimon();
                case "nieves": return new AcNieves();
                case "nonograma": return new AcNonograma();
                case "jarras": return new AcJarras();
                case "cronologia": return new AcOrden(false);
                case "libros": return new AcOrden(true);
                case "canal": return new AcCanal();
                case "pinata": return new AcPinata();
                case "balanza": return new AcBalanza();
                case "hanoi": return new AcHanoi();
                case "cesar": return new AcCesar();
                case "caja": return new AcCaja();
                case "ruta": return new AcRuta();
                case "caso": return new AcCaso();
                case "final": return new AcFinal();
            }
            return null;
        }
    }
}
