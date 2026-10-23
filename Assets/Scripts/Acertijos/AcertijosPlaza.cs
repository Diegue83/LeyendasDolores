// Acertijos del nivel 2 (nieves, papel picado, jarras, cronología) y el de ordenar libros del nivel 4.
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Leyendas
{
    // ---------- Lógica de las nieves (asignación con pistas) ----------
    public class AcNieves : Acertijo
    {
        Logica.Asignacion z;
        Image[] bolas; Text[] nombres; RectTransform[] vasos;
        static readonly string[] COLOR = { "c4dd78", "6a3424", "e3b36a", "f6f0c8", "f6a3c0", "ff9a6a", "9ad0f0" };

        public override void Construir()
        {
            z = new Logica.Asignacion(Enteros(d.respuesta));
            int n = d.opciones.Length;
            bolas = new Image[n]; nombres = new Text[n]; vasos = new RectTransform[n];
            float w = 128, gap = 10, x0 = (720 - (n * w + (n - 1) * gap)) / 2;
            for (int i = 0; i < n; i++)
            {
                int k = i;
                float x = x0 + i * (w + gap);
                var v = Imagen("UI/papel", x, 6, w, 196, null, null, true);
                Tocable(v, () => Siguiente(k));
                vasos[i] = v.rectTransform;
                Texto("Vaso " + (i + 1), 0, 6, w, 26, 18, Pal.CHILE, TextAnchor.MiddleCenter, 1, v.transform);
                bolas[i] = Imagen("UI/redondo", w / 2 - 36, 40, 72, 72, Pal.A(Color.gray, .3f), v.transform);
                var copa = Imagen(null, w / 2 - 30, 100, 60, 40, Pal.Hex("1f6aa0"), v.transform);
                F.Asegurar<Outline>(copa.gameObject).effectColor = Pal.PAPEL;
                nombres[i] = Texto("?", 4, 146, w - 8, 44, 17, Pal.CAFE, TextAnchor.MiddleCenter, 2, v.transform);
            }
            var pistas = string.Join("\n", d.lineas.Select((l, i) => (i + 1) + ". " + l).ToArray());
            Texto("<b>Las pistas de Don Chuy</b>\n" + pistas, 16, 210, 690, 148, 16, Pal.PAPEL, TextAnchor.UpperLeft);
            Boton("Servir", 260, 366, 200, 58, Pal.ROSA, Servir, null, 24);
        }

        void Siguiente(int i)
        {
            if (listo) return;
            z.Siguiente(i);
            int s = z.valor[i];
            bolas[i].color = Pal.Hex(COLOR[s % COLOR.Length]);
            nombres[i].text = d.opciones[s];
            Tw.Brinco(bolas[i].transform, .2f, .25f);
            Sonido.Tocar("tick", 1f + s * .08f);
        }

        void Servir()
        {
            if (listo) return;
            if (!z.Completa) { Aviso("Falta poner sabor en algunos vasos."); return; }
            if (!z.SinRepetir) { Fallo("Hay sabores repetidos: cada vaso lleva uno distinto."); return; }
            if (z.Resuelto) { Resolver("¡Así mero! Don Chuy está feliz."); return; }
            Fallo("Don Chuy niega con la cabeza: así no quedan sus vasos.");
        }
    }

    // ---------- Nonograma (papel picado) ----------
    public class AcNonograma : Acertijo
    {
        Logica.Nonograma z;
        Image[] celdas;
        Text[] pf, pc;
        static readonly Color SIN = Pal.Hex("ff7ab0"), CON = Pal.Hex("2a1830");

        public override void Construir()
        {
            z = new Logica.Nonograma(d.lineas);
            float c = Mathf.Min(58, 320f / Mathf.Max(z.filas, z.columnas)), x0 = 300, y0 = 100;
            celdas = new Image[z.filas * z.columnas];
            pf = new Text[z.filas]; pc = new Text[z.columnas];
            Imagen(null, x0 - 8, y0 - 8, z.columnas * c + 16, z.filas * c + 16, Pal.ROSA);
            for (int i = 0; i < celdas.Length; i++)
            {
                int k = i;
                var im = Imagen(null, x0 + (i % z.columnas) * c + 2, y0 + (i / z.columnas) * c + 2, c - 4, c - 4, SIN, null, true);
                Tocable(im, () => Tocar(k));
                celdas[i] = im;
            }
            for (int f = 0; f < z.filas; f++) pf[f] = Texto(string.Join("  ", z.pistaFilas[f].Select(x => x.ToString()).ToArray()), x0 - 170, y0 + f * c, 156, c, 24, Pal.PAPEL, TextAnchor.MiddleRight, 1);
            for (int k = 0; k < z.columnas; k++) pc[k] = Texto(string.Join("\n", z.pistaColumnas[k].Select(x => x.ToString()).ToArray()), x0 + k * c, 0, c, y0 - 10, 24, Pal.PAPEL, TextAnchor.LowerCenter, 1);
            Boton("Borrar", 590, 372, 120, 50, Pal.NOCHE3, Borrar, null, 18);
            Revisar();
        }

        void Tocar(int i)
        {
            if (listo) return;
            z.Tocar(i);
            celdas[i].color = z.pintada[i] ? CON : SIN;
            Tw.Brinco(celdas[i].transform, .1f, .18f);
            Sonido.Tocar("papel", z.pintada[i] ? 1.2f : .9f, .7f);
            Revisar();
            if (z.Resuelto) Resolver("¡Recortaste una campana!");
        }

        void Borrar()
        {
            if (listo) return;
            for (int i = 0; i < celdas.Length; i++) if (z.pintada[i]) { z.Tocar(i); celdas[i].color = SIN; }
            Revisar();
        }

        void Revisar()
        {
            for (int f = 0; f < z.filas; f++) pf[f].color = z.FilaCumple(f) ? Pal.OK : Pal.PAPEL;
            for (int k = 0; k < z.columnas; k++) pc[k].color = z.ColumnaCumple(k) ? Pal.OK : Pal.PAPEL;
        }
    }

    // ---------- Jarras de agua de jamaica ----------
    public class AcJarras : Acertijo
    {
        Logica.Jarras z;
        int[] cap, ini, meta;
        Image[] liquido; Text[] etiqueta; RectTransform[] jarra;
        int sel = -1;
        Text cuenta;
        bool vertiendo;

        public override void Construir()
        {
            cap = Enteros(d.lineas[0]); ini = Enteros(d.lineas[1]); meta = Enteros(d.lineas[2]);
            z = new Logica.Jarras(cap, ini, meta);
            int n = cap.Length;
            liquido = new Image[n]; etiqueta = new Text[n]; jarra = new RectTransform[n];
            float[] xs = { 150, 360, 570 };
            for (int i = 0; i < n; i++)
            {
                int k = i;
                float h = 48 + cap[i] * 34, w = 128;
                var caja = F.Nodo("Jarra" + cap[i], area);
                F.Pos(caja, xs[i] - w / 2, 344 - h, w, h);
                F.Pivote(caja, new Vector2(.5f, 0));
                jarra[i] = caja;
                var liq = F.Img("Liquido", caja, "Acertijos/liquido");
                F.Pos(liq.rectTransform, 9, 22, w - 18, h - 26);
                liq.type = Image.Type.Filled; liq.fillMethod = Image.FillMethod.Vertical; liq.fillOrigin = (int)Image.OriginVertical.Bottom;
                liq.fillAmount = (float)z.litros[i] / cap[i];
                liquido[i] = liq;
                var vid = F.Img("Vidrio", caja, "Acertijos/jarra_vidrio", null, true);
                F.Llenar(vid.rectTransform);
                Tocable(vid, () => Tocar(k));
                etiqueta[i] = Texto("", xs[i] - 90, 350, 180, 34, 22, Pal.PAPEL, TextAnchor.MiddleCenter, 1);
            }
            var m = new List<string>();
            for (int i = 0; i < n; i++) if (meta[i] >= 0) m.Add(meta[i] + " L en la jarra de " + cap[i]);
            Texto("Meta: " + string.Join(" y ", m.ToArray()), 20, 0, 470, 30, 18, Pal.MAIZ, TextAnchor.MiddleLeft, 2);
            cuenta = Texto("", 20, 30, 300, 26, 17, Pal.PAPEL2, TextAnchor.MiddleLeft);
            Boton("Reiniciar", 576, 386, 134, 44, Pal.NOCHE3, Reiniciar, null, 18);
            Pintar();
        }

        void Pintar()
        {
            for (int i = 0; i < cap.Length; i++) etiqueta[i].text = z.litros[i] + " / " + cap[i] + " L";
            cuenta.text = "Movimientos: " + z.movimientos;
        }

        void Tocar(int i)
        {
            if (listo || vertiendo) return;
            if (sel < 0) { if (z.litros[i] == 0) { Aviso("Esa jarra está vacía."); return; } Elegir(i, true); return; }
            if (sel == i) { Elegir(i, false); return; }
            int de = sel;
            Elegir(sel, false);
            var antes = (int[])z.litros.Clone();
            if (!z.Vaciar(de, i)) { Aviso("No cabe más agua en esa jarra."); Tw.Brinco(jarra[i], -.05f, .2f); return; }
            Correr(Verter(de, i, antes));
        }

        void Elegir(int i, bool si)
        {
            sel = si ? i : -1;
            var rt = jarra[i];
            var p = rt.anchoredPosition;
            Tw.Mover(rt, p, new Vector2(p.x, si ? p.y + 16 : p.y - 16), .15f);
            if (si) Sonido.Tocar("tick");
        }

        IEnumerator Verter(int de, int a, int[] antes)
        {
            vertiendo = true;
            Sonido.Tocar("agua");
            float f0 = (float)antes[de] / cap[de], f1 = (float)antes[a] / cap[a];
            float t0 = (float)z.litros[de] / cap[de], t1 = (float)z.litros[a] / cap[a];
            jarra[de].localRotation = Quaternion.Euler(0, 0, a > de ? -12 : 12);
            yield return Tw.Valor(.6f, k => { liquido[de].fillAmount = Mathf.Lerp(f0, t0, k); liquido[a].fillAmount = Mathf.Lerp(f1, t1, k); });
            jarra[de].localRotation = Quaternion.identity;
            Pintar();
            vertiendo = false;
            if (z.Resuelto) Resolver("¡Cuatro y cuatro litros!");
        }

        void Reiniciar()
        {
            if (listo || vertiendo) return;
            if (sel >= 0) Elegir(sel, false);
            z = new Logica.Jarras(cap, ini, meta);
            for (int i = 0; i < cap.Length; i++) liquido[i].fillAmount = (float)z.litros[i] / cap[i];
            Pintar();
        }
    }

    // ---------- Ordenar: cronología (tarjetas) y libros (lomos) ----------
    public class AcOrden : Acertijo
    {
        readonly bool libros;
        Logica.Orden z;
        RectTransform[] items;
        Image[] fondos;
        int sel = -1;
        public AcOrden(bool libros) { this.libros = libros; }

        int N { get { return libros ? 6 : d.lineas.Length; } }

        Vector2 Lugar(int pos) { return libros ? new Vector2(54 + pos * 104, -16) : new Vector2(20, -6 - pos * 70); }

        public override void Construir()
        {
            z = new Logica.Orden(N, d.semilla);
            items = new RectTransform[N]; fondos = new Image[N];
            if (libros)
            {
                Imagen(null, 30, 300, 660, 16, Pal.Hex("6a3a1e"));
                for (int p = 0; p < N; p++) Texto((p + 1).ToString(), 54 + p * 104, 318, 92, 30, 18, Pal.PAPEL2, TextAnchor.MiddleCenter, 1);
            }
            else for (int p = 0; p < N; p++) Texto((p + 1) + "°", 0, 6 + p * 70, 18, 62, 18, Pal.MAIZ, TextAnchor.MiddleCenter, 1);
            for (int e = 0; e < N; e++)
            {
                int k = e;
                Image im;
                if (libros)
                {
                    im = Imagen("Acertijos/libro" + e, 0, 0, 92, 284, null, null, true);
                    if (e < d.lineas.Length)
                    {
                        var partes = d.lineas[e].Split('·');
                        var sub = Texto(partes.Length > 1 ? partes[1].Trim() : "", -10, 286, 112, 40, 12, Pal.PAPEL2, TextAnchor.UpperCenter, 0, im.transform);
                        sub.gameObject.SetActive(false);
                    }
                }
                else
                {
                    im = Imagen("UI/papel", 0, 0, 680, 62, null, null, true);
                    Texto(d.lineas[e], 16, 0, 650, 62, 17, Pal.CAFE, TextAnchor.MiddleLeft, 0, im.transform);
                }
                fondos[e] = im;
                items[e] = im.rectTransform;
                Tocable(im, () => Tocar(k));
            }
            for (int pos = 0; pos < N; pos++) items[z.actual[pos]].anchoredPosition = Lugar(pos);
            Boton("Comprobar", 250, 370, 220, 56, Pal.ROSA, Comprobar, null, 22);
        }

        void Tocar(int elemento)
        {
            if (listo) return;
            int pos = z.actual.IndexOf(elemento);
            if (sel < 0) { sel = pos; fondos[elemento].color = Pal.MAIZ; Tw.Brinco(items[elemento], .06f, .2f); Sonido.Tocar("tick"); return; }
            int otro = z.actual[sel];
            fondos[otro].color = Color.white;
            if (sel == pos) { sel = -1; return; }
            z.Intercambiar(sel, pos);
            Tw.Mover(items[otro], items[otro].anchoredPosition, Lugar(pos), .25f, Tw.EntraSale);
            Tw.Mover(items[elemento], items[elemento].anchoredPosition, Lugar(sel), .25f, Tw.EntraSale);
            Sonido.Tocar("papel");
            sel = -1;
        }

        void Comprobar()
        {
            if (listo) return;
            if (sel >= 0) { fondos[z.actual[sel]].color = Color.white; sel = -1; }
            if (z.Resuelto)
            {
                for (int e = 0; e < N; e++) { fondos[e].color = Pal.Hex("d8f5dc"); Tw.Brinco(items[e], .08f, .3f); }
                Resolver(libros ? "¡En orden! Mira los números dorados." : "¡Así fue como pasó!");
                return;
            }
            Fallo(libros ? "La caja no hace ningún ruido. Ese no es el orden." : "Así no fue como pasó.");
        }
    }
}
