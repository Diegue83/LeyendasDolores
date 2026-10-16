// Componente separado en su propio archivo para que Unity lo guarde en escenas y prefabs.
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Leyendas
{
    // Sonido al tocar un botón (se guarda con la escena, a diferencia de los eventos agregados por código).
    public class SonidoBoton : MonoBehaviour, IPointerClickHandler
    {
        public string sonido = "toque";
        public void OnPointerClick(PointerEventData e) { var s = GetComponent<Selectable>(); if (s == null || s.interactable) Sonido.Tocar(sonido); }
    }
}
