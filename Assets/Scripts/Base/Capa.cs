// Componente separado en su propio archivo para que Unity lo guarde en escenas y prefabs.
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Leyendas
{
    // Capa dibujada sobre el fondo. Parche = cambio de estado (puerta abierta...). Recorte = elemento con transparencia.
    public class Capa : MonoBehaviour
    {
        public string nombre;
        public bool esParche, esObjeto;
        public Image imagen;
    }
}
