// Componente separado en su propio archivo para que Unity lo guarde en escenas y prefabs.
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Leyendas
{
    // Parpadeo de Ana en el retrato: cambia el sprite un instante cada pocos segundos.
    public class Parpadeo : MonoBehaviour
    {
        public Image imagen;
        public Sprite abierto, cerrado;
        float siguiente = 2f, cerradoHasta = -1;
        void Update()
        {
            if (imagen == null || cerrado == null) return;
            if (cerradoHasta > 0 && Time.time > cerradoHasta) { imagen.sprite = abierto; cerradoHasta = -1; }
            if (Time.time > siguiente)
            {
                if (imagen.sprite == abierto) { imagen.sprite = cerrado; cerradoHasta = Time.time + .13f; }
                siguiente = Time.time + UnityEngine.Random.Range(2.4f, 5f);
            }
        }
    }
}
