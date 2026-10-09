using UnityEngine;
using System.Collections;

public class PresentacionCapitulo : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    public float tiempoEspera = 1f;
    public float tiempoFadeIn = 1f;
    public float tiempoVisible = 3f;
    public float tiempoFadeOut = 1f;

    void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(Mostrar());
    }

    IEnumerator Mostrar()
    {
        yield return new WaitForSeconds(tiempoEspera);

        canvasGroup.alpha = 0f;

        float t = 0f;

        while (t < tiempoFadeIn)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(t / tiempoFadeIn);
            yield return null;
        }

        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(tiempoVisible);

        t = 0f;

        while (t < tiempoFadeOut)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = 1f - (t / tiempoFadeOut);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
}