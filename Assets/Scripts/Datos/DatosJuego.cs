// Datos editables del juego (Assets/Resources/DatosJuego.asset). Cambia aquí textos, códigos, tiempos par y pistas
// sin tocar el código. Si el asset no existe, se usan los valores de DatosPorDefecto.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Leyendas
{
    [Serializable]
    public class NivelDatos
    {
        public string id;            // n1..n4
        public string nombre;        // "Parroquia de Nuestra Señora de los Dolores"
        public string pieza;         // corona, yugo, badajo, cuerpo
        public string nombrePieza;   // "la corona"
        public string vistaInicial;
        [Tooltip("Segundos para 3 estrellas en todo el nivel (0 = calcularlo con los acertijos)")] public float tiempoPar;
        [TextArea(2, 6)] public string objetivo;
        [TextArea(2, 6)] public string cierre;
    }

    [Serializable]
    public class VistaDatos
    {
        public string id;        // n1_atrio
        public string nombre;    // "Atrio"
        public string padre;     // vista a la que regresa la flecha (vacío = ninguna)
    }

    [Serializable]
    public class AcertijoDatos
    {
        public string id;
        public string titulo;
        [TextArea(2, 5)] public string instrucciones;
        public float tiempoPar = 60f;          // segundos estimados; suman al tiempo par del nivel si este es 0
        public string respuesta;               // código o solución (según el acertijo)
        public string[] opciones = new string[0];  // sabores, lugares, símbolos...
        public string[] lineas = new string[0];    // pistas del enunciado, hechos, plano, etc.
        [TextArea(1, 4)] public string[] pistas = new string[0]; // ayudas (cada una cuesta puntos)
        public int semilla = 7;
    }

    [Serializable]
    public class ObjetoDatos
    {
        public string id;
        public string nombre;
        [TextArea(1, 4)] public string descripcion;
    }

    [Serializable]
    public class LineaDatos
    {
        public string clave;      // varias líneas con la misma clave se dicen en orden
        public string quien = "ana";  // ana, chuy, lupita, alfarero, pista
        public string expresion = "normal"; // normal, feliz, pensando, sorpresa (solo Ana)
        [TextArea(1, 5)] public string texto;
    }

    [Serializable]
    public class CombinacionDatos
    {
        public string a, b, resultado;
        [TextArea(1, 3)] public string mensaje;
        public bool conservarA, conservarB;
    }

    [CreateAssetMenu(fileName = "DatosJuego", menuName = "Leyendas/Datos del juego")]
    public class DatosJuego : ScriptableObject
    {
        public const int VERSION = 9;
        [Tooltip("Versión de los datos. Si es menor a la del juego, se rellenan con los valores originales.")] public int version;
        public string titulo = "Leyendas de Dolores Hidalgo";
        public string subtitulo = "El misterio de la campana";
        [Tooltip("Texto de la ventana de Créditos (admite <b>negritas</b> y <size=..>)")]
        [TextArea(6, 20)] public string creditos =
            "<size=30><b>Leyendas de Dolores Hidalgo</b></size>\n" +
            "<i>El misterio de la campana</i>\n" +
            "\n" +
            "<b>Creado por</b>\n" +
            "Juan Diego Juárez Cruz\n" +
            "Zayda Fernanda Vargas Vargas\n" +
            "<size=16>Idea, diseño, historia, acertijos, arte y programación</size>\n" +
            "\n" +
            "<b>Universidad Tecnológica del Norte de Guanajuato</b>\n" +
            "Ingeniería en Desarrollo y Gestión de Software · 2026\n" +
            "\n" +
            "<b>Herramientas</b>\n" +
            "Hecho con Unity. Arte, textos, código y sonidos con ayuda de Claude (Anthropic)\n" +
            "\n" +
            "<b>Tipografías</b>\n" +
            "Lilita One · Juan Montoreano (SIL Open Font License)\n" +
            "Atkinson Hyperlegible · Braille Institute (SIL Open Font License)\n" +
            "\n" +
            "<b>Gracias a</b>\n" +
            "Dolores Hidalgo, Cuna de la Independencia Nacional, Guanajuato";
        [Tooltip("Puntos que se restan al nivel por cada pista pedida dentro de un acertijo")] public int costoPista = 100;
        public List<NivelDatos> niveles = new List<NivelDatos>();
        public List<VistaDatos> vistas = new List<VistaDatos>();
        public List<AcertijoDatos> acertijos = new List<AcertijoDatos>();
        public List<ObjetoDatos> objetos = new List<ObjetoDatos>();
        public List<CombinacionDatos> combinaciones = new List<CombinacionDatos>();
        public List<LineaDatos> dialogos = new List<LineaDatos>();

        static DatosJuego actual;
        public static DatosJuego D
        {
            get
            {
                if (actual == null)
                {
                    actual = Resources.Load<DatosJuego>("DatosJuego");
                    if (actual == null || actual.version < VERSION)
                    {
                        if (actual != null) Debug.LogWarning("Leyendas: DatosJuego es de una versión anterior; se usan los valores de la v9. Usa el menú Leyendas > 1. Construir proyecto editable para actualizarlo.");
                        actual = CreateInstance<DatosJuego>();
                        DatosPorDefecto.Llenar(actual);
                    }
                }
                return actual;
            }
        }

        public AcertijoDatos Acertijo(string id) { return acertijos.Find(a => a.id == id) ?? new AcertijoDatos { id = id, titulo = id }; }
        public ObjetoDatos Objeto(string id) { return objetos.Find(o => o.id == id) ?? new ObjetoDatos { id = id, nombre = id, descripcion = "" }; }
        public VistaDatos Vista(string id) { return vistas.Find(v => v.id == id) ?? new VistaDatos { id = id, nombre = id, padre = "" }; }
        public NivelDatos Nivel(int i) { return niveles[Mathf.Clamp(i, 0, niveles.Count - 1)]; }
        public List<LineaDatos> Dialogo(string clave) { return dialogos.FindAll(l => l.clave == clave); }
    }
}
