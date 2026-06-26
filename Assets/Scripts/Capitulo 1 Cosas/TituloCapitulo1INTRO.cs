using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class IntroUI : MonoBehaviour
{
    public CanvasGroup titulo;
    public CanvasGroup texto;

    // Factor de velocidad del fade. Valores < 1 hacen el fade más lento, > 1 más rápido.
    // Se expone en el inspector para poder ajustarlo sin recompilar.
    [Range(0.1f, 5f)]
    public float fadeSpeed = 0.5f; // Por defecto, fade más lento que el original.

    public string mensaje = "Umbral del dolor";

    // Tiempo de espera antes de iniciar cada fade (en segundos).
    [Header("Delay before each fade")]
    public float delayBeforeFade = 1f;

    void Start()
    {
        StartCoroutine(Secuencia());
    }

    IEnumerator Secuencia()
    {
        // 1. mostrar título (espera antes del fade)
        yield return new WaitForSeconds(delayBeforeFade);
        yield return StartCoroutine(Fade(titulo, 0, 1));

        yield return new WaitForSeconds(2f);

        // 2. mostrar texto (espera antes del fade)
        yield return new WaitForSeconds(delayBeforeFade);
        yield return StartCoroutine(Fade(texto, 0, 1));

        // 3. Mostrar mensaje completo sin efecto typewriter
        // (Se eliminó el uso de TMP_Text según solicitud)
    }

    IEnumerator Fade(CanvasGroup cg, float start, float end)
    {
        float t = 0;

        while (t < 1f)
        {
            // Multiplicamos por fadeSpeed para controlar la velocidad del efecto.
            t += Time.deltaTime * fadeSpeed;
            cg.alpha = Mathf.Lerp(start, end, t);
            yield return null;
        }

        cg.alpha = end;
    }
}