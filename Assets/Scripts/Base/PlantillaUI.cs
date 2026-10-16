// Componente separado en su propio archivo para que Unity lo guarde en escenas y prefabs.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Leyendas
{
    // Lista de las piezas de un prefab de acertijo, en el orden en que la lógica las pide.
    // La llena una sola vez la herramienta de migración; después el prefab se edita libremente en Unity
    // (mover, escalar, cambiar sprite o color...). Para ocultar una pieza, desactívala en lugar de borrarla.
    public class PlantillaUI : MonoBehaviour
    {
        public List<RectTransform> elementos = new List<RectTransform>();
    }
}
