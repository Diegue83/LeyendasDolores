// Paleta, carga de arte y fábrica de piezas de interfaz (uGUI). Todo lo que se crea aquí aparece en la Hierarchy.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Leyendas
{
    // Paleta de la interfaz v4 (arte folk mexicano).
    public static class Pal
    {
        public static readonly Color NOCHE = Hex("2a1416"), NOCHE2 = Hex("3a1c1e"), NOCHE3 = Hex("9a3a2a"), BORDE = Hex("7a4a3a");
        public static readonly Color PAPEL = Hex("fff1dc"), PAPEL2 = Hex("ecc9a0"), MAIZ = Hex("f6c739"), ROSA = Hex("e8357a");
        public static readonly Color TURQUESA = Hex("1fb3a9"), CHILE = Hex("d2392f"), OK = Hex("7ad08a"), MAL = Hex("ff7a6b");
        public static readonly Color CAFE = Hex("3a2618"), VERDE_OK = Hex("2f6a4a"), ELEGIDO = Hex("8a3a2a"), NARANJA = Hex("f39a1f"), AZUL = Hex("1f6aa0");
        public static readonly Color[] FOLK = { ROSA, NARANJA, TURQUESA, MAIZ, Hex("5c9a3f"), Hex("9b4fd1") };

        public static Color Hex(string h)
        {
            Color c; ColorUtility.TryParseHtmlString("#" + h, out c); return c;
        }
        public static Color A(Color c, float a) { c.a = a; return c; }
    }

    // Carga de sprites desde Resources/Arte (con caché).
    public static class Arte
    {
        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();
        static readonly Dictionary<string, Texture2D> texturas = new Dictionary<string, Texture2D>();

        public static Sprite S(string ruta)
        {
            if (string.IsNullOrEmpty(ruta)) return null;
            Sprite s;
            if (cache.TryGetValue(ruta, out s) && s != null) return s;
            s = Resources.Load<Sprite>("Arte/" + ruta);
            if (s == null)
            {
                // Si la imagen no se importó como Sprite, se crea uno al vuelo desde la textura.
                var t = T(ruta);
                if (t != null) s = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(.5f, .5f), 100f);
            }
            cache[ruta] = s;
            return s;
        }

        public static Texture2D T(string ruta)
        {
            Texture2D t;
            if (texturas.TryGetValue(ruta, out t) && t != null) return t;
            t = Resources.Load<Texture2D>("Arte/" + ruta);
            texturas[ruta] = t;
            return t;
        }

        public static void Limpiar() { cache.Clear(); texturas.Clear(); }
    }

    // Fábrica de objetos de interfaz. Coordenadas en "puntos de la interfaz" (lienzo de 1280 x 720),
    // medidas desde la esquina superior izquierda del padre.
    //
    // Plantillas (versión nativa): mientras se arma un acertijo desde su prefab (F.Usar), cada pieza que la lógica
    // pide se toma del prefab en orden en lugar de crearse, y no se le cambia posición, tamaño, sprite ni color:
    // lo que edites en el prefab se respeta. F.Grabar solo lo usa la herramienta de migración al crear el prefab.
    public static class F
    {
        static PlantillaUI grabando, usando;
        static int siguiente;
        static readonly HashSet<Transform> reutilizados = new HashSet<Transform>();

        public static void Grabar(PlantillaUI p) { grabando = p; usando = null; reutilizados.Clear(); }
        public static void Usar(PlantillaUI p) { usando = p; grabando = null; siguiente = 0; reutilizados.Clear(); }
        public static void Terminar() { grabando = null; usando = null; reutilizados.Clear(); }
        public static int PiezasUsadas { get { return siguiente; } }
        // true si la pieza viene del prefab (su diseño no se toca mientras se arma el acertijo).
        public static bool EsDelPrefab(Transform t) { return usando != null && t != null && reutilizados.Contains(t); }

        static RectTransform DelPrefab()
        {
            if (usando == null) return null;
            if (siguiente >= usando.elementos.Count) { siguiente++; return null; }
            var rt = usando.elementos[siguiente++];
            if (rt != null) reutilizados.Add(rt);
            return rt;
        }

        static Font titulo, texto, textoB;
        public static Font Titulo { get { Fuentes(); return titulo; } }
        public static Font Texto { get { Fuentes(); return texto; } }
        public static Font TextoB { get { Fuentes(); return textoB; } }

        static void Fuentes()
        {
            if (titulo != null) return;
            titulo = Resources.Load<Font>("Fuentes/LilitaOne-Regular");
            texto = Resources.Load<Font>("Fuentes/AtkinsonHyperlegible-Regular");
            textoB = Resources.Load<Font>("Fuentes/AtkinsonHyperlegible-Bold");
            var defecto = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (titulo == null) titulo = defecto;
            if (texto == null) texto = defecto;
            if (textoB == null) textoB = texto;
        }

        public static RectTransform Nodo(string nombre, Transform padre)
        {
            var previo = DelPrefab();
            if (previo != null) return previo;
            var go = new GameObject(nombre, typeof(RectTransform));
            go.layer = 5; // UI
            var rt = (RectTransform)go.transform;
            rt.SetParent(padre, false);
            if (grabando != null) grabando.elementos.Add(rt);
            return rt;
        }

        // Posición desde la esquina superior izquierda del padre.
        public static RectTransform Pos(RectTransform rt, float x, float y, float w, float h)
        {
            if (EsDelPrefab(rt)) return rt;
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(w, h);
            return rt;
        }

        // Centrado en el padre, con desplazamiento (x a la derecha, y hacia abajo).
        public static RectTransform Centro(RectTransform rt, float x, float y, float w, float h)
        {
            if (EsDelPrefab(rt)) return rt;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(.5f, .5f);
            rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(w, h);
            return rt;
        }

        public static RectTransform Llenar(RectTransform rt, float margen = 0)
        {
            if (EsDelPrefab(rt)) return rt;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.pivot = new Vector2(.5f, .5f);
            rt.offsetMin = new Vector2(margen, margen); rt.offsetMax = new Vector2(-margen, -margen);
            return rt;
        }

        public static void Pivote(RectTransform rt, Vector2 pivote)
        {
            if (EsDelPrefab(rt)) return;
            // Cambia el pivote sin mover el rectángulo en pantalla.
            var tam = rt.rect.size; var d = pivote - rt.pivot;
            rt.pivot = pivote;
            rt.anchoredPosition += new Vector2(d.x * tam.x, d.y * tam.y);
        }

        public static Image Img(string nombre, Transform padre, string sprite, Color? color = null, bool raycast = false)
        {
            var rt = Nodo(nombre, padre);
            if (EsDelPrefab(rt)) return Asegurar<Image>(rt.gameObject);
            var im = rt.gameObject.AddComponent<Image>();
            Sprite(im, sprite);
            im.color = color ?? Color.white;
            im.raycastTarget = raycast;
            return im;
        }

        // Asigna el sprite y, si tiene bordes 9-slice, lo pone en modo Sliced con el tamaño correcto.
        public static void Sprite(Image im, string ruta)
        {
            var s = Arte.S(ruta);
            im.sprite = s;
            if (s != null && s.border.sqrMagnitude > 0)
            {
                im.type = Image.Type.Sliced;
                im.fillCenter = true;
                // El arte de interfaz está al doble de resolución: los bordes se ven a la mitad.
                im.pixelsPerUnitMultiplier = s.pixelsPerUnit >= 199f ? 1f : 2f;
            }
            else im.type = Image.Type.Simple;
        }

        public static Text Txt(string nombre, Transform padre, string s, int tam, Color color, TextAnchor ali = TextAnchor.MiddleCenter, int estilo = 0)
        {
            var rt = Nodo(nombre, padre);
            if (EsDelPrefab(rt))
            {
                // Del prefab se respeta la apariencia (fuente, tamaño, color, lugar); el contenido sale de DatosJuego,
                // para que un cambio en las pistas o textos de un acertijo no se quede con el texto viejo del prefab.
                var tp = Asegurar<Text>(rt.gameObject);
                if (s != null) tp.text = s;
                return tp;
            }
            var t = rt.gameObject.AddComponent<Text>();
            t.font = estilo == 1 ? Titulo : estilo == 2 ? TextoB : Texto;
            t.text = s; t.fontSize = tam; t.color = color; t.alignment = ali;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.supportRichText = true;
            t.raycastTarget = false;
            if (estilo == 1)
            {
                var sh = rt.gameObject.AddComponent<Shadow>();
                sh.effectColor = Pal.A(Color.black, .45f); sh.effectDistance = new Vector2(0, -2);
            }
            return t;
        }

        // Botón con fondo teñible (UI/boton), texto opcional e ícono opcional.
        public static Button Btn(string nombre, Transform padre, string texto, Color color, UnityAction accion, string icono = null, int tam = 22)
        {
            var im = Img(nombre, padre, "UI/boton", color, true);
            if (EsDelPrefab(im.transform))
            {
                // Del prefab: solo se conecta la acción; ícono y texto se piden igual para no desfasar el orden.
                var bp = Asegurar<Button>(im.gameObject);
                if (accion != null) bp.onClick.AddListener(accion);
                if (!string.IsNullOrEmpty(icono)) Img("Icono", im.transform, icono, Pal.PAPEL);
                if (!string.IsNullOrEmpty(texto)) Txt("Texto", im.transform, texto, tam, Pal.PAPEL, TextAnchor.MiddleCenter, 1);
                return bp;
            }
            var b = im.gameObject.AddComponent<Button>();
            var cb = b.colors;
            cb.normalColor = Color.white; cb.highlightedColor = new Color(1, 1, .92f); cb.pressedColor = new Color(.82f, .78f, .74f);
            cb.disabledColor = new Color(.6f, .6f, .6f, .6f); cb.fadeDuration = .08f;
            b.colors = cb;
            b.targetGraphic = im;
            if (accion != null) b.onClick.AddListener(accion);
            b.gameObject.AddComponent<SonidoBoton>();
            b.gameObject.AddComponent<Pulsar>();
            if (!string.IsNullOrEmpty(icono))
            {
                var ic = Img("Icono", im.transform, icono, Pal.PAPEL);
                Llenar(ic.rectTransform, 10);
                ic.preserveAspect = true;
                if (!string.IsNullOrEmpty(texto))
                {
                    ic.rectTransform.anchorMin = new Vector2(0, 0); ic.rectTransform.anchorMax = new Vector2(0, 1);
                    ic.rectTransform.pivot = new Vector2(0, .5f);
                    ic.rectTransform.offsetMin = new Vector2(10, 9); ic.rectTransform.offsetMax = new Vector2(38, -9);
                }
            }
            if (!string.IsNullOrEmpty(texto))
            {
                var t = Txt("Texto", im.transform, texto, tam, Pal.PAPEL, TextAnchor.MiddleCenter, 1);
                Llenar(t.rectTransform, 4);
                if (!string.IsNullOrEmpty(icono)) t.rectTransform.offsetMin = new Vector2(40, 4);
            }
            return b;
        }

        public static void TextoBoton(Button b, string s)
        {
            var t = b.GetComponentInChildren<Text>(true);
            if (t != null) t.text = s;
        }

        public static T Asegurar<T>(GameObject g) where T : Component
        {
            var c = g.GetComponent<T>();
            if (c == null) c = g.AddComponent<T>();
            return c;
        }

        public static void Borrar(Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--)
            {
                var c = t.GetChild(i).gameObject;
                if (Application.isPlaying) Object.Destroy(c); else Object.DestroyImmediate(c);
            }
        }
    }
}
