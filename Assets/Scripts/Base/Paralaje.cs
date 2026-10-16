// Componente separado en su propio archivo para que Unity lo guarde en escenas y prefabs.
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Leyendas
{
    // Paralaje de los planos de una vista. La escena (fondo, capas y zonas) no se mueve; los planos de atrás se
    // deslizan poco y al revés, y el primer plano se desliza más. En reposo hay un vaivén muy suave, y al tocar algo
    // la "cámara" se inclina un poco hacia ese punto.
    public class Paralaje : MonoBehaviour
    {
        public List<RectTransform> planos = new List<RectTransform>();
        public List<float> factores = new List<float>();
        public List<Vector2> bases = new List<Vector2>();
        [Tooltip("Desplazamiento en px de un plano con paralaje 1")] public float amplitud = 22f;
        [Tooltip("Vaivén en reposo (0 = quieto)")] [Range(0, 1)] public float vaiven = .35f;
        [Tooltip("Qué tan rápido sigue la cámara")] public float suavidad = 2.5f;
        Vector2 empuje, camara;

        public void Agregar(RectTransform rt, float factor)
        {
            planos.Add(rt); factores.Add(factor); bases.Add(rt.anchoredPosition);
        }

        // dir: posición tocada respecto al centro del escenario, de -1 a 1.
        public void Empujar(Vector2 dir) { empuje = Vector2.ClampMagnitude(dir, 1f); }

        void OnDisable()
        {
            camara = empuje = Vector2.zero;
            for (int i = 0; i < planos.Count && i < bases.Count; i++) if (planos[i]) planos[i].anchoredPosition = bases[i];
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime, t = Time.unscaledTime;
            empuje = Vector2.Lerp(empuje, Vector2.zero, 1 - Mathf.Exp(-dt * .5f));
            var meta = new Vector2(Mathf.Sin(t * .23f), Mathf.Sin(t * .17f + 1f) * .5f) * vaiven + empuje;
            camara = Vector2.Lerp(camara, meta, 1 - Mathf.Exp(-dt * suavidad));
            for (int i = 0; i < planos.Count && i < factores.Count && i < bases.Count; i++)
                if (planos[i]) planos[i].anchoredPosition = bases[i] - camara * factores[i] * amplitud;
        }
    }
}
