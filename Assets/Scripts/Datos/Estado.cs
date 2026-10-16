// Partidas guardadas (PlayerPrefs, en JSON), en 3 ranuras. La ranura 0 usa la clave de siempre (compatible con
// las partidas anteriores). Menú "Leyendas > Borrar partida guardada" borra las tres.
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Leyendas
{
    [Serializable]
    public class Record
    {
        public string id;
        public int puntos, estrellas, errores, pistas;
        public float segundos;
    }

    [Serializable]
    public class Estado
    {
        const string CLAVE = "leyendas_dolores_v7", CLAVE_RANURA = "leyendas_ranura";
        public const int RANURAS = 3;
        static string Clave(int r) { return r <= 0 ? CLAVE : CLAVE + "_" + r; }

        // Ranura en uso (se recuerda entre sesiones).
        public static int Ranura
        {
            get { return Mathf.Clamp(PlayerPrefs.GetInt(CLAVE_RANURA, 0), 0, RANURAS - 1); }
            set { PlayerPrefs.SetInt(CLAVE_RANURA, Mathf.Clamp(value, 0, RANURAS - 1)); PlayerPrefs.Save(); }
        }

        public string guardada = "";   // fecha y hora del último guardado (dd/mm/aaaa hh:mm)
        // Foto de la partida al entrar por primera vez a cada nivel (JSON). "Reiniciar nivel" vuelve a ella.
        public string[] inicio = new string[4];

        public int nivel;              // nivel actual (0..3)
        public int nivelMax;           // nivel más alto desbloqueado
        public string vista = "";
        public bool empezada, terminada, sonido = true;
        public List<string> mochila = new List<string>();
        public List<string> banderas = new List<string>();
        public List<string> piezas = new List<string>();
        public List<Record> records = new List<Record>();   // mejor resultado de cada nivel (id = n1..n4); sobrevive a "Nueva partida"
        public List<Record> partida = new List<Record>();   // resultado de cada nivel en esta partida

        // Partida en curso de cada nivel: tiempo, errores y pistas de acertijos acumulados.
        public float[] tiempo = new float[4];
        public int[] errores = new int[4];
        public int[] pistas = new int[4];
        public List<int> completos = new List<int>();     // niveles terminados en esta partida
        public List<Record> acertijos = new List<Record>(); // errores y pistas de cada acertijo (id = acertijo); se muestran al terminar el nivel

        public bool B(string b) { return banderas.Contains(b); }
        public void Poner(string b) { if (!banderas.Contains(b)) banderas.Add(b); }
        public bool Lleva(string o) { return mochila.Contains(o); }
        public bool Completo(int n) { return completos.Contains(n); }

        public int Total { get { return partida.Sum(r => r.puntos); } }
        public int Estrellas { get { return partida.Sum(r => r.estrellas); } }
        public Record Mejor(string id) { return records.Find(r => r.id == id); }
        public Record DePartida(string id) { return partida.Find(r => r.id == id); }

        public void Reparar()
        {
            if (tiempo == null || tiempo.Length != 4) tiempo = new float[4];
            if (errores == null || errores.Length != 4) errores = new int[4];
            if (pistas == null || pistas.Length != 4) pistas = new int[4];
            if (completos == null) completos = new List<int>();
            if (mochila == null) mochila = new List<string>();
            if (banderas == null) banderas = new List<string>();
            if (piezas == null) piezas = new List<string>();
            if (records == null) records = new List<Record>();
            if (partida == null) partida = new List<Record>();
            if (acertijos == null) acertijos = new List<Record>();
            if (inicio == null || inicio.Length != 4) inicio = new string[4];
        }

        // Anota el resultado de la partida y lo guarda como récord si es mejor. Devuelve true si es récord nuevo.
        public bool Registrar(Record r)
        {
            partida.RemoveAll(x => x.id == r.id);
            partida.Add(r);
            var viejo = Mejor(r.id);
            if (viejo == null) { records.Add(r); return true; }
            if (r.puntos <= viejo.puntos) return false;
            records.Remove(viejo); records.Add(r);
            return true;
        }

        public void Guardar()
        {
            guardada = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            PlayerPrefs.SetString(Clave(Ranura), JsonUtility.ToJson(this));
            PlayerPrefs.Save();
        }

        // Copia de la partida sin las fotos de nivel (para guardarla como foto sin anidarlas).
        public string Foto()
        {
            var copia = JsonUtility.FromJson<Estado>(JsonUtility.ToJson(this));
            copia.inicio = new string[4];
            return JsonUtility.ToJson(copia);
        }

        public float TiempoTotal { get { return tiempo.Sum(); } }

        public static Estado Cargar() { return Cargar(Ranura); }
        public static bool Existe(int r) { return !string.IsNullOrEmpty(PlayerPrefs.GetString(Clave(r), "")); }

        public static Estado Cargar(int r)
        {
            var json = PlayerPrefs.GetString(Clave(r), "");
            if (string.IsNullOrEmpty(json)) return new Estado();
            try { var e = JsonUtility.FromJson<Estado>(json) ?? new Estado(); e.Reparar(); return e; }
            catch (Exception) { return new Estado(); }
        }

        public static void Borrar() { Borrar(Ranura); }
        public static void Borrar(int r) { PlayerPrefs.DeleteKey(Clave(r)); PlayerPrefs.Save(); }
        public static void BorrarTodas() { for (int r = 0; r < RANURAS; r++) PlayerPrefs.DeleteKey(Clave(r)); PlayerPrefs.DeleteKey(CLAVE_RANURA); PlayerPrefs.Save(); }
    }
}
