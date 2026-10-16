// Componente separado en su propio archivo para que Unity lo guarde en escenas y prefabs.
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Leyendas
{
    // Flota hacia arriba y abajo (indicadores, objetos brillantes).
    public class Flotar : MonoBehaviour
    {
        public float altura = 6f, velocidad = 2.4f;
        RectTransform rt; Vector2 base0; bool listo;
        void OnEnable() { rt = transform as RectTransform; if (rt != null) { base0 = rt.anchoredPosition; listo = true; } }
        void OnDisable() { if (listo && rt != null) rt.anchoredPosition = base0; }
        void Update() { if (listo) rt.anchoredPosition = base0 + Vector2.up * altura * Mathf.Sin(Time.time * velocidad); }
    }
}
