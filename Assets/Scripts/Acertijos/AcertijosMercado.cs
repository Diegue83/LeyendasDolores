// Acertijos del nivel 3: canal de talavera, piñata, balanza de cocadas y ollas (Torre de Hanói).
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Leyendas
{
    // ---------- Canal de azulejos (girar piezas) ----------
    public class AcCanal : Acertijo
    {
        Logica.Canal z;
        RectTransform[] piezas;
        Image[] agua;
        float[] ang, meta;
        Image jarraMeta;
        static readonly string[] TIPO = { "", "recto", "curva", "te", "cruz" };
        const float T = 96f;
        float x0, y0;

        public override void Construir()
        {
            var es = Enteros(d.respuesta);
            z = new Logica.Canal(d.lineas, es[0], es[1], d.semilla);
            int n = z.filas * z.columnas;
            x0 = (720 - z.columnas * T) / 2; y0 = 22;
            Imagen(null, x0 - 8, y0 - 8, z.columnas * T + 16, z.filas * T + 16, Pal.Hex("123050"));
            piezas = new RectTransform[n]; agua = new Image[n]; ang = new float[n]; meta = new float[n];
            for (int i = 0; i < n; i++)
            {
                int k = i;
                var b = Imagen("Acertijos/azulejo", x0 + (i % z.columnas) * T, y0 + (i / z.columnas) * T, T - 2, T - 2, null, null, true);
                if (z.tipo[i] == 0) continue;
                Tocable(b, () => Tocar(k));
                var p = F.Nodo("Pieza", b.transform); F.Llenar(p);
                var c = F.Img("Canal", p, "Acertijos/canal_" + TIPO[z.tipo[i]]); F.Llenar(c.rectTransform);
                agua[i] = F.Img("Agua", p, "Acertijos/agua_" + TIPO[z.tipo[i]]); F.Llenar(agua[i].rectTransform);
                piezas[i] = p;
                ang[i] = meta[i] = -90 * z.giro[i];
                p.localRotation = Quaternion.Euler(0, 0, ang[i]);
            }
            Imagen("Acertijos/fuente", x0 - T - 6, y0 + z.filaEntrada * T, T, T);
            jarraMeta = Imagen("Acertijos/jarra_meta", x0 + z.columnas * T + 8, y0 + z.filaSalida * T, T, T);
            Texto("Fuente", x0 - T - 10, y0 + z.filaEntrada * T + T, T + 8, 24, 15, Pal.PAPEL2);
            Texto("Jarra", x0 + z.columnas * T + 4, y0 + z.filaSalida * T + T, T + 8, 24, 15, Pal.PAPEL2);
            Pintar();
        }

        void Tocar(int i)
        {
            if (listo) return;
            z.Girar(i);
            meta[i] -= 90;
            Sonido.Tocar("tick", 1.1f);
            Pintar();
            if (z.Resuelto) Correr(Final());
        }

        void Pintar()
        {
            var con = z.ConAgua();
            for (int i = 0; i < agua.Length; i++) if (agua[i] != null) agua[i].enabled = con.Contains(i);
        }

        public override void Actualizar(float dt)
        {
            for (int i = 0; i < piezas.Length; i++)
            {
                if (piezas[i] == null) continue;
                ang[i] = Mathf.Lerp(ang[i], meta[i], 1 - Mathf.Exp(-dt * 16));
                piezas[i].localRotation = Quaternion.Euler(0, 0, ang[i]);
            }
        }

        IEnumerator Final()
        {
            listo = true;
            Sonido.Tocar("agua");
            yield return new WaitForSecondsRealtime(.35f);
            jarraMeta.sprite = Arte.S("Acertijos/jarra_meta_llena");
            Tw.Brinco(jarraMeta.transform, .25f, .4f);
            listo = false;
            Resolver("¡El agua llegó a la jarra!");
        }
    }

    // ---------- Piñata (de tiempo) ----------
    public class AcPinata : Acertijo
    {
        RectTransform pinata, palo;
        float t, vel = 1.7f, espera;
        int golpes, meta = 3;
        Text faltan;
        const float AMP = 250f, BANDA = 90f;

        public override void Construir()
        {
            int.TryParse(d.respuesta, out meta); if (meta <= 0) meta = 3;
            Imagen(null, 0, 14, 720, 4, Pal.Hex("2a1b14"));
            var banda = Imagen(null, 360 - BANDA / 2, 18, BANDA, 290, Pal.A(Pal.MAIZ, .28f));
            var br = F.Asegurar<Brillo>(banda.gameObject); br.minimo = .5f; br.maximo = 1f;
            var p = Imagen("Acertijos/pinata", 290, 18, 140, 200, null, null, true);
            Tocable(p, Pegar);
            pinata = p.rectTransform;
            F.Pivote(pinata, new Vector2(.5f, 1));
            var pl = Imagen("Objetos/palo", 420, 230, 120, 120);
            palo = pl.rectTransform;
            F.Pivote(palo, new Vector2(.1f, .1f));
            faltan = Texto("", 20, 330, 300, 36, 20, Pal.PAPEL, TextAnchor.MiddleLeft, 1);
            Boton("¡Pega!", 250, 360, 220, 64, Pal.ROSA, Pegar, null, 30);
            Pintar();
        }

        void Pintar() { faltan.text = "Golpes: " + golpes + " de " + meta; }

        public override void Actualizar(float dt)
        {
            if (listo) return;
            t += dt * vel;
            espera = Mathf.Max(0, espera - dt);
            float x = 360 + AMP * Mathf.Sin(t);
            pinata.anchoredPosition = new Vector2(x, -18);
            pinata.localRotation = Quaternion.Euler(0, 0, -8 * Mathf.Cos(t));
            palo.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(palo.localEulerAngles.z > 180 ? palo.localEulerAngles.z - 360 : palo.localEulerAngles.z, 0, dt * 8));
        }

        void Pegar()
        {
            if (listo || espera > 0) return;
            espera = .45f;
            palo.localRotation = Quaternion.Euler(0, 0, 50);
            float off = Mathf.Abs(AMP * Mathf.Sin(t));
            if (off <= BANDA / 2 + 22)
            {
                golpes++;
                Sonido.Tocar("golpe", 1f + golpes * .1f);
                Tw.Brinco(pinata, .15f, .3f);
                Confeti.Lluvia(j.confeti, (Vector2)j.confeti.InverseTransformPoint(pinata.TransformPoint(pinata.rect.center)), 10);
                vel *= 1.4f;
                Pintar();
                if (golpes >= meta) Resolver("¡Se rompió la piñata!");
                else Bien("¡Le diste! Faltan " + (meta - golpes) + ".");
            }
            else Fallo("¡Uy! Pasó de largo.");
        }
    }

    // ---------- Balanza: la cocada hueca ----------
    public class AcBalanza : Acertijo
    {
        Logica.Balanza z;
        int intentos;
        int[] lugar = new int[9];   // 0 bandeja, 1 plato izquierdo, 2 plato derecho
        RectTransform brazo, platoI, platoD;
        RectTransform[] cocadas = new RectTransform[9];
        Image[] cocImg = new Image[9];
        float angulo, metaAng;
        bool elegir;
        Text pesadas;
        Button bElegir;
        static readonly Vector2 PIVOTE = new Vector2(250, 92);
        const float L = 178;

        public override void Construir()
        {
            z = new Logica.Balanza(9, 2, d.semilla);
            var bs = Imagen("Acertijos/balanza_base", PIVOTE.x - 75, PIVOTE.y - 18, 150, 188);
            bs.preserveAspect = false;
            platoI = Plato(); platoD = Plato();
            var b = Imagen("Acertijos/balanza_brazo", 0, 0, 2 * L + 20, 26);
            brazo = PosC(b.rectTransform, PIVOTE.x, PIVOTE.y, 2 * L + 20, 26);
            b.preserveAspect = false;
            Imagen(null, 20, 356, 470, 66, Pal.A(Pal.NOCHE, .55f));
            Texto("Bandeja", 24, 336, 200, 20, 14, Pal.PAPEL2, TextAnchor.MiddleLeft);
            for (int i = 0; i < 9; i++)
            {
                int k = i;
                var c = Imagen("Acertijos/cocada", 0, 0, 52, 40, null, null, true);
                c.rectTransform.pivot = new Vector2(.5f, .5f);
                Tocable(c, () => Tocar(k));
                Texto((i + 1).ToString(), 0, 6, 52, 30, 15, Pal.CAFE, TextAnchor.MiddleCenter, 1, c.transform);
                cocadas[i] = c.rectTransform; cocImg[i] = c;
            }
            pesadas = Texto("", 516, 196, 196, 30, 18, Pal.PAPEL, TextAnchor.MiddleCenter, 1);
            Boton("Pesar", 524, 236, 180, 56, Pal.TURQUESA, Pesar, null, 22);
            bElegir = Boton("Elegir", 524, 302, 180, 56, Pal.ROSA, CambiarModo, null, 22);
            Texto("Toca una cocada para pasarla: bandeja, plato izquierdo, plato derecho.", 510, 10, 205, 170, 15, Pal.PAPEL2, TextAnchor.UpperCenter);
            Colocar(true);
        }

        RectTransform Plato()
        {
            var p = Imagen("Acertijos/balanza_plato", 0, 0, 150, 123);
            p.preserveAspect = false;
            p.rectTransform.pivot = new Vector2(.5f, 1);
            return p.rectTransform;
        }

        Vector2 Extremo(int lado)
        {
            float a = angulo * Mathf.Deg2Rad;
            float s = lado == 1 ? -1 : 1;
            return new Vector2(PIVOTE.x + s * L * Mathf.Cos(a), PIVOTE.y - s * L * Mathf.Sin(a)); // y hacia abajo
        }

        void Colocar(bool inmediato)
        {
            var eI = Extremo(1); var eD = Extremo(2);
            platoI.anchoredPosition = new Vector2(eI.x, -eI.y);
            platoD.anchoredPosition = new Vector2(eD.x, -eD.y);
            brazo.localRotation = Quaternion.Euler(0, 0, angulo);
            int bi = 0, nI = 0, nD = 0;
            for (int i = 0; i < 9; i++)
            {
                Vector2 dst;
                if (lugar[i] == 0) { dst = new Vector2(52 + bi * 50, -389); bi++; }
                else
                {
                    var e = lugar[i] == 1 ? eI : eD;
                    int idx = lugar[i] == 1 ? nI++ : nD++;
                    dst = new Vector2(e.x - 42 + (idx % 3) * 42, -(e.y + 76 - (idx / 3) * 24));
                }
                if (inmediato) cocadas[i].anchoredPosition = dst;
                else cocadas[i].anchoredPosition = Vector2.Lerp(cocadas[i].anchoredPosition, dst, .35f);
            }
            pesadas.text = "Pesadas: " + z.pesadas + " de " + z.maxPesadas;
        }

        public override void Actualizar(float dt)
        {
            angulo = Mathf.Lerp(angulo, metaAng, 1 - Mathf.Exp(-dt * 5));
            Colocar(false);
        }

        void Tocar(int i)
        {
            if (listo) return;
            if (elegir)
            {
                if (z.Elegir(i)) { cocImg[i].color = Pal.OK; Resolver("¡Esa es la cocada hueca!"); }
                else
                {
                    cocImg[i].color = Pal.MAL;
                    Fallo("Esa no era. Doña Lupita revolvió las cocadas otra vez.");
                    Correr(Reiniciar());
                }
                return;
            }
            lugar[i] = (lugar[i] + 1) % 3;
            Sonido.Tocar("tick", lugar[i] == 0 ? .8f : 1.1f);
            metaAng = 0;
        }

        void Pesar()
        {
            if (listo) return;
            if (elegir) CambiarModo();
            if (!z.QuedanPesadas) { Aviso("Ya usaste las 2 pesadas. Toca «Elegir» y luego la cocada hueca."); return; }
            var izq = Enumerable.Range(0, 9).Where(i => lugar[i] == 1).ToList();
            var der = Enumerable.Range(0, 9).Where(i => lugar[i] == 2).ToList();
            if (izq.Count == 0 && der.Count == 0) { Aviso("Primero pon cocadas en los platos."); return; }
            int r = z.Pesar(izq, der);
            metaAng = r < 0 ? 9f : r > 0 ? -9f : 0f; // -1: baja la izquierda
            Sonido.Tocar("golpe", .7f, .6f);
            Bien(r == 0 ? "Se equilibra." : r < 0 ? "Baja el plato izquierdo: el derecho pesa menos." : "Baja el plato derecho: el izquierdo pesa menos.");
        }

        void CambiarModo()
        {
            if (listo) return;
            elegir = !elegir;
            F.TextoBoton(bElegir, elegir ? "Cancelar" : "Elegir");
            if (elegir) Aviso("Toca la cocada que creas que está hueca.");
        }

        IEnumerator Reiniciar()
        {
            yield return new WaitForSecondsRealtime(1.1f);
            intentos++;
            z = new Logica.Balanza(9, 2, d.semilla + intentos * 31);
            for (int i = 0; i < 9; i++) { lugar[i] = 0; cocImg[i].color = Color.white; }
            metaAng = 0;
            if (elegir) CambiarModo();
        }
    }

    // ---------- Torre de Hanói con ollas ----------
    public class AcHanoi : Acertijo
    {
        Logica.Hanoi z;
        RectTransform[] ollas;
        int sel = -1;
        Text cuenta;
        static readonly float[] X = { 130, 360, 590 };
        const float BASE = 318, PASO = 44;

        public override void Construir()
        {
            int n = 4; int.TryParse(d.respuesta, out n); if (n < 2) n = 4;
            z = new Logica.Hanoi(n);
            Imagen(null, 20, BASE, 680, 22, Pal.Hex("6a3a1e"));
            for (int p = 0; p < 3; p++)
            {
                int k = p;
                Imagen("Acertijos/poste", X[p] - 10, BASE - 230, 20, 232).preserveAspect = false;
                var zona = Imagen(null, X[p] - 112, 30, 224, BASE + 24, Color.clear, null, true);
                Tocable(zona, () => Tocar(k));
            }
            ollas = new RectTransform[n];
            for (int dsc = n - 1; dsc >= 0; dsc--)
            {
                var im = Imagen("Acertijos/olla" + Mathf.Clamp(3 - dsc * 4 / n, 0, 3), 0, 0, 210, 79);
                im.rectTransform.pivot = new Vector2(.5f, 0);
                ollas[dsc] = im.rectTransform;
            }
            for (int p = 0; p < 3; p++) for (int s = 0; s < z.postes[p].Count; s++) ollas[z.postes[p][s]].anchoredPosition = Lugar(p, s);
            cuenta = Texto("", 160, 350, 400, 30, 18, Pal.PAPEL, TextAnchor.MiddleCenter, 1);
            Pintar();
        }

        Vector2 Lugar(int poste, int nivel) { return new Vector2(X[poste], -(BASE + 2 - nivel * PASO)); }

        void Pintar() { cuenta.text = "Movimientos: " + z.movimientos + "   ·   mínimo " + z.Minimo; }

        void Tocar(int p)
        {
            if (listo) return;
            if (sel < 0)
            {
                if (z.postes[p].Count == 0) return;
                sel = p;
                var o = ollas[z.Arriba(p)];
                o.SetAsLastSibling();
                Tw.Mover(o, o.anchoredPosition, new Vector2(X[p], -64), .18f);
                Sonido.Tocar("tick");
                return;
            }
            int de = sel; sel = -1;
            int disco = z.Arriba(de);
            var rt = ollas[disco];
            if (de == p) { Tw.Mover(rt, rt.anchoredPosition, Lugar(p, z.postes[p].Count - 1), .18f); return; }
            if (!z.Mover(de, p))
            {
                Fallo("Una olla grande no puede ir sobre una chica.");
                Tw.Mover(rt, rt.anchoredPosition, Lugar(de, z.postes[de].Count - 1), .2f);
                return;
            }
            Correr(Mover(rt, p, z.postes[p].Count - 1));
            Sonido.Tocar("golpe", 1.3f, .5f);
            Pintar();
            if (z.Resuelto) Resolver(z.movimientos == z.Minimo ? "¡Perfecto: en el mínimo de movimientos!" : "¡Todas las ollas pasaron!");
        }

        IEnumerator Mover(RectTransform rt, int p, int nivel)
        {
            var a = rt.anchoredPosition; var arriba = new Vector2(X[p], -64);
            yield return Tw.Valor(.14f, k => rt.anchoredPosition = Vector2.Lerp(a, arriba, k));
            var b = Lugar(p, nivel);
            yield return Tw.Valor(.16f, k => rt.anchoredPosition = Vector2.Lerp(arriba, b, k), Tw.Rebote);
        }
    }
}
