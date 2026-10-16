// Componente separado en su propio archivo para que Unity lo guarde en escenas y prefabs.
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Leyendas
{
    // Pedazos de papel picado que caen (celebración).
    public class Confeti : MonoBehaviour
    {
        public Vector2 vel; public float giro, vida = 3f;
        RectTransform rt; float t;
        void Awake() { rt = (RectTransform)transform; }
        void Update()
        {
            t += Time.unscaledDeltaTime;
            vel.y -= 520f * Time.unscaledDeltaTime;
            vel.x *= 1 - .6f * Time.unscaledDeltaTime;
            rt.anchoredPosition += vel * Time.unscaledDeltaTime + new Vector2(Mathf.Sin(t * 6 + giro) * 30f * Time.unscaledDeltaTime, 0);
            rt.Rotate(0, 0, giro * Time.unscaledDeltaTime);
            if (t > vida) Destroy(gameObject);
        }

        public static void Lluvia(RectTransform padre, Vector2 origen, int n = 46)
        {
            if (padre == null) return;
            for (int i = 0; i < n; i++)
            {
                var im = F.Img("Confeti", padre, "UI/redondo", Pal.FOLK[i % Pal.FOLK.Length]);
                im.sprite = null; // rectángulo sólido
                var rt = im.rectTransform;
                F.Centro(rt, 0, 0, UnityEngine.Random.Range(10f, 18f), UnityEngine.Random.Range(12f, 22f));
                rt.anchoredPosition = origen;
                var c = im.gameObject.AddComponent<Confeti>();
                float a = UnityEngine.Random.Range(30f, 150f) * Mathf.Deg2Rad, v = UnityEngine.Random.Range(380f, 820f);
                c.vel = new Vector2(Mathf.Cos(a) * v, Mathf.Sin(a) * v);
                c.giro = UnityEngine.Random.Range(-360f, 360f);
                c.vida = UnityEngine.Random.Range(2.2f, 3.4f);
            }
        }
    }
}
