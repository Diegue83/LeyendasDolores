// Validación "guardar, cerrar Unity y volver a abrir" (dos ejecuciones separadas de Unity):
//   1) -executeMethod Leyendas.Pruebas.Editor.ValidacionReinicio.Marcar     → mueve y recolorea piezas y guarda
//   2) -executeMethod Leyendas.Pruebas.Editor.ValidacionReinicio.Verificar  → (otro arranque de Unity) comprueba y deja todo como estaba
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Leyendas.Pruebas.Editor
{
    public static class ValidacionReinicio
    {
        const string ESCENA = "Assets/Escenas/Niveles/N2_Plaza.unity";
        const string PREFAB = "Assets/Resources/Prefabs/Acertijos/Acertijo_candado.prefab";
        const string NOTA = "Library/validacion_reinicio.txt";
        static readonly Vector2 MOVER = new Vector2(11, -6);
        static readonly Color COLOR = new Color(.2f, .6f, .9f, 1f);

        static RectTransform Chuy()
        {
            var esc = EditorSceneManager.OpenScene(ESCENA, OpenSceneMode.Single);
            return (RectTransform)esc.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Vista>(true)).First(v => v.id == "n2_plaza").Capa("pj_chuy").transform;
        }

        public static void Marcar()
        {
            var rt = Chuy();
            var pos = rt.anchoredPosition;
            rt.anchoredPosition = pos + MOVER;
            EditorSceneManager.MarkSceneDirty(rt.gameObject.scene);
            EditorSceneManager.SaveScene(rt.gameObject.scene);
            var raiz = PrefabUtility.LoadPrefabContents(PREFAB);
            var boton = raiz.GetComponentsInChildren<Button>(true).Last();
            var color = boton.GetComponent<Image>().color;
            boton.GetComponent<Image>().color = COLOR;
            PrefabUtility.SaveAsPrefabAsset(raiz, PREFAB);
            PrefabUtility.UnloadPrefabContents(raiz);
            File.WriteAllText(NOTA, pos.x + ";" + pos.y + ";" + ColorUtility.ToHtmlStringRGBA(color));
            Debug.Log("VALIDACION_MARCADA");
            EditorApplication.Exit(0);
        }

        public static void Verificar()
        {
            var datos = File.ReadAllText(NOTA).Split(';');
            var pos = new Vector2(float.Parse(datos[0]), float.Parse(datos[1]));
            Color original; ColorUtility.TryParseHtmlString("#" + datos[2], out original);
            var rt = Chuy();
            bool escenaOk = (rt.anchoredPosition - (pos + MOVER)).sqrMagnitude < .001f;
            var raiz = PrefabUtility.LoadPrefabContents(PREFAB);
            var img = raiz.GetComponentsInChildren<Button>(true).Last().GetComponent<Image>();
            bool prefabOk = (img.color - COLOR).maxColorComponent < .002f;
            // se deja todo como estaba
            img.color = original;
            PrefabUtility.SaveAsPrefabAsset(raiz, PREFAB);
            PrefabUtility.UnloadPrefabContents(raiz);
            rt.anchoredPosition = pos;
            EditorSceneManager.MarkSceneDirty(rt.gameObject.scene);
            EditorSceneManager.SaveScene(rt.gameObject.scene);
            File.Delete(NOTA);
            Debug.Log("VALIDACION_ESCENA=" + (escenaOk ? "OK" : "FALLO") + " VALIDACION_PREFAB=" + (prefabOk ? "OK" : "FALLO"));
            EditorApplication.Exit(escenaOk && prefabOk ? 0 : 1);
        }
    }
}
