using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FadeManager : MonoBehaviour
{
    public static FadeManager instancia;

    void Awake()
    {
        instancia = this;
    }

    // Fade OUT = desaparecer (alfa 1 → 0)
    public IEnumerator FadeOut(CanvasGroup grupo, float duracion)
    {
        float tiempo = 0f;
        grupo.alpha = 1f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            grupo.alpha = Mathf.Lerp(1f, 0f, tiempo / duracion);
            yield return null;
        }

        grupo.alpha = 0f;
        grupo.gameObject.SetActive(false);
    }

    // Fade IN = aparecer (alfa 0 → 1)
    public IEnumerator FadeIn(CanvasGroup grupo, float duracion)
    {
        grupo.gameObject.SetActive(true);
        grupo.alpha = 0f;
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            grupo.alpha = Mathf.Lerp(0f, 1f, tiempo / duracion);
            yield return null;
        }

        grupo.alpha = 1f;
    }
}