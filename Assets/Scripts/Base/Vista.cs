// Componente separado en su propio archivo para que Unity lo guarde en escenas y prefabs.
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Leyendas
{
    // Una vista del escenario. Sus hijos: Fondo, Capas (parches y recortes), Zonas (tocables) y Objetos (recogibles).
    public class Vista : MonoBehaviour
    {
        public string id;
        public Image fondo;
        public List<Capa> capas = new List<Capa>();
        public List<ZonaInteractiva> zonas = new List<ZonaInteractiva>();

        public Capa Capa(string nombre) { return capas.Find(c => c != null && c.nombre == nombre); }
        public ZonaInteractiva Zona(string nombre)
        {
            var z = zonas.Find(c => c != null && c.nombre == nombre);
            if (z == null) { var c = Capa(nombre); if (c != null) z = c.GetComponent<ZonaInteractiva>(); }
            return z;
        }
    }
}
