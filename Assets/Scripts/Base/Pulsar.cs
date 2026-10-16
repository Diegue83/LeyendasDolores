// Componente separado en su propio archivo para que Unity lo guarde en escenas y prefabs.
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Leyendas
{
    // Botón que se hunde un poco al tocarlo.
    public class Pulsar : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public float escala = .93f;
        Vector3 normal = Vector3.one; bool abajo;
        void Awake() { normal = transform.localScale; }
        public void OnPointerDown(PointerEventData e) { var b = GetComponent<Selectable>(); if (b != null && !b.interactable) return; abajo = true; transform.localScale = normal * escala; }
        public void OnPointerUp(PointerEventData e) { if (abajo) transform.localScale = normal; abajo = false; }
        public void OnPointerExit(PointerEventData e) { if (abajo) transform.localScale = normal; abajo = false; }
        void OnDisable() { transform.localScale = normal; abajo = false; }
    }
}
