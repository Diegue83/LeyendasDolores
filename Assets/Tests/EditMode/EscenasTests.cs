// Pruebas del Editor (sin entrar a Play) de la versión nativa:
//  - Cada escena de nivel contiene TODAS las vistas, capas, zonas y planos del original (vistas.json de referencia).
//  - La escena del juego tiene su interfaz guardada y ninguna vista suelta.
//  - Los 20 prefabs de acertijos existen y tienen sus piezas registradas.
//  - Un cambio hecho a mano en una escena se guarda y sigue ahí al volver a abrirla.
//  - El código del juego ya no contiene el generador de escenarios.
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Leyendas.Editor;

namespace Leyendas.Pruebas.Editor
{
    public class EscenasTests
    {
        // Rutas del proyecto (antes se tomaban de la herramienta de migración, que ya no forma parte del repositorio).
        const string ESCENA_JUEGO = "Assets/Escenas/Juego.unity";
        const string CARPETA_NIVELES = "Assets/Escenas/Niveles";
        const string CARPETA_PREFABS = "Assets/Resources/Prefabs/Acertijos";
        static readonly string[] NIVELES = { "N1_Parroquia", "N2_Plaza", "N3_Mercado", "N4_CasaHidalgo" };
        static VistasJ Referencia() => VistasJ.Cargar();

        static List<Vista> VistasDe(Scene esc) => esc.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Vista>(true)).ToList();

        [Test]
        public void EscenasDeNivel_TienenTodoElContenidoOriginal()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var faltas = new List<string>();
            var vistos = new HashSet<string>();
            foreach (var nombre in NIVELES)
            {
                var ruta = CARPETA_NIVELES + "/" + nombre + ".unity";
                Assert.IsTrue(File.Exists(ruta), "falta la escena " + ruta);
                var esc = EditorSceneManager.OpenScene(ruta, OpenSceneMode.Single);
                Assert.IsNotNull(esc.GetRootGameObjects().Select(g => g.GetComponent<NivelEscena>()).FirstOrDefault(n => n != null), nombre + " no tiene raíz de nivel");
                foreach (var v in VistasDe(esc))
                {
                    vistos.Add(v.id);
                    var refv = Referencia().vistas.FirstOrDefault(x => x.id == v.id);
                    if (refv == null) { faltas.Add(v.id + ": no existe en el original"); continue; }
                    if (v.fondo == null || v.fondo.sprite == null) faltas.Add(v.id + ": fondo sin imagen");
                    foreach (var c in refv.capas)
                    {
                        var capa = v.capas.FirstOrDefault(k => k != null && k.nombre == c.nombre);
                        if (capa == null) faltas.Add(v.id + ": falta la capa " + c.nombre);
                        else if (capa.imagen == null || capa.imagen.sprite == null) faltas.Add(v.id + "/" + c.nombre + ": sin imagen");
                    }
                    foreach (var z in refv.zonas)
                        if (!v.zonas.Any(k => k != null && k.nombre == z.nombre)) faltas.Add(v.id + ": falta la zona " + z.nombre);
                    int planos = refv.planos != null ? refv.planos.Count : 0;
                    var par = v.GetComponent<Paralaje>();
                    if ((par != null ? par.planos.Count(p => p != null) : 0) != planos) faltas.Add(v.id + ": planos " + (par != null ? par.planos.Count : 0) + " de " + planos);
                    // cada plano es un contenedor: debe tener su imagen o elementos dentro, todos con su sprite
                    if (par != null) foreach (var p in par.planos)
                    {
                        var imgs = p.GetComponentsInChildren<UnityEngine.UI.Image>(true);
                        if (imgs.Length == 0) faltas.Add(v.id + "/" + p.name + ": plano vacío");
                        foreach (var im in imgs) if (im.sprite == null) faltas.Add(v.id + "/" + p.name + "/" + im.name + ": sin imagen");
                    }
                }
            }
            foreach (var r in Referencia().vistas) if (!vistos.Contains(r.id)) faltas.Add("vista omitida: " + r.id);
            Assert.IsEmpty(faltas, string.Join("\n", faltas));
        }

        [Test]
        public void EscenaDelJuego_TieneSuInterfazGuardada()
        {
            var esc = EditorSceneManager.OpenScene(ESCENA_JUEGO, OpenSceneMode.Single);
            var j = esc.GetRootGameObjects().Select(g => g.GetComponentInChildren<Juego>(true)).FirstOrDefault(x => x != null);
            Assert.IsNotNull(j, "la escena del juego no tiene el componente Juego");
            Assert.IsNotNull(j.escenario); Assert.IsNotNull(j.modalContenido); Assert.IsNotNull(j.tarjeta); Assert.IsNotNull(j.portada);
            Assert.AreEqual(8, j.ranuras.Count(r => r != null), "la mochila debe tener 8 ranuras");
            Assert.AreEqual(0, j.vistasRaiz.childCount, "las vistas viven en las escenas de nivel, no en la del juego");
            Assert.IsNotNull(Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>(), "falta el EventSystem");
            var lista = EditorBuildSettings.scenes.Select(s => s.path).ToList();
            Assert.AreEqual(ESCENA_JUEGO, lista[0], "la escena del juego debe ser la primera en Build Profiles");
            foreach (var n in NIVELES) Assert.Contains(CARPETA_NIVELES + "/" + n + ".unity", lista);
        }

        [Test]
        public void PrefabsDeAcertijos_Existen()
        {
            var faltas = new List<string>();
            foreach (var d in DatosJuego.D.acertijos)
            {
                var ruta = CARPETA_PREFABS + "/Acertijo_" + d.id + ".prefab";
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(ruta);
                if (go == null) { faltas.Add("falta " + ruta); continue; }
                var pl = go.GetComponent<PlantillaUI>();
                if (pl == null || pl.elementos.Count == 0) faltas.Add(d.id + ": sin piezas");
                else if (pl.elementos.Any(e => e == null)) faltas.Add(d.id + ": piezas rotas");
            }
            Assert.IsEmpty(faltas, string.Join("\n", faltas));
        }

        [Test]
        public void CambioManual_PersisteAlGuardarYReabrir()
        {
            var ruta = CARPETA_NIVELES + "/N2_Plaza.unity";
            var esc = EditorSceneManager.OpenScene(ruta, OpenSceneMode.Single);
            var chuy = VistasDe(esc).First(v => v.id == "n2_plaza").Capa("pj_chuy");
            var rt = (RectTransform)chuy.transform;
            var original = rt.anchoredPosition;
            rt.anchoredPosition = original + new Vector2(7, 3);
            EditorSceneManager.MarkSceneDirty(esc);
            EditorSceneManager.SaveScene(esc);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            esc = EditorSceneManager.OpenScene(ruta, OpenSceneMode.Single);
            rt = (RectTransform)VistasDe(esc).First(v => v.id == "n2_plaza").Capa("pj_chuy").transform;
            var leido = rt.anchoredPosition;
            rt.anchoredPosition = original;                 // se deja como estaba
            EditorSceneManager.MarkSceneDirty(esc);
            EditorSceneManager.SaveScene(esc);
            Assert.AreEqual(original + new Vector2(7, 3), leido, "el cambio manual no se conservó");
        }

        [Test]
        public void ElJuegoYaNoIncluyeElGenerador()
        {
            var juego = typeof(Juego).Assembly;
            Assert.IsNull(juego.GetType("Leyendas.Constructor"), "el Constructor sigue en el código del juego");
            Assert.IsNull(juego.GetType("Leyendas.Editor.MigracionNativa"));
        }
    }
}
