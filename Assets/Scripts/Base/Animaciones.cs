// Animaciones: interpolaciones con corrutinas (Tw) y componentes de animación ambiental.
// Todos los componentes se pueden agregar o ajustar a mano desde el Inspector.
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Leyendas
{
    public static class Tw
    {
        static MonoBehaviour host;
        public static void Host(MonoBehaviour h) { host = h; }

        public static Coroutine Run(IEnumerator e) { return host != null && host.isActiveAndEnabled ? host.StartCoroutine(e) : null; }

        // Curvas
        public static float Lineal(float t) { return t; }
        public static float Sale(float t) { return 1 - Mathf.Pow(1 - t, 3); }
        public static float EntraSale(float t) { return t < .5f ? 4 * t * t * t : 1 - Mathf.Pow(-2 * t + 2, 3) / 2; }
        public static float Rebote(float t) { const float c1 = 1.70158f, c3 = c1 + 1; return 1 + c3 * Mathf.Pow(t - 1, 3) + c1 * Mathf.Pow(t - 1, 2); }

        public static IEnumerator Valor(float dur, Action<float> f, Func<float, float> curva = null)
        {
            curva = curva ?? Sale;
            float t = 0;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                f(curva(Mathf.Clamp01(t / dur)));
                yield return null;
            }
            f(curva(1));
        }

        public static Coroutine Escala(Transform tr, Vector3 a, Vector3 b, float dur, Func<float, float> curva = null)
        {
            if (tr == null) return null;
            tr.localScale = a;
            return Run(Valor(dur, k => { if (tr) tr.localScale = Vector3.LerpUnclamped(a, b, k); }, curva));
        }

        public static Coroutine Alfa(CanvasGroup g, float a, float b, float dur)
        {
            if (g == null) return null;
            g.alpha = a;
            return Run(Valor(dur, k => { if (g) g.alpha = Mathf.Lerp(a, b, k); }, Lineal));
        }

        public static Coroutine Color(Graphic g, Color a, Color b, float dur)
        {
            if (g == null) return null;
            return Run(Valor(dur, k => { if (g) g.color = UnityEngine.Color.Lerp(a, b, k); }, Lineal));
        }

        public static Coroutine Mover(RectTransform rt, Vector2 a, Vector2 b, float dur, Func<float, float> curva = null)
        {
            if (rt == null) return null;
            return Run(Valor(dur, k => { if (rt) rt.anchoredPosition = Vector2.LerpUnclamped(a, b, k); }, curva));
        }

        public static Coroutine Girar(Transform tr, float a, float b, float dur, Func<float, float> curva = null)
        {
            if (tr == null) return null;
            return Run(Valor(dur, k => { if (tr) tr.localRotation = Quaternion.Euler(0, 0, Mathf.LerpUnclamped(a, b, k)); }, curva));
        }

        // Aparece con rebote (paneles, estrellas, objetos nuevos).
        public static Coroutine Aparecer(Transform tr, float dur = .35f, float retraso = 0)
        {
            if (tr == null) return null;
            tr.localScale = Vector3.zero;
            return Run(AparecerC(tr, dur, retraso));
        }
        static IEnumerator AparecerC(Transform tr, float dur, float retraso)
        {
            if (retraso > 0) yield return new WaitForSecondsRealtime(retraso);
            yield return Valor(dur, k => { if (tr) tr.localScale = Vector3.one * k; }, Rebote);
        }

        // Pequeño salto de escala (al recibir un objeto, al acertar).
        public static Coroutine Brinco(Transform tr, float fuerza = .18f, float dur = .3f)
        {
            if (tr == null) return null;
            return Run(Valor(dur, k => { if (tr) tr.localScale = Vector3.one * (1 + fuerza * Mathf.Sin(k * Mathf.PI)); }, Lineal));
        }

        // Sacudida horizontal (al equivocarse).
        public static Coroutine Sacudir(RectTransform rt, float amp = 12, float dur = .4f)
        {
            if (rt == null) return null;
            var base0 = rt.anchoredPosition;
            return Run(Valor(dur, k => { if (rt) rt.anchoredPosition = base0 + new Vector2(Mathf.Sin(k * Mathf.PI * 8) * amp * (1 - k), 0); }, Lineal));
        }

        // Vuelo en arco de un punto a otro (objetos que van a la mochila).
        public static IEnumerator Arco(RectTransform rt, Vector2 a, Vector2 b, float altura, float dur, float escalaFinal)
        {
            var e0 = rt.localScale;
            yield return Valor(dur, k =>
            {
                if (!rt) return;
                var p = Vector2.Lerp(a, b, k) + Vector2.up * altura * 4 * k * (1 - k);
                rt.anchoredPosition = p;
                rt.localScale = Vector3.Lerp(e0, Vector3.one * escalaFinal, k);
                rt.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(k * Mathf.PI) * 18);
            }, EntraSale);
        }

        // Cuenta de un número a otro (puntos).
        public static Coroutine Contar(Text t, int a, int b, float dur, string prefijo = "", string sufijo = "")
        {
            if (t == null) return null;
            return Run(Valor(dur, k => { if (t) t.text = prefijo + Mathf.RoundToInt(Mathf.Lerp(a, b, k)) + sufijo; }, Sale));
        }
    }

}
