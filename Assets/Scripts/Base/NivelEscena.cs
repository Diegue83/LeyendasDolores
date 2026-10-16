// Componente separado en su propio archivo para que Unity lo guarde en escenas y prefabs.
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Leyendas
{
    // Raíz de una escena de nivel (Assets/Escenas/Niveles/*.unity). Sus vistas se editan aquí, en el Editor;
    // al jugar, el Juego las pasa a su escenario. El Canvas de esta raíz solo sirve para verlas al editar.
    public class NivelEscena : MonoBehaviour
    {
        public int nivel;
    }
}
