using System;
using System.Collections.Generic;
using System.Linq;

// ============================================================================================
//  LÓGICA DE LOS ACERTIJOS (C# puro, sin Unity)
//  Aquí solo están las reglas: qué movimientos valen y cuándo un acertijo está resuelto.
//  La parte visual de cada acertijo está en Scripts/Acertijos/*.cs.
//  Estas clases se prueban automáticamente con Pruebas/PruebasLogica.cs (fuera de Assets).
// ============================================================================================
namespace Leyendas.Logica
{
    // ---------- Candado de ruedas (dígitos, letras o símbolos) y criptex ----------
    public class Ruedas
    {
        public readonly string[] alfabeto;   // símbolos de cada rueda
        public readonly int[] posicion;
        readonly string[] respuesta;

        public Ruedas(string[] alfabeto, string[] respuesta, int[] inicio = null)
        {
            this.alfabeto = alfabeto;
            this.respuesta = respuesta;
            posicion = new int[respuesta.Length];
            if (inicio != null) Array.Copy(inicio, posicion, Math.Min(inicio.Length, posicion.Length));
        }

        public static string[] Letras(string s) { return s.Select(c => c.ToString()).ToArray(); }

        public void Girar(int rueda, int paso)
        {
            int n = alfabeto.Length;
            posicion[rueda] = ((posicion[rueda] + paso) % n + n) % n;
        }

        public string Simbolo(int rueda) { return alfabeto[posicion[rueda]]; }

        public bool Resuelto
        {
            get
            {
                for (int i = 0; i < respuesta.Length; i++)
                    if (!string.Equals(Simbolo(i), respuesta[i], StringComparison.OrdinalIgnoreCase)) return false;
                return true;
            }
        }

        public int Correctas()
        {
            int c = 0;
            for (int i = 0; i < respuesta.Length; i++) if (string.Equals(Simbolo(i), respuesta[i], StringComparison.OrdinalIgnoreCase)) c++;
            return c;
        }
    }

    // ---------- Rompecabezas deslizante n x n ----------
    public class Deslizante
    {
        public readonly int n;
        public readonly int[] celdas; // celdas[pos] = pieza (0..n*n-2), -1 = hueco
        public int movimientos;

        public Deslizante(int n, int semilla, int mezcla = 80)
        {
            this.n = n;
            celdas = new int[n * n];
            for (int i = 0; i < n * n - 1; i++) celdas[i] = i;
            celdas[n * n - 1] = -1;
            // Se mezcla con movimientos válidos desde la solución: siempre tiene solución.
            var r = new Random(semilla);
            int anterior = -1;
            for (int k = 0; k < mezcla || Resuelto; k++)
            {
                var vecinos = Vecinos(Hueco).Where(v => v != anterior).ToList();
                int elegido = vecinos[r.Next(vecinos.Count)];
                anterior = Hueco;
                Intercambiar(elegido);
            }
            movimientos = 0;
        }

        public int Hueco { get { return Array.IndexOf(celdas, -1); } }

        public IEnumerable<int> Vecinos(int p)
        {
            int f = p / n, c = p % n;
            if (f > 0) yield return p - n;
            if (f < n - 1) yield return p + n;
            if (c > 0) yield return p - 1;
            if (c < n - 1) yield return p + 1;
        }

        public bool PuedeMover(int pos) { return Vecinos(Hueco).Contains(pos); }

        public bool Mover(int pos)
        {
            if (!PuedeMover(pos)) return false;
            Intercambiar(pos);
            movimientos++;
            return true;
        }

        void Intercambiar(int pos) { int h = Hueco; celdas[h] = celdas[pos]; celdas[pos] = -1; }

        public bool Resuelto
        {
            get
            {
                for (int i = 0; i < n * n - 1; i++) if (celdas[i] != i) return false;
                return true;
            }
        }
    }

    // ---------- Anillos acoplados (rosetón) ----------
    // Girar el anillo i lo mueve +1 y arrastra al siguiente "acople[i]" pasos. Los acoples forman un ciclo
    // (el último arrastra al primero), así que ningún anillo se mueve solo: hay que combinar giros.
    // Con 3 anillos la combinación es única si 1 + a0·a1·a2 es impar (con 8 posiciones por vuelta).
    public class Anillos
    {
        public readonly int pasos;      // posiciones por vuelta
        public readonly int[] pos;
        readonly int[] acople;

