// Componente separado en su propio archivo para que Unity lo guarde en escenas y prefabs.
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Leyendas
{
    // Brillo que sube y baja (focos, rayos de luz, chispas).
    public class Brillo : MonoBehaviour
    {
        public float minimo = .55f, maximo = 1f, velocidad = 1.6f, fase = 0f;
        Graphic g;
        void Awake() { g = GetComponent<Graphic>(); }
        void Update()
        {
            if (g == null) return;
            var c = g.color;
            c.a = Mathf.Lerp(minimo, maximo, .5f + .5f * Mathf.Sin(Time.time * velocidad + fase));
            g.color = c;
        }
    }
}
