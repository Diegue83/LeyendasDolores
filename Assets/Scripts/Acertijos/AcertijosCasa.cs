// Acertijos del nivel 4: rueda de cifrado (César), caja fuerte de disco, ruta insurgente, el caso de El Campanero y armado de la campana.
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Leyendas
{
    // ---------- Rueda de cifrado (César) ----------
    public class AcCesar : Acertijo
    {
        int clave, k;
        string cifrado;
        RectTransform interior;
        float ang;
        Text lectura, textoClave;

        public override void Construir()
        {
            int.TryParse(d.respuesta, out clave);
            string msg = d.lineas.Length > 0 ? d.lineas[0] : "HOLA";
            cifrado = Logica.Cesar.Cifrar(msg, clave);
            Imagen("Acertijos/rueda_exterior", 10, 0, 370, 370);
            var inn = Imagen("Acertijos/rueda_interior", 10, 0, 370, 370);
            interior = inn.rectTransform;
            F.Pivote(interior, new Vector2(.5f, .5f));
            Boton("", 104, 374, 80, 50, Pal.TURQUESA, () => Girar(-1), "Iconos/flecha_izq");
            Boton("", 206, 374, 80, 50, Pal.TURQUESA, () => Girar(1), "Iconos/flecha_der");
            Texto("Carta cifrada", 396, 0, 320, 28, 18, Pal.MAIZ, TextAnchor.MiddleLeft, 1);
            Texto(cifrado, 396, 30, 320, 120, 19, Pal.PAPEL2, TextAnchor.UpperLeft, 2);
            textoClave = Texto("", 396, 150, 320, 28, 18, Pal.MAIZ, TextAnchor.MiddleLeft, 1);
            lectura = Texto("", 396, 180, 320, 130, 19, Pal.PAPEL, TextAnchor.UpperLeft, 2);
            Boton("Leer", 450, 360, 200, 58, Pal.ROSA, Leer, null, 24);
            Pintar();
        }

        void Girar(int s)
        {
            if (listo) return;
            k = ((k + s) % 26 + 26) % 26;
            Sonido.Tocar("tick", .9f + k * .01f);
            Pintar();
        }

        void Pintar()
        {
            textoClave.text = "Leyendo con la A bajo la " + Logica.Cesar.ABC[k] + "  (clave " + k + ")";
            lectura.text = Logica.Cesar.Descifrar(cifrado, k);
        }

        public override void Actualizar(float dt)
        {
            float meta = -k * 360f / 26f;
            float dlt = Mathf.DeltaAngle(ang, meta);
            ang += dlt * (1 - Mathf.Exp(-dt * 12));
            interior.localRotation = Quaternion.Euler(0, 0, ang);
        }

        void Leer()
        {
            if (listo) return;
            if (k == clave) { lectura.color = Pal.OK; Resolver("¡El mensaje tiene sentido!"); }
            else Fallo("Todavía no tiene sentido. Prueba otra clave.");
        }
    }

    // ---------- Caja fuerte de disco ----------
    public class AcCaja : Acertijo
    {
        Logica.CajaFuerte z;
        RectTransform disco;
        float ang, meta;
        Text numero, marcas;
        int[] sentidos;

        public override void Construir()
        {
            int n = d.lineas.Length > 0 ? int.Parse(d.lineas[0]) : 40;
            sentidos = d.lineas.Length > 1 ? Enteros(d.lineas[1]) : new[] { 1, -1, 1 };
            z = new Logica.CajaFuerte(n, Enteros(d.respuesta), sentidos);
            Imagen(null, 30, 16, 400, 400, Pal.Hex("3a3a46"));
            var dsc = Imagen("Acertijos/dial", 50, 36, 360, 360);
            disco = dsc.rectTransform;
            F.Pivote(disco, new Vector2(.5f, .5f));
            Imagen("Acertijos/dial_marca", 210, 4, 40, 40);
            var bi = Boton("Izquierda", 448, 30, 128, 60, Pal.TURQUESA, null, "Iconos/flecha_izq", 18);
            F.Asegurar<Repetidor>(bi.gameObject).accion = () => Girar(-1);
            var bd = Boton("Derecha", 586, 30, 128, 60, Pal.TURQUESA, null, "Iconos/flecha_der", 18);
            F.Asegurar<Repetidor>(bd.gameObject).accion = () => Girar(1);
            numero = Texto("0", 448, 104, 266, 90, 70, Pal.PAPEL, TextAnchor.MiddleCenter, 1);
            Boton("Marcar", 486, 206, 190, 60, Pal.ROSA, Marcar, null, 24);
            marcas = Texto("", 448, 280, 266, 50, 30, Pal.MAIZ, TextAnchor.MiddleCenter, 1);
            Texto("Derecha · Izquierda · Derecha", 440, 330, 280, 30, 15, Pal.PAPEL2);
            Pintar();
        }

        void Girar(int s)
        {
            if (listo) return;
            z.Girar(s);
            meta -= s * 360f / z.numeros;
            Sonido.Tocar("tick", s > 0 ? 1.15f : .95f, .7f);
            Pintar();
        }

        void Pintar()
        {
            numero.text = z.posicion.ToString("00");
            var p = new List<string>();
            for (int i = 0; i < z.Pasos; i++) p.Add(i < z.marcados.Count ? z.marcados[i].ToString("00") : "__");
            marcas.text = string.Join(" · ", p.ToArray());
        }

        public override void Actualizar(float dt)
        {
            ang = Mathf.Lerp(ang, meta, 1 - Mathf.Exp(-dt * 18));
            disco.localRotation = Quaternion.Euler(0, 0, ang);
        }

        void Marcar()
        {
            if (listo) return;
            if (z.Marcar())
            {
                Sonido.Tocar("golpe", 1.6f, .6f);
                Tw.Brinco(marcas.transform, .1f, .2f);
                Pintar();
                if (z.Resuelto) { Sonido.Tocar("puerta", .6f); Resolver("¡Clac! La caja fuerte se abrió."); }
                else Bien("Marcado. Ahora gira hacia la " + (sentidos[z.marcados.Count] > 0 ? "DERECHA." : "IZQUIERDA."));
            }
            else { Pintar(); Fallo("El disco hizo «clac»... y se reinició. Empieza otra vez."); }
        }
    }

    // ---------- Ruta insurgente en el mapa ----------
    public class AcRuta : Acertijo
    {
        Logica.Ruta z;
        int[] orden;
        Image[] puntos;
        Vector2[] pos;
        readonly List<GameObject> lineas = new List<GameObject>();
        RectTransform capaLineas;
        const float X0 = 100, Y0 = 6, ESC = 520f / 320f;

        public override void Construir()
        {
            orden = Enteros(d.respuesta);
            z = new Logica.Ruta(orden);
            Imagen("Acertijos/mapa_ruta", X0, Y0, 520, 390).preserveAspect = false;
            capaLineas = F.Pos(F.Nodo("Lineas", area), 0, 0, 720, 430);
            int n = d.opciones.Length;
            puntos = new Image[n]; pos = new Vector2[n];
            for (int i = 0; i < n; i++)
            {
                var partes = d.opciones[i].Split(':');
                var xy = partes[1].Split(',');
                pos[i] = new Vector2(X0 + float.Parse(xy[0], System.Globalization.CultureInfo.InvariantCulture) * ESC, Y0 + float.Parse(xy[1], System.Globalization.CultureInfo.InvariantCulture) * ESC);
                int k = i;
                var p = Imagen("Acertijos/punto_mapa", pos[i].x - 19, pos[i].y - 19, 38, 38, null, null, true);
                Tocable(p, () => Visitar(k));
                puntos[i] = p;
                var t = Texto(partes[0], pos[i].x - 80, pos[i].y + 16, 160, 24, 15, Pal.CAFE, TextAnchor.UpperCenter, 2);
                var o = F.Asegurar<Outline>(t.gameObject); o.effectColor = Pal.A(Pal.PAPEL, .8f); o.effectDistance = new Vector2(1.5f, -1.5f);
            }
            Pintar();
        }

        void Pintar()
        {
            for (int i = 0; i < puntos.Length; i++) puntos[i].sprite = Arte.S(z.hecha.Contains(i) ? "Acertijos/punto_mapa_ok" : "Acertijos/punto_mapa");
            foreach (var l in lineas) Object.Destroy(l);
            lineas.Clear();
            for (int i = 1; i < z.hecha.Count; i++) Linea(pos[z.hecha[i - 1]], pos[z.hecha[i]]);
        }

        void Linea(Vector2 a, Vector2 b)
        {
            var im = F.Img("Tramo", capaLineas, null, Pal.CHILE);
            var rt = im.rectTransform;
            var dlt = b - a;
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, .5f);
            rt.anchoredPosition = new Vector2(a.x, -a.y);
            rt.sizeDelta = new Vector2(dlt.magnitude, 6);
            rt.localRotation = Quaternion.Euler(0, 0, -Mathf.Atan2(dlt.y, dlt.x) * Mathf.Rad2Deg);
            lineas.Add(im.gameObject);
            Tw.Escala(rt, new Vector3(0, 1, 1), Vector3.one, .3f);
        }

        void Visitar(int i)
        {
            if (listo || z.hecha.Contains(i)) return;
            if (z.Visitar(i))
            {
                Sonido.Tocar("tick", 1f + z.hecha.Count * .1f);
                Tw.Brinco(puntos[i].transform, .3f, .3f);
                Pintar();
                if (z.Resuelto) Resolver("¡Esa fue la ruta insurgente!");
            }
            else { Pintar(); Fallo("Por ahí no pasaron. Empieza otra vez desde Dolores."); }
        }
    }

    // ---------- Armar la campana ----------
    public class AcFinal : Acertijo
    {
        string[] partes;
        int[] orden;
        int paso;
        Dictionary<string, GameObject> colocadas = new Dictionary<string, GameObject>();
        Dictionary<string, Button> botones = new Dictionary<string, Button>();
        RectTransform campana;

        public override void Construir()
        {
            partes = d.opciones;
            orden = Enteros(d.respuesta);
            var c = F.Pos(F.Nodo("Campana", area), 30, 20, 340, 340);
            campana = c;
            var cont = F.Img("Contorno", c, "Acertijos/contorno_campana"); F.Llenar(cont.rectTransform);
            foreach (var p in new[] { "yugo", "corona", "cuerpo", "badajo" })
            {
                var im = F.Img(p, c, "Acertijos/parte_" + p); F.Llenar(im.rectTransform);
                im.gameObject.SetActive(false);
                colocadas[p] = im.gameObject;
            }
            // Los botones en otro orden para que no regale la respuesta.
            var mezcla = new[] { "corona", "badajo", "yugo", "cuerpo" }.Where(x => partes.Contains(x)).ToArray();
            for (int i = 0; i < mezcla.Length; i++)
            {
                var nombre = mezcla[i];
                var b = Boton("", 410 + (i % 2) * 150, 20 + (i / 2) * 160, 136, 136, Pal.NOCHE3, () => Poner(nombre), "Objetos/" + nombre);
                botones[nombre] = b;
                Texto(D(nombre), 400 + (i % 2) * 150, 158 + (i / 2) * 160, 156, 22, 15, Pal.PAPEL2);
            }
        }

        string D(string p) { return DatosJuego.D.Objeto(p).nombre; }

        void Poner(string p)
        {
            if (listo) return;
            if (p != partes[orden[paso]]) { Fallo("Esa pieza todavía no va."); return; }
            paso++;
            colocadas[p].SetActive(true);
            Tw.Aparecer(colocadas[p].transform, .4f);
            botones[p].interactable = false;
            botones[p].GetComponent<Image>().color = Pal.A(Color.white, .3f);
            Sonido.Tocar("golpe", 1.2f);
            if (paso >= orden.Length) Correr(Final());
        }

        IEnumerator Final()
        {
            listo = true;
            F.Pivote(campana, new Vector2(.5f, 1));
            var v = F.Asegurar<Vaiven>(campana.gameObject); v.amplitud = 2; v.Golpe(10);
            Sonido.Tocar("campana_grande");
            yield return new WaitForSecondsRealtime(.6f);
            listo = false;
            Resolver("¡La campana está completa!");
        }
    }

    // ---------- El caso: ¿quién es El Campanero? ----------
    // Sospechosos (lineas): "nombre|retrato|rasgos|lo que Ana sabe"; evidencias (opciones): "rasgo:texto".
    // El culpable es el único que tiene TODOS los rasgos de las evidencias; al acusar a otro se dice cuál no encaja.
    public class AcCaso : Acertijo
    {
        string[] nombre, rasgos; string[][] codigos;
        string[] evCodigo, evTexto;
        Image[] tarjetas;
        int sel = -1;

        public override void Construir()
        {
            int n = d.lineas.Length;
            nombre = new string[n]; rasgos = new string[n]; codigos = new string[n][];
            evCodigo = d.opciones.Select(o => o.Split(':')[0].Trim()).ToArray();
            evTexto = d.opciones.Select(o => o.Substring(o.IndexOf(':') + 1).Trim()).ToArray();
            Texto("Evidencias", 6, 0, 270, 30, 20, Pal.MAIZ, TextAnchor.MiddleLeft, 1);
            for (int e = 0; e < evTexto.Length; e++)
            {
                var c = Imagen("UI/papel", 6, 34 + e * 98, 270, 90);
                Texto((e + 1) + ". " + evTexto[e], 14, 4, 246, 82, 15, Pal.CAFE, TextAnchor.MiddleLeft, 0, c.transform);
            }
            tarjetas = new Image[n];
            for (int i = 0; i < n; i++)
            {
                var partes = d.lineas[i].Split('|');
                nombre[i] = partes[0]; codigos[i] = partes[2].Split(',').Select(x => x.Trim()).ToArray();
                int k = i;
                var t = Imagen("UI/papel", 290 + (i % 2) * 215, (i / 2) * 182, 205, 174, null, null, true);
                Tocable(t, () => Elegir(k));
                tarjetas[i] = t;
                Imagen("Retratos/" + partes[1], 8, 8, 70, 70, null, t.transform);
                Texto(partes[0], 82, 6, 118, 70, 16, Pal.CHILE, TextAnchor.MiddleLeft, 1, t.transform);
                var hechos = Texto(partes.Length > 3 ? partes[3].Replace(" · ", "\n") : "", 10, 82, 188, 88, 13, Pal.CAFE, TextAnchor.UpperLeft, 0, t.transform);
                hechos.resizeTextForBestFit = true; hechos.resizeTextMinSize = 9; hechos.resizeTextMaxSize = 13;   // que quepan los 4 datos
            }
            Boton("Acusar", 400, 372, 220, 54, Pal.ROSA, Acusar, null, 22);
        }

        void Elegir(int i)
        {
            if (listo) return;
            sel = i;
            for (int k = 0; k < tarjetas.Length; k++) tarjetas[k].color = k == i ? Pal.MAIZ : Color.white;
            Tw.Brinco(tarjetas[i].transform, .05f, .2f);
            Sonido.Tocar("papel");
        }

        void Acusar()
        {
            if (listo) return;
            if (sel < 0) { Aviso("Toca primero a un sospechoso."); return; }
            int falla = System.Array.FindIndex(evCodigo, c => !codigos[sel].Contains(c));
            if (falla < 0 && sel.ToString() == d.respuesta.Trim()) { Sonido.Tocar("campana_grande"); Resolver(); return; }
            Fallo(falla >= 0 ? "No encaja con la evidencia " + (falla + 1) + ": «" + evTexto[falla] + "»" : "Revisa otra vez las evidencias.");
        }
    }
}