        public Anillos(int anillos, int pasos, int[] acople, int semilla, int mezcla = 12)
        {
            this.pasos = pasos;
            pos = new int[anillos];
            this.acople = acople;
            var r = new Random(semilla);
            // Mezcla aplicando giros válidos: siempre tiene solución.
            while (true)
            {
                for (int k = 0; k < mezcla; k++) Girar(r.Next(anillos), 1);
                if (!Resuelto) break;
            }
        }

        // veces puede ser negativo (girar al revés).
        public void Girar(int i, int veces = 1)
        {
            veces = ((veces % pasos) + pasos) % pasos;
            int sig = (i + 1) % pos.Length;
            for (int v = 0; v < veces; v++)
            {
                pos[i] = (pos[i] + 1) % pasos;
                if (i < acople.Length && acople[i] != 0)
                    pos[sig] = ((pos[sig] + acople[i]) % pasos + pasos) % pasos;
            }
        }

        public bool Resuelto { get { return pos.All(p => p == 0); } }
    }

    // ---------- Flama del candelabro ----------
    // Las velas están unidas por brazos de bronce. La primera vela se prende con la vela de Ana; cada vela nueva
    // se prende con la ÚLTIMA que se encendió y solo si un brazo las une. Hay que encender todas en un solo recorrido.
    public class Flama
    {
        public readonly int velas;
        public readonly List<int[]> brazos;
        public readonly List<int> encendidas = new List<int>();

        public Flama(int velas, IEnumerable<int[]> brazos) { this.velas = velas; this.brazos = brazos.ToList(); }

        // Lee "0-4,1-2,..." (pares de velas unidas).
        public static List<int[]> LeerBrazos(string s)
        {
            return s.Split(',').Select(p => p.Trim().Split('-')).Select(p => new[] { int.Parse(p[0]), int.Parse(p[1]) }).ToList();
        }

        public bool Unidas(int a, int b) { return brazos.Any(x => (x[0] == a && x[1] == b) || (x[0] == b && x[1] == a)); }
        public int Ultima { get { return encendidas.Count > 0 ? encendidas[encendidas.Count - 1] : -1; } }

        // 1 = se encendió, 0 = ya estaba encendida, -1 = la flama no alcanza (no hay brazo desde la última)
        public int Prender(int v)
        {
            if (encendidas.Contains(v)) return 0;
            if (encendidas.Count > 0 && !Unidas(Ultima, v)) return -1;
            encendidas.Add(v);
            return 1;
        }

        // La flama se quedó sin camino: hay velas apagadas pero ninguna unida a la última.
        public bool Atorada
        {
            get
            {
                if (encendidas.Count == 0 || Resuelto) return false;
                for (int v = 0; v < velas; v++) if (!encendidas.Contains(v) && Unidas(Ultima, v)) return false;
                return true;
            }
        }

        public void Apagar() { encendidas.Clear(); }
        public bool Resuelto { get { return encendidas.Count == velas; } }
    }

    // ---------- Partitura de las campanas ----------
    // La melodía está escrita en alturas (0 = grave, 1 = media, 2 = aguda); cada campana suena a una altura.
    // Hay que tocar la melodía completa sin equivocarse; un error la reinicia.
    public class Partitura
    {
        public readonly int[] melodia;
        readonly int[] alturaDe;   // altura de cada campana
        public int entrada;

        public Partitura(int[] melodia, int[] alturaDe) { this.melodia = melodia; this.alturaDe = alturaDe; }

        // 1 = melodía completa, 0 = correcto (falta), -1 = error (vuelve al inicio)
        public int Tocar(int campana)
        {
            if (Resuelto) return 1;
            if (alturaDe[campana] != melodia[entrada]) { entrada = 0; return -1; }
            entrada++;
            return Resuelto ? 1 : 0;
        }

        public bool Resuelto { get { return entrada >= melodia.Length; } }
    }

    // ---------- Lógica de asignación (nieves de Don Chuy) ----------
    public class Asignacion
    {
        public readonly int[] valor;   // valor[vaso] = índice del sabor (-1 = vacío)
        readonly int[] solucion;

        public Asignacion(int[] solucion)
        {
            this.solucion = solucion;
            valor = Enumerable.Repeat(-1, solucion.Length).ToArray();
        }

        public void Siguiente(int vaso) { valor[vaso] = (valor[vaso] + 1) % solucion.Length; }
        public bool Completa { get { return valor.All(v => v >= 0); } }
        public bool SinRepetir { get { return valor.Where(v => v >= 0).Distinct().Count() == valor.Count(v => v >= 0); } }
        public int Correctos { get { int c = 0; for (int i = 0; i < valor.Length; i++) if (valor[i] == solucion[i]) c++; return c; } }
        public bool Resuelto { get { return Correctos == solucion.Length; } }
    }

