// Utilidad del Editor: en cada escena de nivel deja encendida solo la vista inicial del nivel (atrio, plaza, pasillo,
// sala) para que al abrir la escena se vea lo que el jugador ve primero. No cambia nada más de las escenas.
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Leyendas.Editor
{
    public static class VistaInicialEscenas
    {
        static readonly string[] ESCENAS = { "N1_Parroquia", "N2_Plaza", "N3_Mercado", "N4_CasaHidalgo" };

        [MenuItem("Leyendas/Escenas/Encender la vista inicial de cada nivel", false, 60)]
        public static void Aplicar()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var actual = EditorSceneManager.GetActiveScene().path;
            for (int n = 0; n < ESCENAS.Length; n++)
            {
                var esc = EditorSceneManager.OpenScene("Assets/Escenas/Niveles/" + ESCENAS[n] + ".unity", OpenSceneMode.Single);
                string inicial = DatosJuego.D.Nivel(n).vistaInicial;
                foreach (var v in esc.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Vista>(true)))
                    v.gameObject.SetActive(v.id == inicial);
                EditorSceneManager.MarkSceneDirty(esc);
                EditorSceneManager.SaveScene(esc);
            }
            if (!string.IsNullOrEmpty(actual)) EditorSceneManager.OpenScene(actual, OpenSceneMode.Single);
            Debug.Log("Leyendas: en cada escena de nivel quedó encendida su vista inicial.");
        }

        public static void AplicarDesdeConsola() { Aplicar(); EditorApplication.Exit(0); }
    }
}
