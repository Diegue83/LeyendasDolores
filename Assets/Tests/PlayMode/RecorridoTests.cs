// Pruebas de juego (Play Mode) de la versión nativa.
//  - Recorrido completo de los 4 niveles tocando las zonas reales (clic simulado de la interfaz), recogiendo y
//    combinando objetos, abriendo cada acertijo desde su prefab y llegando al final.
//  - Antes de cada toque se comprueba que la zona no esté tapada por ninguna otra imagen (regla de construcción:
//    nada se dibuja encima de lo que se puede tocar).
//  - Cinco acertijos se resuelven con sus botones reales del prefab (candado, criptex, César, ruta y campana);
//    el resto se da por resuelto con su lógica para seguir el recorrido.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Leyendas.Pruebas
{
    public class RecorridoTests
    {
        Juego J => Juego.I;
        readonly List<string> avisos = new List<string>();
        void Registrar(string msg, string pila, LogType t) { if (t == LogType.Warning || t == LogType.Error || t == LogType.Exception) avisos.Add(t + ": " + msg); }

        [UnitySetUp]
        public IEnumerator Preparar()
        {
            Estado.BorrarTodas();
            avisos.Clear();
            Application.logMessageReceived += Registrar;
            var anterior = Juego.I;   // LoadScene ocurre hasta el siguiente cuadro: hay que esperar al Juego NUEVO
            SceneManager.LoadScene("Juego", LoadSceneMode.Single);
            yield return null;
            yield return Esperar(() => J != null && J != anterior && J.Listo, 60, "el juego no terminó de cargar sus escenas de nivel");
            J.velocidadTexto = 4000;
            J.duracionTransicion = .05f;
            // En batch Unity simula 640x480 (4:3). El juego está pensado para teléfono horizontal (16:9 o más ancho):
            // se escala por ancho para que la interfaz completa quepa, como en esos teléfonos.
            if ((float)Screen.width / Screen.height < 16f / 9f) J.GetComponent<CanvasScaler>().matchWidthOrHeight = 0f;
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator Limpiar()
        {
            Application.logMessageReceived -= Registrar;
            Estado.BorrarTodas();
            yield return null;
        }

        // ------------------------------------------------------------ utilidades
        static IEnumerator Esperar(Func<bool> cond, float seg, string falla)
        {
            float t = 0;
            while (!cond())
            {
                t += Time.unscaledDeltaTime;
                if (t > seg) Assert.Fail("Tiempo agotado: " + falla + "  [" + Diagnostico() + "]");
                yield return null;
            }
        }

        static string Diagnostico()
        {
            var j = Juego.I;
            if (j == null) return "sin Juego";
            int n = j.vistasRaiz != null ? j.vistasRaiz.childCount : -1;
            bool tr = false; try { tr = (bool)typeof(Juego).GetField("enTransicion", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(j); } catch { }
            return "vista=" + j.VistaActual + " portada=" + (j.portada != null && j.portada.activeSelf) + " empezada=" + (j.E != null && j.E.empezada)
                + " vistas=" + n + " transicion=" + tr + " modal=" + j.modal.activeSelf + " tarjeta=" + j.tarjeta.activeSelf
                + " listo=" + j.Listo + " escenas=" + SceneManager.sceneCount + " tiempo=" + Time.timeScale
                + " | dialogo: " + Dialogo(j) + " || HISTORIA: " + string.Join(" || ", historia);
        }

        static readonly List<string> historia = new List<string>();
        static IEnumerator VigilarDialogo()
        {
            var f = BindingFlags.NonPublic | BindingFlags.Instance;
            string antes = null;
            while (true)
            {
                var j = Juego.I;
                if (j != null)
                {
                    var actual = typeof(Juego).GetField("lineaActual", f).GetValue(j);
                    string texto = actual == null ? "-" : (string)actual.GetType().GetField("texto").GetValue(actual);
                    bool completa = (bool)typeof(Juego).GetField("completa", f).GetValue(j);
                    int cola = ((ICollection)typeof(Juego).GetField("cola", f).GetValue(j)).Count;
                    string clave = texto + "|" + completa + "|" + cola + "|" + j.VistaActual + "|" + j.modal.activeSelf;
                    if (clave != antes) { historia.Add(Time.unscaledTime.ToString("0.00") + " " + clave); antes = clave; if (historia.Count > 40) historia.RemoveAt(0); }
                }
                yield return null;
            }
        }

        static string Dialogo(Juego j)
        {
            try
            {
                var f = BindingFlags.NonPublic | BindingFlags.Instance;
                var cola = (ICollection)typeof(Juego).GetField("cola", f).GetValue(j);
                var actual = typeof(Juego).GetField("lineaActual", f).GetValue(j);
                bool completa = (bool)typeof(Juego).GetField("completa", f).GetValue(j);
                string texto = actual == null ? "(ninguna)" : (string)actual.GetType().GetField("texto").GetValue(actual);
                object despues = actual == null ? null : actual.GetType().GetField("despues").GetValue(actual);
                return "t=" + Time.unscaledTime.ToString("0.0") + " cola=" + cola.Count + " completa=" + completa + " conAccion=" + (despues != null) + " actual=«" + texto + "»";
            }
            catch (Exception e) { return "?" + e.Message; }
        }

        static T Privado<T>(object o, string campo) => (T)o.GetType().GetField(campo, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(o);
        bool EnTransicion => Privado<bool>(J, "enTransicion");

        Vista VistaObj(string id) => J.vistasRaiz.GetComponentsInChildren<Vista>(true).First(v => v.id == id);

        IEnumerator Libre() { yield return Esperar(() => !EnTransicion && !J.modal.activeSelf && !J.tarjeta.activeSelf, 20, "la pantalla no quedó libre"); }

        // Comprueba que lo que está encima de la zona (en su centro) sea la zona misma.
        public static void ComprobarAlcanzable(GameObject zona, string que)
        {
            var rt = (RectTransform)zona.transform;
            var pantalla = RectTransformUtility.WorldToScreenPoint(null, rt.TransformPoint(rt.rect.center));
            var ped = new PointerEventData(EventSystem.current) { position = pantalla };
            var r = new List<RaycastResult>();
            EventSystem.current.RaycastAll(ped, r);
            Assert.IsTrue(r.Count > 0, que + ": nada recibe el toque en su centro");
            var arriba = r[0].gameObject;
            Assert.IsTrue(arriba == zona || arriba.transform.IsChildOf(zona.transform),
                que + ": la tapa «" + arriba.name + "» (" + string.Join(" > ", Ruta(arriba.transform)) + ")");
        }
        static IEnumerable<string> Ruta(Transform t) { var l = new List<string>(); while (t != null) { l.Insert(0, t.name); t = t.parent; } return l.Skip(Math.Max(0, l.Count - 4)); }

        static void Clic(GameObject go)
        {
            var ped = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute(go, ped, ExecuteEvents.pointerClickHandler);
        }

        IEnumerator Tocar(string vista, string zona)
        {
            yield return Libre();
            yield return Esperar(() => J.VistaActual == vista, 15, "no se llegó a la vista " + vista + " (está en " + J.VistaActual + ")");
            var z = VistaObj(vista).Zona(zona);
            Assert.IsNotNull(z, "no existe la zona " + vista + "/" + zona);
            Assert.IsTrue(z.gameObject.activeInHierarchy, "la zona " + vista + "/" + zona + " no está visible");
            ComprobarAlcanzable(z.gameObject, vista + "/" + zona);
            Clic(z.gameObject);
            yield return null;
        }

        IEnumerator Volver(string a)
        {
            yield return Libre();
            Assert.IsTrue(J.bVolver.gameObject.activeInHierarchy, "no hay flecha para regresar desde " + J.VistaActual);
            ComprobarAlcanzable(J.bVolver.gameObject, "flecha de regresar");
            J.bVolver.onClick.Invoke();
            yield return Esperar(() => J.VistaActual == a, 10, "la flecha no regresó a " + a);
        }

        int Ranura(string obj) { int i = J.E.mochila.IndexOf(obj); Assert.GreaterOrEqual(i, 0, "no está en la mochila: " + obj); return i; }

        IEnumerator Combinar(string a, string b)
        {
            yield return Libre();
            Clic(J.ranuras[Ranura(a)].gameObject); yield return null;
            Clic(J.ranuras[Ranura(b)].gameObject); yield return null;
        }

        // Espera el acertijo abierto desde su prefab y lo resuelve (con su lógica o con una función de botones).
        IEnumerator Acertijo(string id, Func<Transform, IEnumerator> conBotones = null)
        {
            yield return Esperar(() => J.modal.activeSelf, 10, "no se abrió el acertijo " + id);
            var inst = J.modalContenido.Find("Acertijo_" + id);
            Assert.IsNotNull(inst, "el acertijo " + id + " no se armó desde su prefab");
            Assert.IsFalse(avisos.Any(a => a.Contains("pidió")), "el prefab de " + id + " no coincide con su lógica: " + string.Join(" | ", avisos));
            var ac = Privado<Acertijo>(J, "acertijo");
            yield return null;
            if (conBotones != null) yield return conBotones(inst);
            else ac.GetType().GetMethod("Resolver", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(ac, new object[] { "¡Lo lograste!" });
            // Sin pantalla de resultado: el acertijo se cierra solo (la valoración llega al terminar el nivel).
            float t = 0;
            while (J.modal.activeSelf)
            {
                Assert.IsFalse(J.resultado.activeSelf, "el acertijo " + id + " mostró una pantalla de resultado");
                t += Time.unscaledDeltaTime;
                if (t > 10) Assert.Fail("el acertijo " + id + " no se cerró solo al resolverse [" + Diagnostico() + "]");
                yield return null;
            }
            yield return null;
        }

        // Abre un acertijo suelto (sin el recorrido) y devuelve sus botones en el orden en que se armaron.
        IEnumerator AbrirSuelto(string id, List<Button> botones)
        {
            J.AbrirAcertijo(id, () => { });
            yield return null;
            var inst = J.modalContenido.Find("Acertijo_" + id);
            Assert.IsNotNull(inst, "el acertijo " + id + " no se armó desde su prefab");
            Assert.IsFalse(avisos.Any(a => a.Contains("pidió")), "el prefab de " + id + " no coincide con su lógica: " + string.Join(" | ", avisos));
            botones.Clear(); botones.AddRange(Botones(inst));
        }

        IEnumerator EsperarCierre(string id)
        {
            float t = 0;
            while (J.modal.activeSelf)
            {
                Assert.IsFalse(J.resultado.activeSelf, id + ": se mostró una pantalla de resultado");
                t += Time.unscaledDeltaTime;
                if (t > 10) Assert.Fail(id + ": no se resolvió con sus piezas");
                yield return null;
            }
        }

        static List<Button> Botones(Transform t) => t.GetComponentsInChildren<Button>(true).ToList();
        static IEnumerator Pulsar(Button b, int veces = 1) { for (int i = 0; i < veces; i++) { Assert.IsTrue(b.onClick.GetPersistentEventCount() + 1 > 0); b.onClick.Invoke(); yield return null; } }

        // Como un jugador: mientras espera, toca el panel de diálogo para leer más rápido.
        IEnumerator AvanzarDialogoHasta(Func<bool> cond, float seg, string falla)
        {
            float t = 0, toque = 0;
            while (!cond())
            {
                t += Time.unscaledDeltaTime; toque += Time.unscaledDeltaTime;
                if (toque > .15f) { toque = 0; Clic(J.dialogoPanel.gameObject); }
                if (t > seg) Assert.Fail("Tiempo agotado: " + falla + "  [" + Diagnostico() + "]");
                yield return null;
            }
        }

        IEnumerator SiguienteLugar(string vistaInicial)
        {
            yield return AvanzarDialogoHasta(() => J.tarjeta.activeSelf, 60, "no apareció la tarjeta de fin de nivel");
            var b = J.tarjetaBotones.GetComponentsInChildren<Button>().FirstOrDefault(x => x.GetComponentInChildren<Text>().text.Contains("Siguiente"));
            Assert.IsNotNull(b, "la tarjeta no tiene el botón «Siguiente lugar»");
            b.onClick.Invoke();
            yield return Esperar(() => J.VistaActual == vistaInicial, 15, "no se entró a " + vistaInicial);
        }

        // ------------------------------------------------------------ recorrido
        [UnityTest, Timeout(900000)]
        public IEnumerator RecorridoCompleto_DeLaPortadaAlFinal()
        {
            historia.Clear();
            J.StartCoroutine(VigilarDialogo());
            Assert.IsTrue(J.portada.activeSelf, "no se muestra la portada");
            J.bJugar.onClick.Invoke();
            yield return Esperar(() => J.VistaActual == "n1_atrio", 15, "Jugar no llevó al atrio");

            // ---------------- Nivel 1 · Parroquia ----------------
            yield return Tocar("n1_atrio", "placa");
            yield return Tocar("n1_atrio", "puerta_sacristia");
            yield return Acertijo("candado", inst =>
            {
                var b = Botones(inst);   // arriba0, abajo0, arriba1, abajo1, arriba2, abajo2, arriba3, abajo3, Abrir
                return Secuencia(Pulsar(b[1], 1), Pulsar(b[3], 6), Pulsar(b[7], 9), Pulsar(b[8]));
            });
            Assert.IsTrue(J.E.B("ok_candado"));
            yield return Esperar(() => J.VistaActual == "n1_sacristia", 10, "no se entró a la sacristía");
            yield return Tocar("n1_sacristia", "obj_vara");
            yield return Tocar("n1_sacristia", "obj_vela");
            yield return Tocar("n1_sacristia", "cuadro");
            yield return Acertijo("deslizante");
            yield return Tocar("n1_sacristia", "obj_llave_campanario");
            yield return Tocar("n1_sacristia", "puerta_nave");
            yield return Tocar("n1_nave", "roseton");
            yield return Acertijo("anillos");
            yield return Tocar("n1_nave", "obj_partitura");
            yield return Tocar("n1_nave", "obj_cerillos");
            yield return Combinar("vela", "cerillos");
            Assert.IsTrue(J.E.Lleva("vela_encendida"), "no se encendió la vela");
            yield return Volver("n1_atrio");
            yield return Tocar("n1_atrio", "puerta_torre");
            yield return Esperar(() => J.VistaActual == "n1_torre", 10, "no se entró a la torre");
            yield return Tocar("n1_torre", "candelabro");
            yield return Acertijo("candelabro");
            yield return Tocar("n1_torre", "escalera");
            yield return Tocar("n1_campanario", "campanas");
            yield return Acertijo("campanas");
            yield return Tocar("n1_campanario", "obj_corona");
            Assert.IsTrue(J.E.piezas.Contains("corona"));
            yield return SiguienteLugar("n2_plaza");

            // ---------------- Nivel 2 · Plaza ----------------
            yield return Tocar("n2_plaza", "chuy");
            yield return Acertijo("nieves");
            yield return Tocar("n2_plaza", "obj_ficha3");
            yield return Tocar("n2_plaza", "obj_ficha6");
            yield return Tocar("n2_plaza", "kiosko");
            yield return Esperar(() => J.VistaActual == "n2_kiosko", 10, "no se entró al kiosko");
            yield return Tocar("n2_kiosko", "obj_ficha1");
            yield return Tocar("n2_kiosko", "papel_picado");
            yield return Acertijo("nonograma");
            yield return Volver("n2_plaza");
            yield return Tocar("n2_plaza", "neveria");
            yield return Tocar("n2_neveria", "obj_ficha2");
            yield return Tocar("n2_neveria", "jarras");
            yield return Acertijo("jarras");
            yield return Volver("n2_plaza");
            yield return Tocar("n2_plaza", "estatua");
            yield return Tocar("n2_estatua", "placa");
            yield return Acertijo("cronologia");
            yield return Tocar("n2_estatua", "compartimento");
            yield return Acertijo("criptex", inst =>
            {
                // ALDAMA con el alfabeto ABCDEFGHIJKLMNÑOPQRSTUVWXYZ: A=0, L=11, D=3, M=12
                var b = Botones(inst);
                int[] pasos = { 0, 11, 3, 0, 12, 0 };
                var l = new List<IEnumerator>();
                for (int i = 0; i < 6; i++) l.Add(Pulsar(b[i * 2 + 1], pasos[i]));
                l.Add(Pulsar(b[12]));
                return Secuencia(l.ToArray());
            });
            Assert.IsTrue(J.E.piezas.Contains("yugo"));
            Assert.IsFalse(Enumerable.Range(1, 6).Any(i => J.E.Lleva("ficha" + i)), "las fichas debían gastarse en el criptex");
            yield return SiguienteLugar("n3_pasillo");

            // ---------------- Nivel 3 · Mercado ----------------
            yield return Tocar("n3_pasillo", "lupita");
            yield return Acertijo("balanza");
            yield return Tocar("n3_pasillo", "talavera");
            yield return Tocar("n3_talavera", "azulejos");
            yield return Acertijo("canal");
            yield return Volver("n3_pasillo");
            yield return Tocar("n3_pasillo", "pinata");
            yield return Acertijo("pinata");
            yield return Tocar("n3_pasillo", "obj_cajita");
            yield return Tocar("n3_pasillo", "bodega");
            yield return Esperar(() => J.VistaActual == "n3_bodega", 10, "no se entró a la bodega");
            yield return Tocar("n3_bodega", "ollas");
            yield return Acertijo("hanoi");
            yield return Volver("n3_pasillo");
            yield return Libre();
            Clic(J.ranuras[Ranura("cajita")].gameObject); yield return null;
            Clic(J.ranuras[Ranura("cajita")].gameObject); yield return null;
            yield return Acertijo("simbolos");
            Assert.IsTrue(J.E.piezas.Contains("badajo"));
            yield return SiguienteLugar("n4_sala");

            // ---------------- Nivel 4 · Casa de Hidalgo ----------------
            yield return Tocar("n4_sala", "escritorio");
            yield return Tocar("n4_sala", "obj_rueda");
            yield return Tocar("n4_sala", "obj_carta");
            yield return Tocar("n4_sala", "obj_cerillos");
            yield return Tocar("n4_sala", "puerta_biblioteca");
            yield return Tocar("n4_biblioteca", "obj_vela");
            yield return Combinar("vela", "cerillos");
            yield return Combinar("vela_encendida", "carta");
            Assert.IsTrue(J.E.Lleva("carta_revelada"), "no se reveló la carta");
            yield return Combinar("rueda", "carta_revelada");
            yield return Acertijo("cesar", inst => { var b = Botones(inst); return Secuencia(Pulsar(b[1], 4), Pulsar(b[2])); });
            yield return Tocar("n4_biblioteca", "libros");
            yield return Acertijo("libros");
            yield return Tocar("n4_biblioteca", "caja_fuerte");
            yield return Acertijo("caja");
            Assert.IsTrue(J.E.Lleva("llave_cofre"));
            yield return Volver("n4_sala");
            yield return Tocar("n4_sala", "patio");
            yield return Tocar("n4_patio", "cofre");
            yield return Acertijo("ruta", inst => { var b = Botones(inst); return Secuencia(Pulsar(b[1]), Pulsar(b[2]), Pulsar(b[3]), Pulsar(b[4])); });
            Assert.IsTrue(J.E.piezas.Contains("cuerpo"));
            yield return Volver("n4_sala");
            yield return Tocar("n4_sala", "vitrina");
            // El caso: se acusa a Doña Lupita (tarjeta 3) con el botón «Acusar»; después de la confesión se arma la campana.
            Assert.IsTrue(new[] { "ev_llave", "ev_pie", "ev_azul", "ev_zurdo" }.All(J.E.B), "faltan evidencias del caso: " + string.Join(",", J.E.banderas.Where(x => x.StartsWith("ev_"))));
            yield return Acertijo("caso", inst => { var b = Botones(inst); return Secuencia(Pulsar(b[2]), Pulsar(b[4])); });
            yield return AvanzarDialogoHasta(() => J.modal.activeSelf, 40, "después de la confesión no se abrió el armado de la campana");
            // botones mezclados: corona, badajo, yugo, cuerpo → orden correcto yugo, corona, cuerpo, badajo
            yield return Acertijo("final", inst => { var b = Botones(inst); return Secuencia(Pulsar(b[2]), Pulsar(b[0]), Pulsar(b[3]), Pulsar(b[1])); });
            yield return AvanzarDialogoHasta(() => J.tarjeta.activeSelf, 60, "no apareció la tarjeta final");
            Assert.IsTrue(J.E.terminada, "el juego no quedó terminado");
            Assert.AreEqual(4, J.E.partida.Count, "faltan resultados de nivel");
            Assert.Greater(J.E.Total, 0);
            Assert.IsFalse(avisos.Any(a => a.StartsWith("Error") || a.StartsWith("Exception")), "errores durante el recorrido: " + string.Join(" | ", avisos));
        }

        static IEnumerator Secuencia(params IEnumerator[] pasos) { foreach (var p in pasos) yield return p; }

        // ------------------------------------------------------------ zonas, prefabs y planos
        [UnityTest, Timeout(300000)]
        public IEnumerator TodasLasZonas_SeAlcanzanEnSuVista()
        {
            J.bJugar.onClick.Invoke();
            yield return Esperar(() => J.VistaActual != "", 15, "no empezó el juego");
            var faltas = new List<string>();
            foreach (var v in J.vistasRaiz.GetComponentsInChildren<Vista>(true).ToList())
            {
                J.IrA(v.id, false);
                yield return null;
                // se muestran todos los objetos y estados para revisar que nada tape lo que se toca
                foreach (var z in v.zonas.Where(z => z != null && z.gameObject.activeInHierarchy))
                {
                    try { ComprobarAlcanzable(z.gameObject, v.id + "/" + z.nombre); }
                    catch (AssertionException ex) { faltas.Add(ex.Message); }
                }
            }
            Assert.IsEmpty(faltas, string.Join("\n", faltas));
        }

        [UnityTest, Timeout(300000)]
        public IEnumerator CadaAcertijo_SeArmaDesdeSuPrefab()
        {
            J.bJugar.onClick.Invoke();
            yield return Esperar(() => J.VistaActual != "", 15, "no empezó el juego");
            var faltas = new List<string>();
            foreach (var d in J.D.acertijos)
            {
                avisos.Clear();
                J.AbrirAcertijo(d.id, null);
                yield return null;
                var inst = J.modalContenido.Find("Acertijo_" + d.id);
                if (inst == null) faltas.Add(d.id + ": no se armó desde su prefab");
                else
                {
                    var pl = inst.GetComponent<PlantillaUI>();
                    if (pl == null || pl.elementos.Count == 0) faltas.Add(d.id + ": el prefab no tiene piezas registradas");
                    if (pl != null && pl.elementos.Any(e => e == null)) faltas.Add(d.id + ": el prefab tiene piezas borradas");
                    foreach (var b in Botones(inst)) if (b.GetComponent<Repetidor>() == null && b.onClick.GetPersistentEventCount() == 0 && !TieneOyentes(b)) faltas.Add(d.id + ": el botón «" + b.name + "» no hace nada");
                    if (inst.GetComponentsInChildren<Transform>(true).Length - 1 != pl.elementos.Count) faltas.Add(d.id + ": se crearon piezas fuera del prefab");
                }
                if (avisos.Any()) faltas.Add(d.id + ": " + string.Join(" | ", avisos));
                J.CerrarAcertijo(false);
                yield return Esperar(() => !J.modal.activeSelf, 5, "no se cerró " + d.id);
            }
            Assert.IsEmpty(faltas, string.Join("\n", faltas));
        }

        // ------------------------------------------------------------ v8: acertijos rediseñados, mapa y diálogo
        [UnityTest, Timeout(120000)]
        public IEnumerator AcertijosRediseñados_SeResuelvenConSusPiezas()
        {
            J.bJugar.onClick.Invoke();
            yield return Esperar(() => J.VistaActual != "", 15, "no empezó el juego");
            var b = new List<Button>();

            // Candelabro: la flama solo pasa por los brazos; un salto sin brazo es error, y el recorrido único enciende las 9.
            yield return AbrirSuelto("candelabro", b);
            var ac = Privado<Acertijo>(J, "acertijo");
            b[4].onClick.Invoke(); yield return null;
            b[2].onClick.Invoke(); yield return null;       // 4 y 2 no están unidas
            Assert.AreEqual(1, ac.errores, "pasar la flama sin brazo debe contar como error");
            b[9].onClick.Invoke(); yield return null;       // «Apagar todas»
            foreach (int v in new[] { 0, 4, 3, 6, 7, 8, 5, 1, 2 }) { b[v].onClick.Invoke(); yield return null; }
            yield return EsperarCierre("candelabro");

            // Campanas: la grande es grave y la chica aguda; la melodía se lee en la partitura.
            yield return AbrirSuelto("campanas", b);
            b[3].onClick.Invoke(); yield return null;       // «Tocar la melodía»
            int[] campanaDeAltura = { 1, 0, 2 };            // altura 0 = grande (botón 1), 1 = mediana (0), 2 = chica (2)
            foreach (int h in J.D.Acertijo("campanas").lineas[0].Split(',').Select(int.Parse)) { b[campanaDeAltura[h]].onClick.Invoke(); yield return null; }
            yield return EsperarCierre("campanas");

            // Anillos en ciclo: se busca la combinación de giros y se pulsa con los botones reales.
            yield return AbrirSuelto("anillos", b);
            var real = Privado<Logica.Anillos>(Privado<Acertijo>(J, "acertijo"), "z");
            int[] giros = null;
            for (int g0 = 0; g0 < 8 && giros == null; g0++)
                for (int g1 = 0; g1 < 8 && giros == null; g1++)
                    for (int g2 = 0; g2 < 8 && giros == null; g2++)
                    {
                        var sim = new Logica.Anillos(3, 8, new[] { 1, -1, 2 }, 1);
                        Array.Copy(real.pos, sim.pos, 3);
                        sim.Girar(0, g0); sim.Girar(1, g1); sim.Girar(2, g2);
                        if (sim.Resuelto) giros = new[] { g0, g1, g2 };
                    }
            Assert.IsNotNull(giros, "los anillos no tienen solución");
            for (int i = 0; i < 3; i++) for (int v = 0; v < giros[i]; v++) { b[i * 2 + 1].onClick.Invoke(); yield return null; }
            yield return EsperarCierre("anillos");

            // El caso: acusar a Don Tomás (diestro) es error; a Doña Lupita, que encaja con las 4 evidencias, lo resuelve.
            yield return AbrirSuelto("caso", b);
            ac = Privado<Acertijo>(J, "acertijo");
            b[0].onClick.Invoke(); yield return null; b[4].onClick.Invoke(); yield return null;
            Assert.AreEqual(1, ac.errores, "acusar a Don Tomás debe ser un error");
            StringAssert.Contains("zurdo", J.modalMensaje.text, "el error debe decir qué evidencia no encaja");
            b[2].onClick.Invoke(); yield return null; b[4].onClick.Invoke(); yield return null;
            yield return EsperarCierre("caso");

            // Papel picado: la campana se recorta casilla por casilla.
            yield return AbrirSuelto("nonograma", b);
            var filas = J.D.Acertijo("nonograma").lineas;
            int cols = filas[0].Length;
            for (int f = 0; f < filas.Length; f++) for (int c = 0; c < cols; c++) if (filas[f][c] == '#') { b[f * cols + c].onClick.Invoke(); yield return null; }
            yield return EsperarCierre("nonograma");
        }

        [UnityTest, Timeout(60000)]
        public IEnumerator Mapa_MuestraLosLugares_PausaElReloj_YViaja()
        {
            J.bJugar.onClick.Invoke();
            yield return Esperar(() => J.VistaActual != "", 15, "no empezó el juego");
            yield return Libre();
            J.bMapa.onClick.Invoke();
            yield return null;
            Assert.IsTrue(J.mapa.activeSelf, "no se abrió la ventana del mapa");
            Assert.AreEqual(4, J.mapaLugares.Count(x => x != null && x.gameObject.activeInHierarchy), "faltan marcadores de lugares");
            Assert.IsFalse(J.mapaViajar.interactable, "no se debe poder viajar al lugar donde ya se está");
            float t0 = J.E.tiempo[0];
            yield return new WaitForSecondsRealtime(.5f);
            Assert.AreEqual(t0, J.E.tiempo[0], .001f, "el reloj del nivel siguió corriendo con el mapa abierto");
            J.mapaLugares[2].onClick.Invoke(); yield return null;
            Assert.IsFalse(J.mapaViajar.interactable, "un lugar bloqueado no debe permitir viajar");
            J.E.nivelMax = 1;
            J.mapaLugares[1].onClick.Invoke(); yield return null;
            Assert.IsTrue(J.mapaViajar.interactable, "un lugar abierto debe permitir viajar");
            J.mapaViajar.onClick.Invoke();
            yield return Esperar(() => !J.mapa.activeSelf && J.VistaActual.StartsWith("n2_"), 15, "el mapa no llevó a la Plaza");
        }

        [UnityTest, Timeout(60000)]
        public IEnumerator Dialogo_SeReemplazaAlTocarOtroLugar()
        {
            J.bJugar.onClick.Invoke();
            yield return Esperar(() => J.VistaActual == "n1_atrio", 15, "no empezó en el atrio");
            yield return Libre();
            J.velocidadTexto = 15;   // escritura lenta, como la ve el jugador
            J.DecirTexto("ana", "", "TEXTO VIEJO número uno que tarda bastante en escribirse completo.");
            J.DecirTexto("ana", "", "TEXTO VIEJO número dos.");
            yield return new WaitForSecondsRealtime(.2f);
            yield return Tocar("n1_atrio", "placa");
            yield return new WaitForSecondsRealtime(.3f);
            Assert.IsFalse(J.textoDialogo.text.Contains("VIEJO"), "el diálogo siguió con el texto anterior: «" + J.textoDialogo.text + "»");
            var nuevo = J.D.Dialogo("n1_placa")[0].texto;   // se está escribiendo: debe ser el comienzo de la línea de la placa
            Assert.IsTrue(J.textoDialogo.text.Length > 0 && nuevo.StartsWith(J.textoDialogo.text), "el diálogo no pasó a lo que se acaba de tocar: «" + J.textoDialogo.text + "»");
        }

        // ------------------------------------------------------------ v9: menú, partidas, pausa y reiniciar
        [UnityTest, Timeout(90000)]
        public IEnumerator Menu_PartidasPausaReiniciarAjustesYCreditos()
        {
            Assert.IsTrue(J.portada.activeSelf, "no empezó en el menú principal");
            // Nueva partida en la ranura 2 (vacía: no pide confirmación)
            J.bPartidas.onClick.Invoke(); yield return null;
            Assert.IsTrue(J.partidas.activeSelf, "no se abrió Partidas");
            Assert.IsFalse(J.ranuraJugar[1].gameObject.activeSelf, "una ranura vacía no debe ofrecer Continuar");
            J.ranuraNueva[1].onClick.Invoke();
            yield return Esperar(() => J.VistaActual == "n1_atrio" && !J.portada.activeSelf, 15, "no empezó la partida 2");
            Assert.AreEqual(1, Estado.Ranura, "no quedó en uso la ranura 2");
            yield return Libre();
            yield return Tocar("n1_atrio", "placa");
            Assert.IsTrue(J.E.B("leida_placa"));
            yield return new WaitForSecondsRealtime(.3f);

            // Pausa: detiene el reloj
            J.bMenu.onClick.Invoke(); yield return null;
            Assert.IsTrue(J.pausa.activeSelf, "no se abrió la pausa");
            float tp = J.E.tiempo[0];
            yield return new WaitForSecondsRealtime(.4f);
            Assert.AreEqual(tp, J.E.tiempo[0], .001f, "el reloj siguió con la pausa abierta");

            // Reiniciar este lugar (con confirmación): vuelve como al llegar
            Assert.IsTrue(J.pausaReiniciar.interactable, "no se puede reiniciar el lugar");
            J.pausaReiniciar.onClick.Invoke(); yield return null;
            Assert.IsTrue(J.confirmar.activeSelf, "reiniciar no pidió confirmación");
            J.confSi.onClick.Invoke(); yield return null;
            Assert.IsFalse(J.E.B("leida_placa"), "reiniciar no borró el avance del lugar");
            Assert.Less(J.E.tiempo[0], 1f, "reiniciar no regresó el tiempo del nivel");
            Assert.AreEqual("n1_atrio", J.VistaActual);
            Assert.IsFalse(J.pausa.activeSelf);

            // Ajustes desde la pausa
            yield return Libre();
            J.bMenu.onClick.Invoke(); yield return null;
            J.pausaAjustes.onClick.Invoke(); yield return null;
            Assert.IsTrue(J.ajustes.activeSelf, "no se abrieron los Ajustes");
            float musica = Ajustes.Musica; int texto = Ajustes.Texto;
            J.ajMusicaMenos.onClick.Invoke();
            Assert.Less(Ajustes.Musica, musica, "el volumen de la música no bajó");
            J.ajTextoMas.onClick.Invoke(); J.ajTextoMas.onClick.Invoke();
            Assert.AreEqual(2, Ajustes.Texto);
            Assert.AreEqual(Ajustes.VelocidadTexto, J.velocidadTexto, "la velocidad del texto no se aplicó");
            Ajustes.Musica = musica; Ajustes.Texto = texto; Ajustes.Guardar();
            J.ajCerrar.onClick.Invoke(); yield return null;

            // Menú principal y Créditos
            J.pausaMenu.onClick.Invoke(); yield return null;
            Assert.IsTrue(J.portada.activeSelf, "no volvió al menú principal");
            StringAssert.Contains("Partida 2", J.portadaPartida.text);
            J.bCreditos.onClick.Invoke(); yield return null;
            Assert.IsTrue(J.creditos.activeSelf && J.creditosTexto.text.Length > 20, "los créditos no se ven");
            J.creditosCerrar.onClick.Invoke(); yield return null;

            // La partida 2 quedó guardada y se puede continuar
            J.bPartidas.onClick.Invoke(); yield return null;
            Assert.IsTrue(J.ranuraJugar[1].gameObject.activeSelf, "la partida 2 no aparece guardada");
            J.ranuraJugar[1].onClick.Invoke();
            yield return Esperar(() => !J.portada.activeSelf && J.VistaActual == "n1_atrio", 15, "no se continuó la partida 2");
        }

        static bool TieneOyentes(Button b)
        {
            var campo = typeof(UnityEngine.Events.UnityEventBase).GetField("m_Calls", BindingFlags.NonPublic | BindingFlags.Instance);
            var llamadas = campo.GetValue(b.onClick);
            var cuenta = llamadas.GetType().GetProperty("Count", BindingFlags.Public | BindingFlags.Instance);
            if (cuenta != null) return (int)cuenta.GetValue(llamadas) > 0;
            var runtime = llamadas.GetType().GetField("m_RuntimeCalls", BindingFlags.NonPublic | BindingFlags.Instance);
            return runtime != null && ((System.Collections.ICollection)runtime.GetValue(llamadas)).Count > 0;
        }

        [UnityTest]
        public IEnumerator PlanosDeProfundidad_NoBloqueanToques_YSeMueven()
        {
            J.bJugar.onClick.Invoke();
            yield return Esperar(() => J.VistaActual != "", 15, "no empezó el juego");
            J.IrA("n2_plaza", false);
            yield return null;
            var par = VistaObj("n2_plaza").GetComponent<Paralaje>();
            Assert.IsNotNull(par, "la plaza no tiene paralaje");
            Assert.AreEqual(3, par.planos.Count);
            foreach (var p in par.planos)
            {
                var piezas = p.GetComponentsInChildren<Graphic>(true);
                Assert.IsNotEmpty(piezas, "el plano " + p.name + " está vacío");
                foreach (var g in piezas) Assert.IsFalse(g.raycastTarget, "la pieza " + g.name + " del plano " + p.name + " bloquea toques");
            }
            var antes = par.planos.Select(p => p.anchoredPosition).ToList();
            par.Empujar(new Vector2(1, 0));
            yield return new WaitForSecondsRealtime(.6f);
            Assert.IsTrue(par.planos.Select((p, i) => (p.anchoredPosition - antes[i]).magnitude).Any(m => m > .5f), "los planos no se movieron");
        }
    }
}
