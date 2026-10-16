// Ajustes del jugador (PlayerPrefs). Son de la instalación, no de la partida: valen para las 3 ranuras.
using UnityEngine;

namespace Leyendas
{
    public static class Ajustes
    {
        public static float Musica = .6f, Efectos = .8f;   // 0..1
        public static int Texto = 1;                       // 0 lenta, 1 normal, 2 rápida
        public static readonly string[] NOMBRE_TEXTO = { "Lenta", "Normal", "Rápida" };
        static readonly float[] VELOCIDAD = { 30f, 55f, 100f };   // letras por segundo
        public static float VelocidadTexto { get { return VELOCIDAD[Mathf.Clamp(Texto, 0, 2)]; } }

        // La pantalla completa solo tiene sentido en computadora.
        public static bool HayPantallaCompleta { get { return !Application.isMobilePlatform && Application.platform != RuntimePlatform.WebGLPlayer; } }

        public static void Cargar()
        {
            Musica = PlayerPrefs.GetFloat("leyendas_musica", .6f);
            Efectos = PlayerPrefs.GetFloat("leyendas_efectos", .8f);
            Texto = Mathf.Clamp(PlayerPrefs.GetInt("leyendas_texto", 1), 0, 2);
        }

        public static void Guardar()
        {
            PlayerPrefs.SetFloat("leyendas_musica", Musica);
            PlayerPrefs.SetFloat("leyendas_efectos", Efectos);
            PlayerPrefs.SetInt("leyendas_texto", Texto);
            PlayerPrefs.Save();
        }
    }
}
