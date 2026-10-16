// Componente separado en su propio archivo para que Unity lo guarde en escenas y prefabs.
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Leyendas
{
    // Vaivén alrededor del pivote (campanas, piñata, papel picado). "Golpe" agrega un impulso que se apaga.
    public class Vaiven : MonoBehaviour
    {
        public float amplitud = 3f, velocidad = 1.3f, fase = 0f;
        float impulso;
        public void Golpe(float grados = 14f) { impulso = grados; }
        void Update()
        {
            impulso = Mathf.MoveTowards(impulso, 0, Time.deltaTime * 6f);
            float ang = amplitud * Mathf.Sin(Time.time * velocidad + fase) + impulso * Mathf.Sin(Time.time * 9f);
            transform.localRotation = Quaternion.Euler(0, 0, ang);
        }
    }
}
