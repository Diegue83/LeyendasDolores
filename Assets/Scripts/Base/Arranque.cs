// Arranque al dar Play (no construye nada: el juego y los niveles ya están guardados en sus escenas).
//  - Con Assets/Escenas/Juego.unity abierta: juego normal (portada).
//  - Con una escena de nivel abierta (Assets/Escenas/Niveles/*.unity): se carga Juego.unity encima y la partida empieza
//    directamente en ESE nivel, en modo prueba (con las piezas de los niveles anteriores y sin tocar la partida guardada).
//  - Se asegura de que haya un EventSystem (uno solo).
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Leyendas
{
    public static class Arranque
    {
        public const string ESCENA_JUEGO = "Juego";

        // Nivel (0..3) que se está probando al dar Play en su escena; -1 = juego normal.
        public static int NivelDePrueba { get; private set; } = -1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reiniciar() { NivelDePrueba = -1; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Iniciar()
        {
            Application.targetFrameRate = 60;
            if (Object.FindAnyObjectByType<Juego>() == null)
            {
                var nivel = Object.FindAnyObjectByType<NivelEscena>(FindObjectsInactive.Include);
                if (nivel != null)
                {
                    NivelDePrueba = nivel.nivel;
                    Debug.Log("Leyendas: prueba del nivel " + (nivel.nivel + 1) + " (se carga Juego.unity; la partida guardada no se toca).");
                    SceneManager.LoadScene(ESCENA_JUEGO, LoadSceneMode.Additive);   // trae la interfaz y su EventSystem
                    return;
                }
                Debug.LogWarning("Leyendas: abre Assets/Escenas/Juego.unity (o una escena de nivel) para jugar.");
            }
            AsegurarEventSystem();
        }

        public static void AsegurarEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            es.AddComponent<StandaloneInputModule>();
#endif
        }
    }
}
