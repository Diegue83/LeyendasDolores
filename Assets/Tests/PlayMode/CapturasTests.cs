// Diagnóstico visual y de entrada real:
//  - Captura cada una de las 15 vistas tal como se ve en el juego (PNG en Temp/Capturas del proyecto).
//  - Hace clic con un MOUSE VIRTUAL del Input System (pasa por el módulo de entrada real, como el jugador)
//    sobre zonas del nivel 1 y comprueba que el juego reaccione.
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Leyendas.Pruebas
{
    public class CapturasTests
    {
        Juego J => Juego.I;

        [UnitySetUp]
        public IEnumerator Preparar()
        {
            Estado.BorrarTodas();
            var anterior = Juego.I;
            SceneManager.LoadScene("Juego", LoadSceneMode.Single);
            yield return null;
            float t = 0;
            while (!(J != null && J != anterior && J.Listo)) { t += Time.unscaledDeltaTime; if (t > 60) Assert.Fail("no cargó"); yield return null; }
        }

        [UnityTearDown] public IEnumerator Limpiar() { Estado.BorrarTodas(); yield return null; }

        static string Carpeta { get { var c = "C:/Users/52418/AppData/Local/Temp/lgen/capturas"; Directory.CreateDirectory(c); return c; } }

        // Dibuja la interfaz con una cámara a una textura de 1280 x 720 (sirve también en modo batch).
        void Capturar(string nombre)
        {
            var canvas = J.GetComponent<Canvas>();
            var cam = Camera.main;
            var rt = new RenderTexture(1280, 720, 24);
            var modo = canvas.renderMode;
            canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = cam; canvas.planeDistance = 5;
            cam.targetTexture = rt;
            Canvas.ForceUpdateCanvases();
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); tex.Apply();
            File.WriteAllBytes(Path.Combine(Carpeta, nombre + ".png"), tex.EncodeToPNG());
            RenderTexture.active = null; cam.targetTexture = null; rt.Release();
            canvas.renderMode = modo;
        }

        [UnityTest, Timeout(300000)]
        public IEnumerator CapturarVistas()
        {
            Capturar("00_portada");
            J.bJugar.onClick.Invoke();
            float t = 0; while (J.VistaActual == "") { t += Time.unscaledDeltaTime; if (t > 15) Assert.Fail("no empezó"); yield return null; }
            foreach (var v in J.vistasRaiz.GetComponentsInChildren<Vista>(true).Select(x => x.id).OrderBy(x => x).ToList())
            {
                J.IrA(v, false);
                yield return new WaitForSecondsRealtime(.3f);
                Capturar(v);
            }
        }

        // Ventana del mapa y acertijos rediseñados (v8), para revisarlos a ojo.
        [UnityTest, Timeout(120000)]
        public IEnumerator CapturarMapaYAcertijos()
        {
            J.bJugar.onClick.Invoke();
            float t = 0; while (J.VistaActual == "") { t += Time.unscaledDeltaTime; if (t > 15) Assert.Fail("no empezó"); yield return null; }
            yield return new WaitForSecondsRealtime(.5f);
            J.E.nivelMax = 1; J.E.piezas.Add("corona");
            J.bMapa.onClick.Invoke();
            yield return new WaitForSecondsRealtime(.6f);
            Capturar("v8_mapa");
            J.mapaLugares[2].onClick.Invoke();
            yield return new WaitForSecondsRealtime(.2f);
            Capturar("v8_mapa_bloqueado");
            J.mapaCerrar.onClick.Invoke();
            yield return null;
            foreach (var id in new[] { "candelabro", "campanas", "nonograma", "anillos", "nieves", "libros" })
            {
                J.AbrirAcertijo(id, null);
                yield return new WaitForSecondsRealtime(.6f);
                Capturar("v8_" + id);
                J.CerrarAcertijo(false);
                yield return new WaitForSecondsRealtime(.3f);
            }
        }

        // Menú principal, ventanas del menú y el tablero del caso (v9).
        [UnityTest, Timeout(120000)]
        public IEnumerator CapturarMenuYCaso()
        {
            yield return new WaitForSecondsRealtime(.8f);
            Capturar("v9_menu");
            J.bPartidas.onClick.Invoke(); yield return new WaitForSecondsRealtime(.5f); Capturar("v9_partidas");
            J.partidasCerrar.onClick.Invoke();
            J.bAjustes.onClick.Invoke(); yield return new WaitForSecondsRealtime(.5f); Capturar("v9_ajustes");
            J.ajCerrar.onClick.Invoke();
            J.bCreditos.onClick.Invoke(); yield return new WaitForSecondsRealtime(.5f); Capturar("v9_creditos");
            J.creditosCerrar.onClick.Invoke();
            J.bJugar.onClick.Invoke();
            float t = 0; while (J.VistaActual == "" || J.portada.activeSelf) { t += Time.unscaledDeltaTime; if (t > 15) Assert.Fail("no empezó"); yield return null; }
            yield return new WaitForSecondsRealtime(1.5f);
            Capturar("v9_intro");
            J.bMenu.onClick.Invoke(); yield return new WaitForSecondsRealtime(.5f); Capturar("v9_pausa");
            J.pausaContinuar.onClick.Invoke(); yield return null;
            J.AbrirAcertijo("caso", null);
            yield return new WaitForSecondsRealtime(.6f);
            Capturar("v9_caso");
            J.CerrarAcertijo(false);
        }

        // Clic real: mouse virtual del Input System → InputSystemUIInputModule → EventSystem → zona.
        IEnumerator ClicMouse(Mouse m, GameObject go)
        {
            var rt = (RectTransform)go.transform;
            Vector2 p = RectTransformUtility.WorldToScreenPoint(null, rt.TransformPoint(rt.rect.center));
            InputSystem.QueueStateEvent(m, new MouseState { position = p });
            yield return null; yield return null;
            InputSystem.QueueStateEvent(m, new MouseState { position = p }.WithButton(MouseButton.Left, true));
            yield return null; yield return null;
            InputSystem.QueueStateEvent(m, new MouseState { position = p }.WithButton(MouseButton.Left, false));
            yield return null; yield return null;
        }

        [UnityTest, Timeout(120000)]
        public IEnumerator ClicConMouseReal_EnElNivel1()
        {
            // En modo batch la ventana no tiene foco: que el Input System acepte la entrada igual (como con el juego enfocado).
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            var m = InputSystem.AddDevice<Mouse>();
            try
            {
                Assert.IsNotNull(EventSystem.current, "no hay EventSystem activo");
                yield return ClicMouse(m, J.bJugar.gameObject);
                float t = 0; while (J.VistaActual != "n1_atrio") { t += Time.unscaledDeltaTime; if (t > 15) Assert.Fail("el clic en Jugar no funcionó (vista=" + J.VistaActual + ", portada=" + J.portada.activeSelf + ")"); yield return null; }
                yield return new WaitForSecondsRealtime(.5f);
                var atrio = J.vistasRaiz.GetComponentsInChildren<Vista>(true).First(v => v.id == "n1_atrio");
                yield return ClicMouse(m, atrio.Zona("placa").gameObject);
                yield return new WaitForSecondsRealtime(.3f);
                Assert.IsTrue(J.E.B("leida_placa"), "el clic con mouse en la placa del atrio no hizo nada. Encima de la placa está: " + Encima(atrio.Zona("placa").gameObject));
                yield return ClicMouse(m, atrio.Zona("puerta_sacristia").gameObject);
                t = 0; while (!J.modal.activeSelf) { t += Time.unscaledDeltaTime; if (t > 5) Assert.Fail("el clic con mouse en la puerta de la sacristía no abrió el candado. Encima: " + Encima(atrio.Zona("puerta_sacristia").gameObject)); yield return null; }
            }
            finally { InputSystem.RemoveDevice(m); }
        }

        static string Encima(GameObject zona)
        {
            var rt = (RectTransform)zona.transform;
            var ped = new PointerEventData(EventSystem.current) { position = RectTransformUtility.WorldToScreenPoint(null, rt.TransformPoint(rt.rect.center)) };
            var r = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(ped, r);
            return string.Join(", ", r.Take(4).Select(x => x.gameObject.name)) + " | pantalla " + Screen.width + "x" + Screen.height;
        }
    }
}
