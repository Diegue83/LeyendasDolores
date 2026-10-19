// Acertijos de ruedas (candado, criptex, símbolos) y los del nivel 1: cuadro deslizante, anillos, candelabro y campanas.
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Leyendas
{
    // ---------- Ruedas: candado romano (números), criptex (letras), candado de símbolos (dibujos) ----------
    public class AcRuedas : Acertijo
    {
        readonly string modo;
        Logica.Ruedas r;
        Text[] letras; Image[] dibujos; RectTransform[] caras;
        public AcRuedas(string modo) { this.modo = modo; }

        public override void Construir()
        {
            string[] alf, resp;
            if (modo == "candado") { alf = Logica.Ruedas.Letras("0123456789"); resp = Logica.Ruedas.Letras(d.respuesta); }
            else if (modo == "criptex") { alf = Logica.Ruedas.Letras("ABCDEFGHIJKLMNÑOPQRSTUVWXYZ"); resp = Logica.Ruedas.Letras(d.respuesta.ToUpperInvariant()); }
            else { alf = d.opciones; resp = d.respuesta.Split(',').Select(s => s.Trim()).ToArray(); }
            r = new Logica.Ruedas(alf, resp);
            int n = resp.Length;
            float w = modo == "candado" ? 112 : modo == "criptex" ? 90 : 140, gap = modo == "criptex" ? 8 : 18;
            float total = n * w + (n - 1) * gap, x0 = (720 - total) / 2;
            if (modo == "criptex") Imagen("Acertijos/criptex_cuerpo", x0 - 90, 104, total + 180, 200);
            else Imagen("UI/panel", x0 - 26, 34, total + 52, 300, Pal.A(Color.white, .9f));
            letras = new Text[n]; dibujos = new Image[n]; caras = new RectTransform[n];
            for (int i = 0; i < n; i++)
            {
                float x = x0 + i * (w + gap);
                int k = i;
                Boton("", x + w / 2 - 34, 42, 68, 48, Pal.TURQUESA, () => Girar(k, -1), "Iconos/arriba");
                var cara = modo == "criptex" ? Imagen("Acertijos/criptex_anillo", x, 112, w, 170) : Imagen("UI/ranura", x, 100, w, 170);
                cara.preserveAspect = false;
                caras[i] = cara.rectTransform;
                if (modo == "simbolos")
                {
                    dibujos[i] = Imagen(null, 10, 25, w - 20, w - 20, null, cara.transform);
                    dibujos[i].preserveAspect = true;
                }
                else letras[i] = Texto("", 0, 0, w, 170, modo == "criptex" ? 58 : 76, modo == "criptex" ? Pal.Hex("ffe28a") : Pal.PAPEL, TextAnchor.MiddleCenter, 1, cara.transform);
                Boton("", x + w / 2 - 34, 282, 68, 48, Pal.TURQUESA, () => Girar(k, 1), "Iconos/abajo");
                if (modo == "criptex") Texto((i + 1).ToString(), x, 4, w, 30, 18, Pal.PAPEL2, TextAnchor.MiddleCenter, 1);
                Pintar(i);
            }
            if (modo == "candado")
            {
                Texto("DÍA", x0, 4, 2 * w + gap, 30, 20, Pal.MAIZ, TextAnchor.MiddleCenter, 1);
                Texto("MES", x0 + 2 * (w + gap), 4, 2 * w + gap, 30, 20, Pal.MAIZ, TextAnchor.MiddleCenter, 1);
            }
            Boton(modo == "criptex" ? "Abrir criptex" : "Abrir", 250, 360, 220, 60, Pal.ROSA, Probar, null, 24);
        }

        void Pintar(int i)
        {
            var s = r.Simbolo(i);
            if (letras[i] != null) letras[i].text = s;
            if (dibujos[i] != null) dibujos[i].sprite = Arte.S("Acertijos/simbolo_" + s);
        }

        void Girar(int i, int paso)
        {
            if (listo) return;
            r.Girar(i, paso);
            Pintar(i);
            Sonido.Tocar("tick", 1f + i * .05f);
            var c = caras[i];
            j.StartCoroutine(Tw.Valor(.16f, k => { if (c) c.localScale = new Vector3(1, .8f + .2f * k, 1); }));
        }

        void Probar()
        {
            if (listo) return;
            if (r.Resuelto) { Sonido.Tocar("puerta", 1.3f); Resolver(); }
            else Fallo(modo == "criptex" ? "El criptex no se abre." : "No abre.");
        }
    }

    // ---------- Cuadro deslizante 3 x 3 ----------
    public class AcDeslizante : Acertijo
    {
        Logica.Deslizante z;
        RawImage[] piezas; Text[] numeros;
        Text cuenta;
        const float T = 128f;

        public override void Construir()
        {
            z = new Logica.Deslizante(3, d.semilla);
            Imagen("Acertijos/cuadro_marco", 40, 6, 418, 418);
            var tablero = F.Pos(F.Nodo("Tablero", area), 54, 20, 390, 390);
            var fondo = F.Img("Hueco", tablero, null, Pal.NOCHE); F.Llenar(fondo.rectTransform);
            var tex = Arte.T("Acertijos/cuadro");
            piezas = new RawImage[8]; numeros = new Text[8];
            for (int p = 0; p < 8; p++)
            {
                var rt = F.Nodo("Pieza" + (p + 1), tablero);
                var ri = F.Asegurar<RawImage>(rt.gameObject);
                ri.texture = tex;
                int col = p % 3, fila = p / 3;
                ri.uvRect = new Rect(col / 3f, 1 - (fila + 1) / 3f, 1 / 3f, 1 / 3f);
                var ol = F.Asegurar<Outline>(rt.gameObject); ol.effectColor = Pal.A(Pal.NOCHE, .8f); ol.effectDistance = new Vector2(1.5f, -1.5f);
                int pp = p;
                Tocable(ri, () => Tocar(pp));
                numeros[p] = F.Txt("Numero", rt, (p + 1).ToString(), 30, Pal.PAPEL, TextAnchor.UpperLeft, 1);
                F.Llenar(numeros[p].rectTransform, 8);
                numeros[p].gameObject.SetActive(false);
                piezas[p] = ri;
            }
            for (int pos = 0; pos < 9; pos++) if (z.celdas[pos] >= 0) F.Pos(piezas[z.celdas[pos]].rectTransform, (pos % 3) * (T + 2), (pos / 3) * (T + 2), T, T);
            Texto("Guía", 500, 40, 190, 30, 20, Pal.MAIZ, TextAnchor.MiddleCenter, 1);
            var guia = Imagen("Acertijos/cuadro", 505, 74, 180, 180);
            F.Asegurar<Outline>(guia.gameObject).effectColor = Pal.MAIZ;
            cuenta = Texto("Movimientos: 0", 490, 270, 210, 34, 18, Pal.PAPEL);
        }

        void Tocar(int pieza)
        {
            if (listo) return;
            int pos = System.Array.IndexOf(z.celdas, pieza);
            if (!z.PuedeMover(pos)) { Tw.Brinco(piezas[pieza].transform, -.05f, .15f); return; }
            int hueco = z.Hueco;
            z.Mover(pos);
            var rt = piezas[pieza].rectTransform;
            Tw.Mover(rt, rt.anchoredPosition, new Vector2((hueco % 3) * (T + 2), -(hueco / 3) * (T + 2)), .14f);
            Sonido.Tocar("tick", .9f);
            cuenta.text = "Movimientos: " + z.movimientos;
            if (z.Resuelto) Correr(Final());
        }

        IEnumerator Final()
        {
            listo = true;
            yield return new WaitForSecondsRealtime(.2f);
            foreach (var n in numeros) n.gameObject.SetActive(false);
            foreach (var p in piezas) p.GetComponent<Outline>().enabled = false;
            listo = false;
            Resolver();
        }

    }

    // ---------- Anillos acoplados del rosetón ----------
    public class AcAnillos : Acertijo
    {
        Logica.Anillos z;
        RectTransform[] aros = new RectTransform[3];
        float[] angulo = new float[3], meta = new float[3];
        float paso;

        public override void Construir()
        {
            int pasos = d.lineas.Length > 0 ? int.Parse(d.lineas[0]) : 8;
            int[] acople = d.lineas.Length > 1 ? Enteros(d.lineas[1]) : new[] { 1, -1, 0 };
            z = new Logica.Anillos(3, pasos, acople, d.semilla);
            paso = 360f / pasos;
            var fondo = Imagen("UI/redondo", 30, 10, 410, 410, Pal.NOCHE);
            fondo.preserveAspect = true;
            for (int i = 0; i < 3; i++)
            {
                var im = Imagen("Acertijos/anillo" + i, 35, 15, 400, 400);
                aros[i] = im.rectTransform;
                F.Pivote(aros[i], new Vector2(.5f, .5f));
                meta[i] = angulo[i] = -z.pos[i] * paso;
                aros[i].localRotation = Quaternion.Euler(0, 0, angulo[i]);
            }
            var marca = Imagen("Iconos/abajo", 215, -8, 40, 30, Pal.MAIZ);
            F.Asegurar<Flotar>(marca.gameObject).altura = 3;
            string[] nombres = { "Anillo de afuera", "Anillo de en medio", "Anillo de adentro" };
            for (int i = 0; i < 3; i++)
            {
                int k = i;
                float y = 30 + i * 118;
                Texto(nombres[i], 470, y, 240, 30, 19, Pal.MAIZ, TextAnchor.MiddleCenter, 1);
                Boton("", 480, y + 36, 104, 60, Pal.TURQUESA, () => Girar(k, -1), "Iconos/flecha_izq");
                Boton("", 598, y + 36, 104, 60, Pal.TURQUESA, () => Girar(k, 1), "Iconos/flecha_der");
            }
        }

        void Girar(int i, int s)
        {
            if (listo) return;
            var antes = (int[])z.pos.Clone();
            z.Girar(i, s);
            for (int k = 0; k < 3; k++)
            {
                int dlt = z.pos[k] - antes[k];
                int n = z.pasos;
                dlt = ((dlt % n) + n) % n; if (dlt > n / 2) dlt -= n;
                meta[k] -= dlt * paso;
            }
            Sonido.Tocar("tick", .8f + i * .15f);
            if (z.Resuelto) Correr(Final());
        }

        public override void Actualizar(float dt)
        {
            for (int i = 0; i < 3; i++)
            {
                angulo[i] = Mathf.Lerp(angulo[i], meta[i], 1 - Mathf.Exp(-dt * 12));
                aros[i].localRotation = Quaternion.Euler(0, 0, angulo[i]);
            }
        }

        IEnumerator Final()
        {
            listo = true;
            yield return new WaitForSecondsRealtime(.45f);
            foreach (var a in aros) Tw.Brinco(a, .06f, .4f);
            Sonido.Tocar("campana_chica", 1.2f);
            listo = false;
            Resolver();
        }
    }

    // ---------- Candelabro de 9 velas: la flama pasa de vela en vela por los brazos de bronce ----------
    public class AcCandelabro : Acertijo
    {
        Logica.Flama z;
        Image[] velas;
        Text cuenta;
        bool apagando;
        const float X0 = 150, Y0 = 8, CW = 140, CH = 138;

        static Vector2 Centro(int i) { return new Vector2(X0 + (i % 3) * CW + CW / 2, Y0 + (i / 3) * CH + CH / 2); }

        public override void Construir()
        {
            z = new Logica.Flama(9, Logica.Flama.LeerBrazos(d.lineas[0]));
            // pie y brazos de bronce (cada brazo une dos velas; solo por ahí pasa la flama)
            Imagen(null, X0 + 1.5f * CW - 8, Y0 + CH * .5f + 40, 16, CH * 2 + 40, Pal.Hex("8a5a1e"));
            foreach (var b in z.brazos) Brazo(Centro(b[0]) + new Vector2(0, 40), Centro(b[1]) + new Vector2(0, 40));
            velas = new Image[9];
            for (int i = 0; i < 9; i++)
            {
                int k = i;
                var c = Centro(i);
                Imagen(null, c.x - 26, c.y + 32, 52, 14, Pal.Hex("e9b445"));   // platillo
                var cel = Imagen(null, c.x - 45, c.y - 66, 90, 112, Color.clear, null, true);
                Tocable(cel, () => Tocar(k));
                velas[i] = Imagen("Acertijos/vela_off", 20, 4, 50, 104, null, cel.transform);
            }
            cuenta = Texto("Velas: 0 de 9", 4, 180, 140, 40, 19, Pal.PAPEL);
            Boton("Apagar todas", 574, 180, 140, 50, Pal.NOCHE3, () => { if (!listo && !apagando) { z.Apagar(); Pintar(); } }, null, 16);
            Pintar();
        }

        void Brazo(Vector2 a, Vector2 b)
        {
            var im = F.Img("Brazo", area, null, Pal.Hex("c9952f"));
            var rt = im.rectTransform;
            if (F.EsDelPrefab(rt)) return;
            var dl = b - a;
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, .5f);
            rt.anchoredPosition = new Vector2(a.x, -a.y);
            rt.sizeDelta = new Vector2(dl.magnitude, 9);
            rt.localRotation = Quaternion.Euler(0, 0, -Mathf.Atan2(dl.y, dl.x) * Mathf.Rad2Deg);
        }

        void Pintar()
        {
            for (int i = 0; i < 9; i++)
            {
                velas[i].sprite = Arte.S(z.encendidas.Contains(i) ? "Acertijos/vela_on" : "Acertijos/vela_off");
                velas[i].transform.localScale = Vector3.one * (i == z.Ultima && !z.Resuelto ? 1.12f : 1f);
            }
            cuenta.text = "Velas: " + z.encendidas.Count + " de 9";
        }

        void Tocar(int i)
        {
            if (listo || apagando) return;
            int r = z.Prender(i);
            if (r == 0) { Aviso("Esa vela ya está encendida."); return; }
            if (r < 0) { Fallo("La flama no alcanza esa vela: ningún brazo la une con la última que encendiste."); return; }
            Pintar();
            Tw.Brinco(velas[i].transform, .15f, .25f);
            Sonido.Tocar("tick", 1f + z.encendidas.Count * .06f);
            if (z.Resuelto) { Sonido.Tocar("acierto", 1.3f); Resolver(); }
            else if (z.Atorada) Correr(Apagon());
        }

        IEnumerator Apagon()
        {
            apagando = true;
            Fallo("La flama se quedó sin camino y una corriente de aire apagó las velas.");
            yield return new WaitForSecondsRealtime(1.1f);
            z.Apagar(); Pintar();
            apagando = false;
        }
    }

    // ---------- Campanas: tocar la melodía de la partitura ----------
    // Las campanas se pueden probar libremente; la melodía cuenta desde que se pulsa «Tocar la melodía».
    public class AcSimon : Acertijo
    {
        Logica.Partitura z;
        Image[] campanas = new Image[3];
        Vaiven[] vaiven = new Vaiven[3];
        Image[] notas;
        Button tocar;
        bool tocando;
        static readonly string[] SONIDO = { "campana_mediana", "campana_grande", "campana_chica" };
        static readonly string[] NOMBRE = { "Mediana", "Grande", "Chica" };
        static readonly int[] ALTURA = { 1, 0, 2 };   // la grande suena grave y la chica aguda

        public override void Construir()
        {
            z = new Logica.Partitura(Enteros(d.lineas[0]), ALTURA);
            Imagen(null, 20, 14, 680, 12, Pal.Hex("5a3418"));
            float[] xs = { 130, 360, 590 }, tam = { 150, 190, 118 };
            for (int i = 0; i < 3; i++)
            {
                int k = i;
                var im = Imagen("Acertijos/campana" + i, xs[i] - tam[i] / 2, 20, tam[i], tam[i]);
                F.Pivote(im.rectTransform, new Vector2(.5f, 1));
                vaiven[i] = F.Asegurar<Vaiven>(im.gameObject);
                vaiven[i].amplitud = .3f; vaiven[i].fase = i;
                Tocable(im, () => Tocar(k));
                campanas[i] = im;
                Texto(NOMBRE[i], xs[i] - 80, 212, 160, 26, 18, Pal.MAIZ, TextAnchor.MiddleCenter, 1);
            }
            // partitura: tres renglones (arriba agudo, abajo grave)
            Imagen("UI/papel", 40, 244, 640, 98);
            for (int r = 0; r < 3; r++) Imagen(null, 60, 262 + r * 30, 600, 3, Pal.Hex("6a4a2a"));
            int n = z.melodia.Length;
            notas = new Image[n];
            for (int k = 0; k < n; k++)
                notas[k] = Imagen("UI/redondo", 96 + k * (540f / Mathf.Max(1, n - 1)) - 13, 262 + (2 - z.melodia[k]) * 30 - 12, 26, 24, Pal.CAFE);
            tocar = Boton("Tocar la melodía", 240, 354, 240, 56, Pal.ROSA, Empezar, null, 19);
            Pintar();
        }

        void Pintar()
        {
            for (int k = 0; k < notas.Length; k++) notas[k].color = tocando && k < z.entrada ? Pal.Hex("e9b84a") : Pal.CAFE;
            F.TextoBoton(tocar, tocando ? "Volver a empezar" : "Tocar la melodía");
        }

        void Empezar()
        {
            if (listo) return;
            tocando = true; z.entrada = 0;
            Aviso("Toca las campanas siguiendo la partitura, de izquierda a derecha.");
            Pintar();
        }

        void Sonar(int b)
        {
            Sonido.Tocar(SONIDO[b]);
            vaiven[b].Golpe(16);
            Tw.Brinco(campanas[b].transform, .08f, .3f);
        }

        void Tocar(int b)
        {
            if (listo) return;
            Sonar(b);
            if (!tocando) return;
            int res = z.Tocar(b);
            if (res < 0) { tocando = false; Fallo("Esa nota no era. La melodía se interrumpió."); }
            else if (res > 0) { Pintar(); Resolver(); return; }
            Pintar();
        }
    }
}
