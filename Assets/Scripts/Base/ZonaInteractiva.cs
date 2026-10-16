// Componente separado en su propio archivo para que Unity lo guarde en escenas y prefabs.
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Leyendas
{
    // Rectángulo que se puede tocar. Avisa al juego con el nombre de la vista y de la zona.
    public class ZonaInteractiva : MonoBehaviour, IPointerClickHandler
    {
        public string vista, nombre;
        public void OnPointerClick(PointerEventData e)
        {
            if (Juego.I != null) Juego.I.TocarZona(vista, nombre, this);
        }
    }
}