    // ---------- Nonograma ----------
    public class Nonograma
    {
        public readonly int filas, columnas;
        public readonly bool[] pintada;
        public readonly List<int>[] pistaFilas, pistaColumnas;

        public Nonograma(string[] dibujo) // '#' = pintada
        {
            filas = dibujo.Length; columnas = dibujo[0].Length;
            pintada = new bool[filas * columnas];
            pistaFilas = new List<int>[filas];
            pistaColumnas = new List<int>[columnas];
            for (int f = 0; f < filas; f++) pistaFilas[f] = Pista(Enumerable.Range(0, columnas).Select(c => dibujo[f][c] == '#'));
            for (int c = 0; c < columnas; c++) pistaColumnas[c] = Pista(Enumerable.Range(0, filas).Select(f => dibujo[f][c] == '#'));
        }

        public static List<int> Pista(IEnumerable<bool> linea)
        {
            var p = new List<int>(); int run = 0;
            foreach (bool b in linea) { if (b) run++; else if (run > 0) { p.Add(run); run = 0; } }
            if (run > 0) p.Add(run);
            if (p.Count == 0) p.Add(0);
            return p;
        }

        public void Tocar(int i) { pintada[i] = !pintada[i]; }

        public bool FilaCumple(int f) { return Pista(Enumerable.Range(0, columnas).Select(c => pintada[f * columnas + c])).SequenceEqual(pistaFilas[f]); }
        public bool ColumnaCumple(int c) { return Pista(Enumerable.Range(0, filas).Select(f => pintada[f * columnas + c])).SequenceEqual(pistaColumnas[c]); }

        // Resuelto = todas las filas y columnas cumplen sus números (cualquier dibujo que los cumpla vale).
        public bool Resuelto
        {
            get
            {
                for (int f = 0; f < filas; f++) if (!FilaCumple(f)) return false;
                for (int c = 0; c < columnas; c++) if (!ColumnaCumple(c)) return false;
                return true;
            }
        }
    }

    // ---------- Jarras de agua ----------
    public class Jarras
    {
        public readonly int[] capacidad, litros;
        readonly int[] meta; // litros deseados por jarra (-1 = cualquiera)
        public int movimientos;

        public Jarras(int[] capacidad, int[] inicio, int[] meta)
        {
            this.capacidad = (int[])capacidad.Clone();
            litros = (int[])inicio.Clone();
            this.meta = meta;
        }

        public bool Vaciar(int de, int a)
        {
            if (de == a || litros[de] == 0 || litros[a] == capacidad[a]) return false;
            int pasa = Math.Min(litros[de], capacidad[a] - litros[a]);
            litros[de] -= pasa; litros[a] += pasa;
            movimientos++;
            return true;
        }

        public bool Resuelto
        {
            get
            {
                for (int i = 0; i < litros.Length; i++) if (meta[i] >= 0 && litros[i] != meta[i]) return false;
                return true;
            }
        }
    }

    // ---------- Ordenar (cronología y libros) ----------
    public class Orden
    {
        public readonly List<int> actual; // actual[posición] = índice del elemento
        public int intercambios;

        public Orden(int n, int semilla)
        {
            actual = Enumerable.Range(0, n).ToList();
            var r = new Random(semilla);
            do
            {
                for (int i = n - 1; i > 0; i--) { int j = r.Next(i + 1); int t = actual[i]; actual[i] = actual[j]; actual[j] = t; }
            } while (Resuelto && n > 1);
            intercambios = 0;
        }

        public void Intercambiar(int a, int b)
        {
            if (a == b) return;
            int t = actual[a]; actual[a] = actual[b]; actual[b] = t;
            intercambios++;
        }

        public int EnSuLugar { get { int c = 0; for (int i = 0; i < actual.Count; i++) if (actual[i] == i) c++; return c; } }
        public bool Resuelto { get { return EnSuLugar == actual.Count; } }
    }

    // ---------- Balanza: encontrar la pieza más ligera ----------
    public class Balanza
    {
        public readonly int piezas, maxPesadas;
        public readonly int ligera;
        public int pesadas;

        public Balanza(int piezas, int maxPesadas, int semilla)
        {
            this.piezas = piezas; this.maxPesadas = maxPesadas;
            ligera = new Random(semilla).Next(piezas);
        }

