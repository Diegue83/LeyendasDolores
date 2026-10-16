// Componente separado en su propio archivo para que Unity lo guarde en escenas y prefabs.
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Leyendas
{
    // Clic genérico (panel de diálogo, fondo de tarjetas).
    public class AlTocar : MonoBehaviour, IPointerClickHandler
    {
        public Action accion;
        public void OnPointerClick(PointerEventData e) { accion?.Invoke(); }
    }
}
