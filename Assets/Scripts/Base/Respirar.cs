// Componente separado en su propio archivo para que Unity lo guarde en escenas y prefabs.
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Leyendas
{
    // Respiración de los personajes (escala vertical suave desde los pies). "Hablar" los hace rebotar.
    public class Respirar : MonoBehaviour
    {
        public float cantidad = .012f, velocidad = 2.2f;
        float habla;
        public void Hablar(float segundos = 1.2f) { habla = Mathf.Max(habla, segundos); }
        void Update()
        {
            habla = Mathf.Max(0, habla - Time.deltaTime);
            float s = 1 + cantidad * Mathf.Sin(Time.time * velocidad);
            float rebote = habla > 0 ? Mathf.Abs(Mathf.Sin(Time.time * 11f)) * .025f : 0;
            transform.localScale = new Vector3(1, s + rebote, 1);
        }
    }
}