        // -1 = baja la izquierda (la derecha pesa menos) · 0 = equilibrio · 1 = baja la derecha (la izquierda pesa menos)
        public int Pesar(ICollection<int> izquierda, ICollection<int> derecha)
        {
            pesadas++;
            int pi = izquierda.Count * 10 - (izquierda.Contains(ligera) ? 1 : 0);
            int pd = derecha.Count * 10 - (derecha.Contains(ligera) ? 1 : 0);
            return pi > pd ? -1 : pi < pd ? 1 : 0;
        }

        public bool QuedanPesadas { get { return pesadas < maxPesadas; } }
        public bool Elegir(int i) { return i == ligera; }
    }

    // ---------- Azulejos (girar piezas para conectar un canal) ----------
    public class Canal
    {
        // Conexiones: N=1, E=2, S=4, O=8
        public const int N = 1, E = 2, S = 4, O = 8;
        public readonly int filas, columnas;
        public readonly int[] tipo;   // 0 vacío, 1 recto, 2 curva, 3 te, 4 cruz
        public readonly int[] giro;   // 0..3 (cuartos de vuelta en sentido horario)
        public readonly int filaEntrada, filaSalida;
        public int toques;

        static readonly int[] BASE = { 0, N | S, N | E, N | E | S, N | E | S | O };

        public Canal(string[] plano, int filaEntrada, int filaSalida, int semilla)
        {
            // plano: una letra por casilla. '.' vacío, '|' o '-' recto, 'L' curva, 'T' te, '+' cruz.
            filas = plano.Length; columnas = plano[0].Length;
            this.filaEntrada = filaEntrada; this.filaSalida = filaSalida;
            tipo = new int[filas * columnas]; giro = new int[filas * columnas];
            var r = new Random(semilla);
            for (int f = 0; f < filas; f++)
                for (int c = 0; c < columnas; c++)
                {
                    char ch = plano[f][c];
                    tipo[f * columnas + c] = ch == '.' ? 0 : (ch == '|' || ch == '-') ? 1 : ch == 'L' ? 2 : ch == 'T' ? 3 : 4;
                    giro[f * columnas + c] = r.Next(4);
                }
            if (Resuelto) giro[filaEntrada * columnas] = (giro[filaEntrada * columnas] + 1) % 4;
            toques = 0;
        }

        public static int Rotar(int m, int veces)
        {
            for (int v = 0; v < veces; v++) m = ((m << 1) | (m >> 3)) & 15;
            return m;
        }

        public int Conexiones(int i) { return Rotar(BASE[tipo[i]], giro[i]); }

        public void Girar(int i) { giro[i] = (giro[i] + 1) % 4; toques++; }

        // Casillas por donde corre el agua (desde la entrada a la izquierda de filaEntrada).
        public HashSet<int> ConAgua()
        {
            var vis = new HashSet<int>();
            int ini = filaEntrada * columnas;
            if ((Conexiones(ini) & O) == 0) return vis;
            var cola = new Queue<int>(); cola.Enqueue(ini); vis.Add(ini);
            while (cola.Count > 0)
            {
                int p = cola.Dequeue(); int f = p / columnas, c = p % columnas, m = Conexiones(p);
                Probar(m, N, f - 1, c, S, vis, cola);
                Probar(m, S, f + 1, c, N, vis, cola);
                Probar(m, E, f, c + 1, O, vis, cola);
                Probar(m, O, f, c - 1, E, vis, cola);
            }
            return vis;
        }

        void Probar(int m, int lado, int f, int c, int opuesto, HashSet<int> vis, Queue<int> cola)
        {
            if ((m & lado) == 0 || f < 0 || c < 0 || f >= filas || c >= columnas) return;
            int q = f * columnas + c;
            if (vis.Contains(q) || (Conexiones(q) & opuesto) == 0) return;
            vis.Add(q); cola.Enqueue(q);
        }

        public bool Resuelto
        {
            get
            {
                int fin = filaSalida * columnas + columnas - 1;
                return ConAgua().Contains(fin) && (Conexiones(fin) & E) != 0;
            }
        }
    }

    // ---------- Torre de Hanói ----------
    public class Hanoi
    {
        public readonly List<int>[] postes = { new List<int>(), new List<int>(), new List<int>() };
        public readonly int discos;
        public int movimientos;

        public Hanoi(int discos)
        {
            this.discos = discos;
            for (int d = discos - 1; d >= 0; d--) postes[0].Add(d); // 0 = la más chica (arriba)
        }

