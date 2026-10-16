// Vistas del escenario (cada lugar al que se puede entrar), sus capas y sus zonas tocables.
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Leyendas
{
    // ---- Formato de Assets/Resources/Arte/vistas.json (lo escribe FuentesArte/generador/render6.py) ----
    // tipo: "elemento" (decoración propia), "recorte" (personaje u objeto), "parche" (estado). padre: elemento del que es
    // estado (se mueve con él). plano: plano de profundidad donde va (vacío = la escena).
    [Serializable] public class CapaJ { public string nombre, tipo, archivo, padre, plano; public float x, y, w, h; }
    [Serializable] public class ZonaJ { public string nombre; public float x, y, w, h; }
    // Plano de profundidad (estilo Silksong): se dibuja detrás o delante de la escena y se mueve con paralaje.
    // paralaje < 0 = lejos (se mueve poco y al revés) · > 0 = primer plano (se mueve más). x, y, w, h incluyen el margen extra.
    [Serializable] public class PlanoJ { public string nombre, archivo; public float paralaje; public bool delante; public float x, y, w, h; }
    [Serializable] public class VistaJ { public string id, fondo; public List<CapaJ> capas = new List<CapaJ>(); public List<ZonaJ> zonas = new List<ZonaJ>(); public List<PlanoJ> planos = new List<PlanoJ>(); }
    [Serializable]
    public class VistasJ
    {
        public List<VistaJ> vistas = new List<VistaJ>();
        public static VistasJ Cargar()
        {
            var t = Resources.Load<TextAsset>("Arte/vistas");
            if (t == null) { Debug.LogError("Leyendas: falta Assets/Resources/Arte/vistas.json"); return new VistasJ(); }
            return JsonUtility.FromJson<VistasJ>(t.text) ?? new VistasJ();
        }
    }

}
