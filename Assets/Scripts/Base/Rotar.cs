// Componente separado en su propio archivo para que Unity lo guarde en escenas y prefabs.
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Leyendas
{
    // Gira constantemente (chispas, destellos).
    public class Rotar : MonoBehaviour
    {
        public float gradosPorSegundo = 40f;
        void Update() { transform.Rotate(0, 0, gradosPorSegundo * Time.deltaTime); }
    }
}