        public int Arriba(int p) { return postes[p].Count == 0 ? -1 : postes[p][postes[p].Count - 1]; }

        public bool PuedeMover(int de, int a)
        {
            if (de == a || postes[de].Count == 0) return false;
            return postes[a].Count == 0 || Arriba(a) > Arriba(de);
        }

        public bool Mover(int de, int a)
        {
            if (!PuedeMover(de, a)) return false;
            int d = Arriba(de);
            postes[de].RemoveAt(postes[de].Count - 1);
            postes[a].Add(d);
            movimientos++;
            return true;
        }

        public int Minimo { get { return (1 << discos) - 1; } }
        public bool Resuelto { get { return postes[2].Count == discos; } }
    }

    // ---------- Cifrado César ----------
    public static class Cesar
    {
        public const string ABC = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static string Cifrar(string texto, int clave) { return Mover(texto.ToUpperInvariant(), clave); }
        public static string Descifrar(string texto, int clave) { return Mover(texto, -clave); }

        static string Mover(string t, int k)
        {
            var sb = new System.Text.StringBuilder();
            foreach (char ch in t)
            {
                int i = ABC.IndexOf(ch);
                sb.Append(i < 0 ? ch : ABC[((i + k) % 26 + 26) % 26]);
            }
            return sb.ToString();
        }
    }

    // ---------- Caja fuerte de disco ----------
    public class CajaFuerte
    {
        public readonly int numeros;          // números del disco (0..numeros-1)
        public int posicion;
        public int ultimoSentido;             // +1 derecha, -1 izquierda, 0 sin girar
        readonly int[] combinacion;
        readonly int[] sentidos;              // +1 derecha, -1 izquierda
        public readonly List<int> marcados = new List<int>();
        public bool error;

        public CajaFuerte(int numeros, int[] combinacion, int[] sentidos)
        {
            this.numeros = numeros; this.combinacion = combinacion; this.sentidos = sentidos;
        }

        public void Girar(int sentido)
        {
            posicion = ((posicion + sentido) % numeros + numeros) % numeros;
            ultimoSentido = sentido;
        }

        // Marca el número actual. Devuelve false si no coincide con el paso esperado.
        public bool Marcar()
        {
            int k = marcados.Count;
            if (k >= combinacion.Length) return false;
            bool bien = posicion == combinacion[k] && ultimoSentido == sentidos[k];
            if (!bien) { marcados.Clear(); error = true; return false; }
            marcados.Add(posicion);
            ultimoSentido = 0;
            return true;
        }

        public int Pasos { get { return combinacion.Length; } }
        public bool Resuelto { get { return marcados.Count == combinacion.Length; } }
    }

    // ---------- Ruta en un mapa (visitar en orden) ----------
    public class Ruta
    {
        readonly int[] orden;
        public readonly List<int> hecha = new List<int>();

        public Ruta(int[] orden) { this.orden = orden; hecha.Add(orden[0]); }

        public bool Visitar(int lugar)
        {
            if (hecha.Count < orden.Length && orden[hecha.Count] == lugar) { hecha.Add(lugar); return true; }
            hecha.Clear(); hecha.Add(orden[0]);
            return false;
        }

        public bool Resuelto { get { return hecha.Count == orden.Length; } }
    }

    // ---------- Puntuación por tiempo ----------
    public static class Puntaje
    {
        public static int Estrellas(float segundos, float par, int pistas)
        {
            int e = segundos <= par ? 3 : segundos <= par * 2 ? 2 : 1;
            if (pistas > 0) e--;
            return Math.Max(1, e);
        }

        public static int Puntos(float segundos, float par, int errores, int pistas)
        {
            int p = 500 + (int)(Math.Max(0, par * 2 - segundos) * 5) - errores * 25 - pistas * 100;
            return Math.Max(100, p);
        }

        // ---- Puntuación por nivel (tiempo total del lugar, errores y pistas de todos sus acertijos) ----
        public static int EstrellasNivel(float segundos, float par, int pistas)
        {
            int e = segundos <= par ? 3 : segundos <= par * 1.5f ? 2 : 1;
            if (pistas >= 3) e--;
            return Math.Max(1, e);
        }

        public static int PuntosNivel(float segundos, float par, int errores, int pistas, int costoPista)
        {
            int p = 1000 + (int)(Math.Max(0, par * 2 - segundos) * 2) - errores * 20 - pistas * costoPista;
            return Math.Max(200, p);
        }
    }
}
