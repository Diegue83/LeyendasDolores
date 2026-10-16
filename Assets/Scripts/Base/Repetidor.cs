// Componente separado en su propio archivo para que Unity lo guarde en escenas y prefabs.
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Leyendas
{
    // Botón que repite su acción mientras se mantiene presionado (disco de la caja fuerte).
    public class Repetidor : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public Action accion;
        float t, espera; bool abajo;
        public void OnPointerDown(PointerEventData e) { abajo = true; espera = .4f; t = 0; accion?.Invoke(); }
        public void OnPointerUp(PointerEventData e) { abajo = false; }
        public void OnPointerExit(PointerEventData e) { abajo = false; }
        void Update()
        {
            if (!abajo) return;
            t += Time.unscaledDeltaTime;
            if (t >= espera) { t = 0; espera = .09f; accion?.Invoke(); }
        }
    }
}
