// Sonido: efectos y música ambiental. Los archivos están en Assets/Resources/Sonido (se pueden reemplazar).
using System.Collections.Generic;
using UnityEngine;

namespace Leyendas
{
    public class Sonido : MonoBehaviour
    {
        static Sonido yo;
        public AudioSource efectos, ambiente;
        [Range(0, 1)] public float volumenEfectos = .8f, volumenAmbiente = .45f;
        readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        public static bool Activo = true;

        void Awake()
        {
            yo = this;
            if (efectos == null) efectos = gameObject.AddComponent<AudioSource>();
            if (ambiente == null) ambiente = gameObject.AddComponent<AudioSource>();
            efectos.playOnAwake = false;
            ambiente.playOnAwake = false; ambiente.loop = true;
        }

        AudioClip Clip(string n)
        {
            AudioClip c;
            if (!clips.TryGetValue(n, out c)) { c = Resources.Load<AudioClip>("Sonido/" + n); clips[n] = c; }
            return c;
        }

        public static void Tocar(string nombre, float tono = 1f, float volumen = 1f)
        {
            if (yo == null || !Activo) return;
            var c = yo.Clip(nombre);
            if (c == null) return;
            yo.efectos.pitch = tono;
            yo.efectos.PlayOneShot(c, volumen * yo.volumenEfectos);
        }

        // Volúmenes de los Ajustes (0..1). La música se mezcla un poco más baja que los efectos.
        public static void Volumenes(float musica, float efectos)
        {
            if (yo == null) return;
            yo.volumenAmbiente = musica * .75f;
            yo.volumenEfectos = efectos;
            yo.ambiente.volume = yo.volumenAmbiente;
        }

        public static void Ambiente(bool sonar)
        {
            if (yo == null) return;
            var c = yo.Clip("ambiente");
            if (c == null) return;
            yo.ambiente.clip = c;
            yo.ambiente.volume = yo.volumenAmbiente;
            if (sonar && Activo) { if (!yo.ambiente.isPlaying) yo.ambiente.Play(); }
            else yo.ambiente.Stop();
        }
    }
}
